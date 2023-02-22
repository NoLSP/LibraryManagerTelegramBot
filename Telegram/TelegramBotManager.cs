using Newtonsoft.Json;
using NLog;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpecialLibraryBot.Telegram
{
    public class TelegramBotManager
    {
        private ILogger logger;
        private List<long> chatIds;
        private ITelegramBotClient telegramBotClient;


        private static TelegramBotManager? instance;
        public static TelegramBotManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new TelegramBotManager();

                return instance;
            }
        }

        private TelegramBotManager()
        {
            var token = ConfigurationService.TelegramBotConfiguration.Token;

            if (String.IsNullOrWhiteSpace(token))
                throw new Exception("TelegramBotConfiguration - Token - was null or empty/");

            telegramBotClient = new TelegramBotClient(token!);

            logger = LogManager.GetCurrentClassLogger();
            chatIds = AppDataHelper.ReadTelegramBotChatIds();
        }


        //Main

        public void StartRecieving()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            telegramBotClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    var message = update.Message;
                    if (message == null)
                        return;

                    await HandleMessageUpdate(botClient, message);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    var callbackQuery = update.CallbackQuery;
                    if (callbackQuery == null)
                        return;

                    await HandleCallbackQueryUpdate(botClient, callbackQuery);
                }
            }
            catch (Exception ex)
            {
                Instance.logger.Error(ex);
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
           Instance.logger.Error(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        //Handlers

        private static async Task HandleMessageUpdate(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;

            if (!Instance.chatIds.Contains(chatId))
            {
                AppDataHelper.SaveTelegramChatId(chatId);
                Instance.chatIds.Add(chatId);
            }

            if (message.Text == null)
                return;

            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Привет, манагер лучшей группы в мире!");
                return;
            }

            var userCallbackData = new UserCallbackData
            {
                ChatId = chatId,
                Message = message.Text
            };

            await InlineKeyboardActionManager.HndleCurrentAction(userCallbackData);

            //await botClient.SendTextMessageAsync(message.Chat, "/help чтобы узнать, что тут к чему");
        }

        private static async Task HandleCallbackQueryUpdate(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message?.Chat.Id;
            if (chatId == null)
                return;

            var callbackData = callbackQuery.Data;
            if (String.IsNullOrWhiteSpace(callbackData))
                return;

            var userCallbackData = JsonConvert.DeserializeObject<UserCallbackData>(callbackData);
            userCallbackData!.ChatId = chatId;

            await InlineKeyboardActionManager.HndleAction(userCallbackData!.Action!, userCallbackData!);
        }


        //Public methods

        public static async Task SendTextMessageAsync(long chatId, string responseText)
        {
            await Instance.telegramBotClient.SendTextMessageAsync(chatId, responseText);
        }

        public static async Task<bool> SendPublicationEntity(PublicationEntity publication)
        {
            try
            {
                var messageText = $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: \"{publication.Title}\"\n" +
                    $"Источник: {publication.Source}";

                foreach (var chatId in Instance.chatIds)
                {
                    using (var fileStream = new FileStream(publication.ImageFilePath, FileMode.Open))
                    {
                        var file = new InputOnlineFile(fileStream);
                        var keyboard = InlineKeyboardActionManager.GetStandartPublicationKeyboardMurkup(publication.Id);
                        await Instance.telegramBotClient.SendPhotoAsync(chatId, file, messageText, replyMarkup: keyboard);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Instance.logger.Error(ex);
                return false;
            }
        }

        public static async Task SendException(string action, string message)
        {
            foreach(var chatId in Instance.chatIds)
                await Instance.telegramBotClient.SendTextMessageAsync(chatId, $"❗Возникла ошибка❗\n\n{action}\n\n{message}");
        }
    }
}
