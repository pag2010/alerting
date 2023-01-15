using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis;
using MassTransit.Internals.Caching;
using Microsoft.Extensions.Logging;
using Redis.OM;
using Redis.OM.Searching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using static CacherServiceClient.Cacher;
using StateMachine = Alerting.TelegramBot.Dialog.StateMachine;
using StateType = Alerting.TelegramBot.Redis.Enums.StateType;

namespace Telegram.Bot.Services;

public class UpdateHandler : IUpdateHandler
{
    private const int TTL = 120;

    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly CacherClient _cacherClient;
    private readonly IPublisher _publisher;
    private readonly RedisCollection<StateMachine> _cachedStateMachines;
    private readonly StateMachineFabric _stateMachineFabric;

    public UpdateHandler(ITelegramBotClient botClient,
                         ILogger<UpdateHandler> logger,
                         CacherClient cacherClient,
                         IPublisher publisher,
                         RedisConnectionProvider provider)
    {
        _botClient = botClient;
        _logger = logger;
        _cacherClient = cacherClient;
        _publisher = publisher;
        _cachedStateMachines =
                (RedisCollection<StateMachine>)provider.RedisCollection<StateMachine>();
        _stateMachineFabric = new StateMachineFabric(_botClient, _cacherClient);
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
            { InlineQuery: { } inlineQuery }               => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _                                              => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Text is not { } messageText)
                return;

            var dialogStates = await _cachedStateMachines.ToListAsync();
            var dialogState = dialogStates.SingleOrDefault(state => state.UserId == message.From.Id &&
                              state.ChatId == message.Chat.Id &&
                              (message.ReplyToMessage == null || state.LastMessageId == message.ReplyToMessage.MessageId));
            
            Task<Message> action;
            StateMachine stateMachine = null;

            if (dialogState == null)
            {
                action = messageText.Split(' ')[0] switch
                {
                    "/get_info" => GetInfoHandler(_botClient, message, cancellationToken, _cacherClient, _cachedStateMachines),
                    "/start" => Usage(_botClient, message, cancellationToken),
                    "/register" => RegisterHandler(_botClient, message, cancellationToken, _publisher, _cachedStateMachines),
                    _ => Usage(_botClient, message, cancellationToken)
                };
            }
            else
            {
                stateMachine = _stateMachineFabric.GetStateMachine(dialogState);
                action = stateMachine.Action(message, cancellationToken);
                await _cachedStateMachines.UpdateAsync(dialogState);
            }

            var sentMessage = await action;

            if (stateMachine != null && stateMachine.State == StateType.Final)
            {
                await _cachedStateMachines.DeleteAsync(dialogState);
            }

            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage?.MessageId);

        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error BotOnMessageReceived: {errorMessage}", ex.Message);
        }

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);
                
            // Simulate longer running task
            await Task.Delay(500, cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Removing keyboard",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup RequestReplyKeyboard = new(
                new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Who or Where are you?",
                replyMarkup: RequestReplyKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            const string usage = "Команды:\n" +
                                 "/get_info {guid} {guid} - Получить информацию по клиенту. Список guid указывается через пробел\n";

            return await botClient.SendTextMessageAsync(
                         chatId: message.Chat.Id,
                         text: usage,
                         replyMarkup: new ReplyKeyboardRemove(),
                         cancellationToken: cancellationToken);
        }

        static async Task<IEnumerable<Message>> StartInlineQuery(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            return new List<Message>(){await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Press the button to start Inline Query",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken)
            };
        }

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
        static Task<Message> FailingHandler(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.

        static async Task<Message> GetInfoHandler(ITelegramBotClient botClient,
                                                          Message message,
                                                          CancellationToken cancellationToken,
                                                          CacherClient cacherClient,
                                                          RedisCollection<StateMachine> dialogStateMachine)
        { 
            var botMessage = await botClient.SendTextMessageAsync(
                             chatId: message.Chat.Id,
                             text: "Укажите guid",
                             replyMarkup: new ForceReplyMarkup(),
                             cancellationToken: cancellationToken);

            var state = new GetInfoStateMachine(botClient,
                                                cacherClient,
                                                message.Chat.Id,
                                                message.From.Id,
                                                botMessage.MessageId);
            
            await dialogStateMachine.InsertAsync(state, TimeSpan.FromSeconds(TTL));

            return botMessage;
        }

        static async Task<Message> RegisterHandler(ITelegramBotClient botClient,
                                                                Message message,
                                                                CancellationToken cancellationToken,
                                                                IPublisher publisher,
                                                                RedisCollection<StateMachine> dialogStateMachine)
        {
            try
            {
                var botMessage = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Укажите Имя",
                            replyMarkup: new ForceReplyMarkup(),
                            cancellationToken: cancellationToken);

                var state = new RegistrationStateMachine(publisher,
                                                         botClient,
                                                         message.Chat.Id,
                                                         message.From.Id,
                                                         botMessage.MessageId);

                await dialogStateMachine.InsertAsync(state, TimeSpan.FromSeconds(TTL));

                return botMessage;
            }
            catch
            {
                throw;
            }
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            cacheTime: 0,
            isPersonal: true,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chatId: chosenInlineResult.From.Id,
            text: $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }

    #endregion

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
