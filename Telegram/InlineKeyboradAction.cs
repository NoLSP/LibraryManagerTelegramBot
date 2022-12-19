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
    public enum InlineKeyboardAction
    {
        Publicate = 0,
        NotPublicate = 1,
        ChangeTime = 2,
        ChangeTitle = 3,
        ChangeSource = 4,
        ManualProcessing = 5,
        MoveToAlbum = 6

    }
}
