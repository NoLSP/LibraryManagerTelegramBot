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
    public static class InlineKeyboardActionManager
    {
        private static readonly string[] ActionsByIndex = new string[]{
            "publicate",
            "notpublicate",
            "changetime",
            "changetitle",
            "changesource",
            "manualprocessing",
            "movetoalbum"
        };

        private static readonly Dictionary<string, InlineKeyboardAction> ActionStringsByAction = new Dictionary<string, InlineKeyboardAction>()
        {
            ["publicate"] = InlineKeyboardAction.Publicate,
            ["notpublicate"] = InlineKeyboardAction.NotPublicate,
            ["changetime"] = InlineKeyboardAction.ChangeTime,
            ["changetitle"] = InlineKeyboardAction.ChangeTitle,
            ["changesource"] = InlineKeyboardAction.ChangeSource,
            ["manualprocessing"] = InlineKeyboardAction.ManualProcessing,
            ["movetoalbum"] = InlineKeyboardAction.MoveToAlbum,
        };

        public static string? Obtain(InlineKeyboardAction action)
        {
            return ActionsByIndex[(int)action];
        }

        public static InlineKeyboardAction? Obtain(string action)
        {
            action = action.ToLower().Trim();

            if (ActionStringsByAction.ContainsKey(action))
                return ActionStringsByAction[action];

            return null;
        }

        public static bool IsActionRequiresCallbackText(InlineKeyboardAction action)
        {
            if (action == InlineKeyboardAction.Publicate || action == InlineKeyboardAction.NotPublicate || action == InlineKeyboardAction.ManualProcessing || action == InlineKeyboardAction.MoveToAlbum)
                return false;
            else
                return true;
        }
    }
}
