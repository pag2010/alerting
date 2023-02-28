using Alerting.Domain.Repositories.Interfaces;
using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Bot;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis;
using Alerting.TelegramBot.Repository;
using Alerting.TelegramBot.StateMachines;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static CacherServiceClient.Cacher;
using StateType = Alerting.TelegramBot.Redis.Enums.StateType;

namespace Telegram.Bot.Services;

public class UpdateHandler : IUpdateHandler
{
    private const int TTL = 120;

    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly CacherClient _cacherClient;
    private readonly IPublisher _publisher;
    private readonly StateMachineFabric _stateMachineFabric;
    private readonly BotInfo _botInfo;
    private readonly IStateMachineRepository _stateMachineRepository;
    private readonly IClientRuleRepository _clientRuleRepository;
    private readonly IClientRepository _clientRepository;
    private readonly string[] _commands = new string[] 
    {
        "/get_info",
        "/start",
        "/register",
        "/unregister",
        "/edit"
    };

    public UpdateHandler(ITelegramBotClient botClient,
                         ILogger<UpdateHandler> logger,
                         CacherClient cacherClient,
                         IPublisher publisher,
                         BotInfo botInfo,
                         IStateMachineRepository stateMachineRepository,
                         IClientRuleRepository clientRuleRepository,
                         IClientRepository clientRepository)
    {
        _botClient = botClient;
        _logger = logger;
        _cacherClient = cacherClient;
        _publisher = publisher;
        _clientRuleRepository = clientRuleRepository;
        _clientRepository = clientRepository;
        _stateMachineFabric = new StateMachineFabric(_botClient, _cacherClient, _publisher, _clientRuleRepository, _clientRepository);
        _botInfo = botInfo;
        _stateMachineRepository = stateMachineRepository;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message }                       => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }                 => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery }           => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            _                                              => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received callback query from: {CallbackQueryFromId}", callbackQuery.From.Id);

        var message = callbackQuery.Message;
        var cachedStates = await _stateMachineRepository.GetStateMachinesAsync();
        var cachedState = cachedStates.SingleOrDefault(state => state.UserId == callbackQuery.From.Id &&
                          state.ChatId == message.Chat.Id);

        //await _botClient.AnswerCallbackQueryAsync(ыя
        //    callbackQueryId: callbackQuery.Id,
        //    text: $"Received {callbackQuery.Data}",
        //    cancellationToken: cancellationToken);

        //await _botClient.SendTextMessageAsync(
        //    chatId: callbackQuery.Message!.Chat.Id,
        //    text: $"Received {callbackQuery.Data}",
        //    cancellationToken: cancellationToken);

        var stateMachine = _stateMachineFabric.GetStateMachine(cachedState);
        var action = stateMachine.Action(callbackQuery, cancellationToken);

        var sentMessage = await action;

        await UpdateOrDeleteStateMachine(cachedState, stateMachine, sentMessage, message, cancellationToken);

        await _botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
    }

    private async Task UpdateOrDeleteStateMachine(StateMachine cachedState,
                                                  AbstractStateMachine stateMachine,
                                                  Message sentMessage,
                                                  Message originalMessage,
                                                  CancellationToken cancellationToken)
    {
        if (cachedState != null)
        {
            if (sentMessage?.MessageId != null)
            {
                cachedState.LastMessageId = sentMessage?.MessageId;
            }
            cachedState.State = stateMachine.StateMachine.State;
            cachedState.Parameters = stateMachine.StateMachine.Parameters;
            await _stateMachineRepository.UpdateAsync(cachedState);
        }

        if (stateMachine != null && stateMachine.StateMachine.State == StateType.Final)
        {
            await stateMachine.Action(originalMessage, cancellationToken);
            await _stateMachineRepository.DeleteAsync(cachedState);
        }
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Text is not { } messageText)
                return;

            var cachedStates = await _stateMachineRepository.GetStateMachinesAsync();
            var cachedState = cachedStates.SingleOrDefault(state => state.UserId == message.From.Id &&
                              state.ChatId == message.Chat.Id &&
                              (message.ReplyToMessage == null || state.LastMessageId == message.ReplyToMessage.MessageId));

            Task<Message> action;
            AbstractStateMachine stateMachine = null;
            var commandText = messageText.Replace(_botInfo.Name, null);
            if (cachedState == null || _commands.Contains(commandText))
            {
                if (cachedState != null)
                {
                    await _stateMachineRepository.DeleteAsync(cachedState);
                    cachedState = null;
                }

                action = commandText switch
                {
                    "/get_info" => GetInfoHandler(_botClient, message, cancellationToken, _cacherClient, _stateMachineRepository),
                    "/start" => Usage(_botClient, message, cancellationToken),
                    "/register" => RegisterHandler(_botClient, message, cancellationToken, _publisher, _stateMachineRepository),
                    "/unregister" => UnregisterHandler(_botClient, message, cancellationToken, _publisher, _stateMachineRepository),
                    "/edit" => EditHandler(_botClient, message, cancellationToken, _publisher, _stateMachineRepository, _clientRuleRepository, _clientRepository),
                    _ => Usage(_botClient, message, cancellationToken)
                };
            }
            else
            {
                stateMachine = _stateMachineFabric.GetStateMachine(cachedState);
                action = stateMachine.Action(message, cancellationToken);
            }

            var sentMessage = await action;
            await UpdateOrDeleteStateMachine(cachedState, stateMachine, sentMessage, message, cancellationToken);

            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage?.MessageId);

        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error BotOnMessageReceived: {errorMessage}", ex.Message);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            string usage = "Команды:\n" +
                           "/get_info - Получить информацию по клиенту.\n" +
                           "/register - Зарегистрировать клиента.\n" +
                           "/unregister - Разрегистрировать клиента.\n"+
                           "/edit - Изменить клиента.";

            return await botClient.SendTextMessageAsync(
                         chatId: message.Chat.Id,
                         text: usage,
                         replyMarkup: new ReplyKeyboardRemove(),
                         cancellationToken: cancellationToken);
        }

        static async Task<Message> GetInfoHandler(ITelegramBotClient botClient,
                                                          Message message,
                                                          CancellationToken cancellationToken,
                                                          CacherClient cacherClient,
                                                          IStateMachineRepository stateMachineRepository)
        {
            var state = new GetInfoStateMachine(botClient,
                                                cacherClient,
                                                message.Chat.Id,
                                                message.From.Id,
                                                lastMessageId: null);

            var botMessage = await state.Action(message, cancellationToken);
            state.StateMachine.LastMessageId = botMessage.MessageId;

            await stateMachineRepository.InsertAsync(state.StateMachine, TTL);

            return botMessage;
        }

        static async Task<Message> RegisterHandler(ITelegramBotClient botClient,
                                                   Message message,
                                                   CancellationToken cancellationToken,
                                                   IPublisher publisher,
                                                   IStateMachineRepository stateMachineRepository)
        {
            var state = new RegistrationStateMachine(publisher,
                                                     botClient,
                                                     message.Chat.Id,
                                                     message.From.Id,
                                                     null);

            var botMessage = await state.Action(message, cancellationToken);
            state.StateMachine.LastMessageId = botMessage.MessageId;

            await stateMachineRepository.InsertAsync(state.StateMachine, 300);

            return botMessage;
        }

        static async Task<Message> UnregisterHandler(ITelegramBotClient botClient,
                                                   Message message,
                                                   CancellationToken cancellationToken,
                                                   IPublisher publisher,
                                                   IStateMachineRepository stateMachineRepository)
        {
            var state = new UnregistrationStateMachine(publisher,
                                                     botClient,
                                                     message.Chat.Id,
                                                     message.From.Id,
                                                     null);

            var botMessage = await state.Action(message, cancellationToken);
            state.StateMachine.LastMessageId = botMessage.MessageId;

            await stateMachineRepository.InsertAsync(state.StateMachine, TTL);

            return botMessage;
        }

        static async Task<Message> EditHandler(ITelegramBotClient botClient,
                                               Message message,
                                               CancellationToken cancellationToken,
                                               IPublisher publisher,
                                               IStateMachineRepository stateMachineRepository,
                                               IClientRuleRepository clientRuleRepository,
                                               IClientRepository clientRepository)
        {
            var state = new EditStateMachine(publisher,
                                             botClient,
                                             clientRuleRepository,
                                             clientRepository,
                                             message.Chat.Id,
                                             message.From.Id,
                                             null);

            var botMessage = await state.Action(message, cancellationToken);
            state.StateMachine.LastMessageId = botMessage.MessageId;

            await stateMachineRepository.InsertAsync(state.StateMachine, TTL);

            return botMessage;
        }
    }

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
