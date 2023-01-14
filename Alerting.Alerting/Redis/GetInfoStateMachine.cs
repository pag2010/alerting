using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using CacherServiceClient;
using Alerting.TelegramBot.Redis.Enums;
using Redis.OM.Modeling;

namespace Alerting.TelegramBot.Dialog
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "TelegramBotStateMachine" })]
    public class GetInfoStateMachine : StateMachine
    {
        private ITelegramBotClient _botClient;
        private Cacher.CacherClient _cacherClient;

        public GetInfoStateMachine(ITelegramBotClient botClient,
                                   Cacher.CacherClient cacherClient,
                                   long chatId,
                                   long userId,
                                   long lastMessageId)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
            ChatId = chatId;
            UserId = userId;
            LastMessageId = lastMessageId;
            State = StateType.Initial;
            Type = StateMachineType.GetInfo;
        }

        public GetInfoStateMachine(ITelegramBotClient botClient,
                                   Cacher.CacherClient cacherClient, 
                                   StateMachine stateMachine)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
            Id = stateMachine.Id;
            ChatId = stateMachine.ChatId;
            UserId = stateMachine.UserId;
            LastMessageId = stateMachine.LastMessageId;
            State = stateMachine.State;
            Type = StateMachineType.GetInfo;
        }

        public override async Task<Message> Action(Message message,
                                          CancellationToken cancellationToken)
        {
            Guid id;

            if (Guid.TryParse(message.Text, out id))
            {
                var result = await _cacherClient.GetClientInfoAsync(new ClientInfoRequest
                {
                    Id = id.ToString()
                });

                State = StateType.Final;

                return await _botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                       text: JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                       cancellationToken: cancellationToken);
            }
            else
            {
                return await _botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                       text: $"Укажите верный GUID",
                       replyMarkup: new ForceReplyMarkup(),
                       cancellationToken: cancellationToken);
            }
        }
    }
}
