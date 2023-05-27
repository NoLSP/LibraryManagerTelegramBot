using Newtonsoft.Json;
using NLog;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.VK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace SpecialLibraryBot.Telegram
{
    public static class InlineKeyboardActionType
    {
        public const string Publicate = "publicate";
        public const string NotPublicate = "not_publicate";
        public const string ChangeTitle = "change_title";
        public const string ChangeSource = "change_source";
        public const string ManualProcessing = "manual_processing";
        public const string MoveToAlbum = "move_to_album";
        public const string AuthorsList = "authors_list";
        public const string AuthorsListEdit = "authors_list_edit";
        public const string AuthorsListAdd = "authors_list_add";
        public const string AuthorEdit = "author_edit";
        public const string AuthorDelete = "author_delete";
        public const string Start = "start";
    }
}
