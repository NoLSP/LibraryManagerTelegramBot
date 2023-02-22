using Newtonsoft.Json;
using NLog;
using NLog.LayoutRenderers;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.VK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using VkNet.Model;
using static System.Collections.Specialized.BitVector32;

namespace SpecialLibraryBot.Telegram
{
    public static class InlineKeyboardActionManager
    {
        private static Dictionary<long, InlineKeyboardAction> CurrentActions = new Dictionary<long, InlineKeyboardAction>();
        private static Dictionary<string, Func<UserCallbackData, Task>> ActionsByName;

        static InlineKeyboardActionManager()
        {
            ActionsByName = typeof(InlineKeyboardActionManager)
                .GetMethods()
                .Where(x => x.GetCustomAttributes(false).Any(x => x is InlineKeyboardActionAttribute))
                .ToDictionary(x => (x.GetCustomAttributes(false).First(x => x is InlineKeyboardActionAttribute) as InlineKeyboardActionAttribute)!.ActionName, x => x.CreateDelegate<Func<UserCallbackData, Task>>());
        }

        public static InlineKeyboardMarkup GetStandartPublicationKeyboardMurkup(string publicationId)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("✅")
                    {
                        CallbackData = JsonConvert.SerializeObject(new UserCallbackData{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.Publicate
                        })
                    },
                    new InlineKeyboardButton("🏞")
                    {
                        CallbackData = JsonConvert.SerializeObject(new UserCallbackData{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.MoveToAlbum
                        })
                    },
                    new InlineKeyboardButton("✏")
                    {
                        CallbackData = JsonConvert.SerializeObject(new UserCallbackData{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.ChangeTitle
                        })
                    },
                    new InlineKeyboardButton("ℹ")
                    {
                        CallbackData = JsonConvert.SerializeObject(new UserCallbackData{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.ChangeSource
                        })
                    },
                    new InlineKeyboardButton("🔁")
                    {
                        CallbackData = JsonConvert.SerializeObject(new UserCallbackData{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.ManualProcessing
                        })
                    },
                    new InlineKeyboardButton("❎")
                    {
                        CallbackData = JsonConvert.SerializeObject(new UserCallbackData{
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.NotPublicate
                        })
                    }
                }
            });
        }

        private static void CloseAction(long chatId)
        {
            if(CurrentActions.ContainsKey(chatId))
                CurrentActions.Remove(chatId);
        }

        public static async Task HndleAction(string actionName, UserCallbackData data)
        {
            if(ActionsByName.TryGetValue(actionName, out var action))
            {
                await action(data);
            }
            else
            {
                //todo
                var here = "";
            }
        }

        public static async Task HndleCurrentAction(UserCallbackData data)
        {
            if (CurrentActions.TryGetValue(data.ChatId!.Value, out var keyboardAction))
            {
                data.PublicationId = keyboardAction.PublicationId;
                data.Action = keyboardAction.Type;
                if (ActionsByName.TryGetValue(keyboardAction.Type, out var action))
                {
                    await action(data);
                }
            }
        }


        [InlineKeyboardAction(InlineKeyboardActionType.Publicate)]
        public static async Task PublicatePostAction(UserCallbackData data)
        {
            var chatId = data.ChatId!.Value;
            var publication = PublicationManager.GetPublicationEntity(data.PublicationId!);
            if (publication == null)
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if(CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            var messageText = (string?)null;

            if (PublicationManager.PublicatePublication(publication))
            {
                messageText = $"Публикация:\n" +
                $"{publication.SocialNetwork} - {publication.Author}\n" +
                $"Заголовок: {publication.Title}\n" +
                $"Дата публикации: {publication.PublicationDateTime?.ToString("dd.MM HH:mm")}\n" +
                $"Источник: {publication.Source}\n" +
                $"отправлена на публикацию.";
            }
            else
            {
                messageText = $"Публикация:\n" +
                $"{publication.SocialNetwork} - {publication.Author}\n" +
                $"Заголовок: {publication.Title}\n" +
                $"Дата публикации: {publication.PublicationDateTime?.ToString("dd.MM HH:mm")}\n" +
                $"не удалось опубликовать.";
            }

            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.ChangeTitle)]
        public static async Task ChangeTitleAction(UserCallbackData data)
        {
            var chatId = data.ChatId!.Value;
            var publication = PublicationManager.GetPublicationEntity(data.PublicationId!);
            if (publication == null)
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (!CurrentActions.TryGetValue(chatId, out var action))
            {
                CurrentActions.Add(chatId, new InlineKeyboardAction(publication.Id, chatId, InlineKeyboardActionType.ChangeTitle));

                var messageText = $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n\n" +
                    $"Введите новое название:";
                await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
            }
            else
            {
                publication.Title = data.Message!;
                await TelegramBotManager.SendPublicationEntity(publication);
                CloseAction(chatId);
            }
        }

        [InlineKeyboardAction(InlineKeyboardActionType.ChangeSource)]
        public static async Task ChangeSourceAction(UserCallbackData data)
        {
            var chatId = data.ChatId!.Value;
            var publication = PublicationManager.GetPublicationEntity(data.PublicationId!);
            if (publication == null)
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (!CurrentActions.TryGetValue(chatId, out var action))
            {
                CurrentActions.Add(chatId, new InlineKeyboardAction(publication.Id, chatId, InlineKeyboardActionType.ChangeSource));

                var messageText = $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n\n" +
                    $"Введите новый источник:";
                await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
            }
            else
            {
                publication.Source = data.Message!;
                await TelegramBotManager.SendPublicationEntity(publication);
                CloseAction(chatId);
            }
        }

        [InlineKeyboardAction(InlineKeyboardActionType.NotPublicate)]
        public static async Task NotPublicateAction(UserCallbackData data)
        {
            var chatId = data.ChatId!.Value;
            var publication = PublicationManager.GetPublicationEntity(data.PublicationId!);
            if (publication == null)
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            PublicationManager.DeletePublication(publication, true);

            var messageText = $"Публикация:\n" +
                $"{publication.SocialNetwork} - {publication.Author}\n" +
                $"Заголовок: {publication.Title}\n" +
                $"Источник: {publication.Source}\n" +
                $"удалена.";
            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.ManualProcessing)]
        public static async Task ManualProcessingAction(UserCallbackData data)
        {
            var chatId = data.ChatId!.Value;
            var publication = PublicationManager.GetPublicationEntity(data.PublicationId!);
            if (publication == null)
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            var messageText = (string?)null;

            if (PublicationManager.ManualProcessingPublication(publication))
            {
                messageText = $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n" +
                    $"отправлена в каталог ручной обработки.";
            }
            else
            {
                messageText = $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n" +
                    $"не удалось отправить в каталог ручной обработки.";
            }

            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.MoveToAlbum)]
        public static async Task MoveToAlbumAction(UserCallbackData data)
        {
            var chatId = data.ChatId!.Value;
            var publication = PublicationManager.GetPublicationEntity(data.PublicationId!);
            if (publication == null)
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            var messageText = (string?)null;

            if (PublicationManager.MoveToAlbum(publication))
            {
                messageText = $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n" +
                    $"отправлена в альбом.";
            }
            else
            {
                messageText = $"Публикация:\n" +
                    $"{publication.SocialNetwork} - {publication.Author}\n" +
                    $"Заголовок: {publication.Title}\n" +
                    $"Источник: {publication.Source}\n" +
                    $"не удалось отправить в альбом.";
            }

            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }
    }
}
