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
    public class TelegramBot
    {
        private ILogger logger;
        private List<long> chatIds;
        private Dictionary<long, ChatCurrentActionInfo> currentActions;
        private ITelegramBotClient telegramBotClient;


        private static TelegramBot? instance;
        public static TelegramBot Instance
        {
            get
            {
                if (instance == null)
                    instance = new TelegramBot();

                return instance;
            }
        }

        private TelegramBot()
        {
            var token = ConfigurationService.TelegramBotConfiguration.Token;

            if (String.IsNullOrWhiteSpace(token))
                throw new Exception("TelegramBotConfiguration - Token - was null or empty/");

            telegramBotClient = new TelegramBotClient(token!);

            logger = LogManager.GetCurrentClassLogger();
            chatIds = AppDataHelper.ReadTelegramBotChatIds();
            currentActions = new Dictionary<long, ChatCurrentActionInfo>();
        }


        public void StartRecieving(CancellationToken cancellationToken)
        {
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

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
           Instance.logger.Error(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


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

            if (Instance.currentActions.TryGetValue(chatId, out var actionInfo))
            {
                if (actionInfo.PublicationId == null)
                    return;

                var publication = PublicationManager.GetPublicationEntity(actionInfo.PublicationId);
                if (publication == null)
                    return;

                switch (actionInfo.Action)
                {
                    case InlineKeyboardAction.ChangeTitle:
                        publication.Title = message.Text;
                        await SendPublicationEntity(publication);
                        Instance.currentActions.Remove(chatId);
                        break;
                    case InlineKeyboardAction.ChangeSource:
                        publication.Source = message.Text;
                        await SendPublicationEntity(publication);
                        Instance.currentActions.Remove(chatId);
                        break;
                    default:
                        break;
                }

                return;
            }

            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Привет, манагер лучшей группы в мире!");
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat, "/help чтобы узнать, что тут к чему");
        }

        private static async Task HandleCallbackQueryUpdate(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message?.Chat.Id;
            if (chatId == null)
                return;

            var callbackData = callbackQuery.Data;
            if (String.IsNullOrWhiteSpace(callbackData))
                return;

            var publicationDto = JsonConvert.DeserializeObject<PublicationEntityCallBackDto>(callbackData);

            if (Instance.currentActions.ContainsKey(chatId.Value))
                Instance.currentActions.Remove(chatId.Value);

            if (string.IsNullOrWhiteSpace(publicationDto!.PublicationId))
            {
                await botClient.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            var publication = PublicationManager.GetPublicationEntity(publicationDto!.PublicationId!);
            if (publication == null)
            {
                await botClient.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            var action = InlineKeyboardActionManager.Obtain(publicationDto!.Action!);
            if (action == null)
                return;

            var isActionComplete = false;

            if (InlineKeyboardActionManager.IsActionRequiresCallbackText(action.Value))
            {
                Instance.currentActions.Add(chatId.Value, new ChatCurrentActionInfo()
                {
                    PublicationId = publicationDto.PublicationId,
                    Action = action.Value
                });

                isActionComplete = true;
            }
            else
            {
                switch (action)
                {
                    case InlineKeyboardAction.NotPublicate:
                        PublicationManager.DeletePublication(publication);
                        isActionComplete = true;
                        break;
                    case InlineKeyboardAction.Publicate:
                        isActionComplete = PublicationManager.PublicatePublication(publication);
                        break;
                    case InlineKeyboardAction.ManualProcessing:
                        isActionComplete = PublicationManager.ManualProcessingPublication(publication);
                        break;
                    case InlineKeyboardAction.MoveToAlbum:
                        isActionComplete = PublicationManager.MoveToAlbum(publication);
                        break;
                }
            }

            var responseText = ObtainActionResponseText(publication, action.Value, isActionComplete);
            if (responseText == null)
                return;

            await botClient.SendTextMessageAsync(chatId, responseText);
        }


        private static string? ObtainActionResponseText(PublicationEntity publication, InlineKeyboardAction action, bool isActionComplete)
        {
            switch(action)
            {
                case InlineKeyboardAction.Publicate:
                    if(isActionComplete)
                        return $"Публикация:\n" +
                        $"{publication.SocialNetwork} - {publication.Author}\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Дата публикации: {publication.PublicationDateTime?.ToString("dd.MM HH:mm")}\n" +
                        $"Источник: {publication.Source}\n" +
                        $"отправлена на публикацию.";
                    else
                        return $"Публикация:\n" +
                        $"{publication.SocialNetwork} - {publication.Author}\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Источник: {publication.Source}\n" +
                        $"не удалось опубликовать.";

                case InlineKeyboardAction.MoveToAlbum:
                    if (isActionComplete)
                        return $"Публикация:\n" +
                        $"{publication.SocialNetwork} - {publication.Author}\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Источник: {publication.Source}\n" +
                        $"отправлена в альбом.";
                    else
                        return $"Публикация:\n" +
                        $"{publication.SocialNetwork} - {publication.Author}\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Источник: {publication.Source}\n" +
                        $"не удалось отправить в альбом.";

                case InlineKeyboardAction.NotPublicate:
                    return $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n" +
                    $"удалена.";

                case InlineKeyboardAction.ManualProcessing:
                    if(isActionComplete)
                        return $"Публикация:\n" +
                        $"{publication.SocialNetwork} - {publication.Author}\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Источник: {publication.Source}\n" +
                        $"отправлена в каталог ручной обработки.";
                    else
                        return $"Публикация:\n" +
                        $"{publication.SocialNetwork} - {publication.Author}\n" +
                        $"Заголовок: {publication.Title}\n" +
                        $"Источник: {publication.Source}\n" +
                        $"не удалось отправить в каталог ручной обработки.";

                case InlineKeyboardAction.ChangeTitle:
                    return $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n\n" +
                    $"Введите новое название:";

                case InlineKeyboardAction.ChangeSource:
                    return $"Публикация:\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n\n" +
                    $"Введите новый источник:";

                default: 
                    return null;
            }
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
                        var keyboard = GetStandartPublicationKeyboardMurkup(publication.Id);
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

        private static InlineKeyboardMarkup GetStandartPublicationKeyboardMurkup(string publicationId)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("✅")
                    {
                        CallbackData = JsonConvert.SerializeObject(new PublicationEntityCallBackDto{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionManager.Obtain(InlineKeyboardAction.Publicate)
                        })
                    },
                    new InlineKeyboardButton("🏞")
                    {
                        CallbackData = JsonConvert.SerializeObject(new PublicationEntityCallBackDto{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionManager.Obtain(InlineKeyboardAction.MoveToAlbum)
                        })
                    },
                    new InlineKeyboardButton("✏")
                    {
                        CallbackData = JsonConvert.SerializeObject(new PublicationEntityCallBackDto{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionManager.Obtain(InlineKeyboardAction.ChangeTitle)
                        })
                    },
                    new InlineKeyboardButton("ℹ")
                    {
                        CallbackData = JsonConvert.SerializeObject(new PublicationEntityCallBackDto{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionManager.Obtain(InlineKeyboardAction.ChangeSource)
                        })
                    },
                    new InlineKeyboardButton("🔁")
                    {
                        CallbackData = JsonConvert.SerializeObject(new PublicationEntityCallBackDto{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionManager.Obtain(InlineKeyboardAction.ManualProcessing)
                        })
                    },
                    new InlineKeyboardButton("❎")
                    {
                        CallbackData = JsonConvert.SerializeObject(new PublicationEntityCallBackDto{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionManager.Obtain(InlineKeyboardAction.NotPublicate)
                        })
                    }
                }
            });
        }
    }
}
