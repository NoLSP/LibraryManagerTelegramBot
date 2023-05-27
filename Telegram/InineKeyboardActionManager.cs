using Newtonsoft.Json;
using NLog;
using NLog.LayoutRenderers;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.Services.SchedulerTaskService;
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

namespace SpecialLibraryBot.Telegram
{
    public static class InlineKeyboardActionManager
    {
        private static Dictionary<long, InlineKeyboardAction> CurrentActions = new Dictionary<long, InlineKeyboardAction>();
        private static Dictionary<string, Func<UserCallbackData, Task>> ActionsByName;
        private static Dictionary<string, UserCallbackData> UserCallbacksByGuid = new Dictionary<string, UserCallbackData>();

        static InlineKeyboardActionManager()
        {
            ActionsByName = typeof(InlineKeyboardActionManager)
                .GetMethods()
                .Where(x => x.GetCustomAttributes(false).Any(x => x is InlineKeyboardActionAttribute))
                .ToDictionary(x => (x.GetCustomAttributes(false).First(x => x is InlineKeyboardActionAttribute) as InlineKeyboardActionAttribute)!.ActionName, x => x.CreateDelegate<Func<UserCallbackData, Task>>());
        }

        public static InlineKeyboardMarkup GetStandartPublicationKeyboardMurkup(long chatId, string publicationId)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("✅")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.Publicate,
                        })
                    },
                    new InlineKeyboardButton("🏞")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.MoveToAlbum,
                        })
                    },
                    new InlineKeyboardButton("✏")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.ChangeTitle,
                        })
                    },
                    new InlineKeyboardButton("ℹ")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.ChangeSource,
                        })
                    },
                    new InlineKeyboardButton("🔁")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.ManualProcessing,
                        })
                    },
                    new InlineKeyboardButton("❎")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            PublicationId = publicationId,
                            Action = InlineKeyboardActionType.NotPublicate,
                        })
                    }
                }
            });
        }

        public static InlineKeyboardMarkup GetAuthorsListKeyboardMurkup(long chatId)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("DeviantArt")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            Action = InlineKeyboardActionType.AuthorsList,
                            SocialNetwork = "DeviantArt",
                        })
                    }
                }
            });
        }

        public static InlineKeyboardMarkup GetAuthorsListEdittingKeyboardMurkup(long chatId, string socialNetwork)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("Редактировать")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            Action = InlineKeyboardActionType.AuthorsListEdit,
                            SocialNetwork = socialNetwork,
                        })
                    },
                    new InlineKeyboardButton("Добавить")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            Action = InlineKeyboardActionType.AuthorsListAdd,
                            SocialNetwork = socialNetwork,
                        })
                    }
                }
            });
        }

        public static InlineKeyboardMarkup GetAuthorEdittingKeyboardMurkup(long chatId, string socialNetwork, string author)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("Изменить")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            Action = InlineKeyboardActionType.AuthorEdit,
                            SocialNetwork = socialNetwork,
                            Author = author
                        })
                    },
                    new InlineKeyboardButton("Удалить")
                    {
                        CallbackData = ObtainUserCallbackData(new UserCallbackData{
                            ChatId = chatId,
                            Action = InlineKeyboardActionType.AuthorDelete,
                            SocialNetwork = socialNetwork,
                            Author = author
                        })
                    }
                }
            });
        }

        private static string ObtainUserCallbackData(UserCallbackData userCallbackData)
        {
            var guid = Guid.NewGuid().ToString();
            UserCallbacksByGuid.Add(guid, userCallbackData);
            return guid;
        }

        private static void CloseAction(long chatId)
        {
            if(CurrentActions.ContainsKey(chatId))
                CurrentActions.Remove(chatId);
        }

        public static async Task HandleAction(string callbackGuid)
        {
            if (!UserCallbacksByGuid.TryGetValue(callbackGuid, out var callbackData))
                return;

            await HandleAction(callbackData.Action!, callbackData);

            UserCallbacksByGuid.Remove(callbackGuid);
        }

        public static async Task HandleAction(long chatId, string message)
        {
            if (String.IsNullOrWhiteSpace(message))
                return;

            var data = new UserCallbackData
            {
                ChatId = chatId,
                Message = message
            };

            if (data.Message.StartsWith("/"))
            {
                await HandleAction(data.Message.Substring(1), data);
            }
            else if (CurrentActions.TryGetValue(data.ChatId, out var keyboardAction))
            {
                data.PublicationId = keyboardAction.PublicationId;
                data.Action = keyboardAction.Type;
                data.SocialNetwork = keyboardAction.SocialNetwork;
                data.Author = keyboardAction.Author;

                await HandleAction(data.Action, data);
            }
        }

        private static async Task HandleAction(string actionName, UserCallbackData data)
        {
            if (ActionsByName.TryGetValue(actionName, out var action))
            {
                await action(data);
            }
            else
            {
                await TelegramBotManager.SendException(actionName, "Не удалось найти экшен");
            }
        }


        [InlineKeyboardAction(InlineKeyboardActionType.Publicate)]
        public static async Task PublicatePostAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            if (!PublicationManager.TryGetPublicationEntity(data.PublicationId!, out var publication))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if(CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            var messageText = (string?)null;

            if (PublicationManager.PublicatePublication(publication!))
            {
                messageText = $"Публикация:\n" +
                $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                $"Заголовок: {publication!.Title}\n" +
                $"Дата публикации: {publication!.PublicationDateTime?.ToString("dd.MM HH:mm")}\n" +
                $"Источник: {publication!.Source}\n" +
                $"отправлена на публикацию.";
            }
            else
            {
                messageText = $"Публикация:\n" +
                $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                $"Заголовок: {publication!.Title}\n" +
                $"Дата публикации: {publication!.PublicationDateTime?.ToString("dd.MM HH:mm")}\n" +
                $"не удалось опубликовать.";
            }

            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.ChangeTitle)]
        public static async Task ChangeTitleAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            if (!PublicationManager.TryGetPublicationEntity(data.PublicationId!, out var publication))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (!CurrentActions.TryGetValue(chatId, out var action))
            {
                CurrentActions.Add(chatId, new InlineKeyboardAction(publication!.Id, chatId, InlineKeyboardActionType.ChangeTitle));

                var messageText = $"Публикация:\n" +
                    $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                    $"Заголовок: {publication!.Title}\n" +
                    $"Источник: {publication!.Source}\n\n" +
                    $"Введите новое название:";
                await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
            }
            else
            {
                publication!.Title = data.Message!;
                await TelegramBotManager.SendPublicationEntity(publication!);
                CloseAction(chatId);
            }
        }

        [InlineKeyboardAction(InlineKeyboardActionType.ChangeSource)]
        public static async Task ChangeSourceAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            if (!PublicationManager.TryGetPublicationEntity(data.PublicationId!, out var publication))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (!CurrentActions.TryGetValue(chatId, out var action))
            {
                CurrentActions.Add(chatId, new InlineKeyboardAction(publication!.Id, chatId, InlineKeyboardActionType.ChangeSource));

                var messageText = $"Публикация:\n" +
                    $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                    $"Заголовок: {publication!.Title}\n" +
                    $"Источник: {publication!.Source}\n\n" +
                    $"Введите новый источник:";
                await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
            }
            else
            {
                publication!.Source = data.Message!;
                await TelegramBotManager.SendPublicationEntity(publication!);
                CloseAction(chatId);
            }
        }

        [InlineKeyboardAction(InlineKeyboardActionType.NotPublicate)]
        public static async Task NotPublicateAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            if (!PublicationManager.TryGetPublicationEntity(data.PublicationId!, out var publication))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            PublicationManager.DeletePublication(publication!, true);

            var messageText = $"Публикация:\n" +
                $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                $"Заголовок: {publication!.Title}\n" +
                $"Источник: {publication!.Source}\n" +
                $"удалена.";
            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.ManualProcessing)]
        public static async Task ManualProcessingAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            if (!PublicationManager.TryGetPublicationEntity(data.PublicationId!, out var publication))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            var messageText = (string?)null;

            if (PublicationManager.ManualProcessingPublication(publication!))
            {
                messageText = $"Публикация:\n" +
                    $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                    $"Заголовок: {publication!.Title}\n" +
                    $"Источник: {publication!.Source}\n" +
                    $"отправлена в каталог ручной обработки.";
            }
            else
            {
                messageText = $"Публикация:\n" +
                    $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                    $"Заголовок: {publication!.Title}\n" +
                    $"Источник: {publication!.Source}\n" +
                    $"не удалось отправить в каталог ручной обработки.";
            }

            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.MoveToAlbum)]
        public static async Task MoveToAlbumAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            if (!PublicationManager.TryGetPublicationEntity(data.PublicationId!, out var publication))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Публикация не найдена.");
                return;
            }

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            var messageText = (string?)null;

            if (PublicationManager.MoveToAlbum(publication!))
            {
                messageText = $"Публикация:\n" +
                    $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                    $"Заголовок: {publication!.Title}\n" +
                    $"Источник: {publication!.Source}\n" +
                    $"отправлена в альбом.";
            }
            else
            {
                messageText = $"Публикация:\n" +
                    $"{publication!.SocialNetwork} - {publication!.Author}\n" +
                    $"Заголовок: {publication!.Title}\n" +
                    $"Источник: {publication!.Source}\n" +
                    $"не удалось отправить в альбом.";
            }

            await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
        }

        [InlineKeyboardAction(InlineKeyboardActionType.AuthorsList)]
        public static async Task AuthorsListAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            var userMessage = data.Message;
            var socialNetwork = data.SocialNetwork;

            if (userMessage == "/" + InlineKeyboardActionType.AuthorsList)
                CloseAction(chatId);

            if(!CurrentActions.TryGetValue(chatId, out var action))
            {
                action = new InlineKeyboardAction(null, chatId, InlineKeyboardActionType.AuthorsList);
                CurrentActions.Add(chatId, action);
                await TelegramBotManager.SendTextMessageWithKeyboardAsync(chatId, "Выберите социальную сеть, список авторов которой вы хотите посмотреть:", GetAuthorsListKeyboardMurkup(chatId));
                return;
            }
            else if(!String.IsNullOrWhiteSpace(socialNetwork))
            {
                if (socialNetwork.ToLowerInvariant() == "deviantart")
                {
                    CloseAction(chatId);

                    var responseText = new StringBuilder();
                    responseText.AppendLine("Список авторов социальной сети 'DeviantArt':");
                    foreach (var author in SchedulerTaskDeviantArt.Instance.Authors.OrderBy(x => x))
                    {
                        responseText.AppendLine(author);
                    }

                    var keyboard = GetAuthorsListEdittingKeyboardMurkup(chatId, "DeviantArt");

                    await TelegramBotManager.SendTextMessageWithKeyboardAsync(chatId, responseText.ToString(), keyboard);
                }
            }
        }

        [InlineKeyboardAction(InlineKeyboardActionType.AuthorsListEdit)]
        public static async Task AuthorsListEditAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            var socialNetwork = data.SocialNetwork;

            CloseAction(chatId);

            if (String.IsNullOrWhiteSpace(data.SocialNetwork))
                return;

            if (socialNetwork!.ToLowerInvariant() == "deviantart")
            {
                foreach (var author in SchedulerTaskDeviantArt.Instance.Authors.OrderBy(x => x))
                {
                    var keyboard = GetAuthorEdittingKeyboardMurkup(chatId, "DeviantArt", author);
                    await TelegramBotManager.SendTextMessageWithKeyboardAsync(chatId, author, keyboard);
                }
            }
        }

        [InlineKeyboardAction(InlineKeyboardActionType.AuthorsListAdd)]
        public static async Task AuthorsListAddAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            var socialNetwork = data.SocialNetwork;
            var userMessage = data.Message;

            CloseAction(chatId);

            if (String.IsNullOrWhiteSpace(userMessage))
            {
                CurrentActions.Add(chatId, new InlineKeyboardAction(null, chatId, InlineKeyboardActionType.AuthorsListAdd, 0, "DeviantArt"));
                var messageText = $"Введите имя автора:";
                await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
            }
            else
            {
                if (socialNetwork != null && socialNetwork.ToLowerInvariant() == "deviantart")
                {
                    if (!SchedulerTaskDeviantArt.AddAuthor(userMessage, out var reason))
                        await TelegramBotManager.SendTextMessageAsync(chatId, reason);
                    else
                        await TelegramBotManager.SendTextMessageAsync(chatId, $"Автор '{userMessage}' добавлен в базу социальной сети {socialNetwork}.");
                }
            }

        }

        [InlineKeyboardAction(InlineKeyboardActionType.AuthorDelete)]
        public static async Task AuthorDeleteAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            var socialNetwork = data.SocialNetwork;
            var author = data.Author;

            CloseAction(chatId);

            if (String.IsNullOrWhiteSpace(socialNetwork) || String.IsNullOrWhiteSpace(author))
            {
                await TelegramBotManager.SendTextMessageAsync(chatId, "Не удалось удалить автора.");
                return;
            }

            if (socialNetwork!.ToLowerInvariant() == "deviantart")
            {
                if (!SchedulerTaskDeviantArt.DeleteAuthor(author, out var reason))
                    await TelegramBotManager.SendTextMessageAsync(chatId, reason);
                else
                    await TelegramBotManager.SendTextMessageAsync(chatId, $"Автор '{author}' удален из базы социальной сети {socialNetwork}.");
            }

        }

        [InlineKeyboardAction(InlineKeyboardActionType.AuthorEdit)]
        public static async Task AuthorEditAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            var socialNetwork = data.SocialNetwork;
            var author = data.Author;
            var userMessage = data.Message;

            CloseAction(chatId);

            if (String.IsNullOrWhiteSpace(userMessage))
            {
                CurrentActions.Add(chatId, new InlineKeyboardAction(null, chatId, InlineKeyboardActionType.AuthorEdit, 0, "DeviantArt", author));
                var messageText = $"Введите новое имя автора '{author}':";
                await TelegramBotManager.SendTextMessageAsync(chatId, messageText);
            }
            else
            {
                if (socialNetwork != null && socialNetwork.ToLowerInvariant() == "deviantart")
                {
                    if (!SchedulerTaskDeviantArt.EditAuthor(author!, userMessage, out var reason))
                        await TelegramBotManager.SendTextMessageAsync(chatId, reason);
                    else
                        await TelegramBotManager.SendTextMessageAsync(chatId, $"Имя автора '{author}' изменено на '{userMessage}'.");
                }
            }

        }

        [InlineKeyboardAction(InlineKeyboardActionType.Start)]
        public static async Task StartAction(UserCallbackData data)
        {
            var chatId = data.ChatId;
            var userMessage = data.Message;

            if (CurrentActions.ContainsKey(chatId))
                CloseAction(chatId);

            await TelegramBotManager.SendTextMessageAsync(chatId, "Привет, манагер лучшей группы в мире!");
        }
    }
}
