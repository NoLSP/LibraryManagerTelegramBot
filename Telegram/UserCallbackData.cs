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
    public class UserCallbackData
    {
        public string? PublicationId { get; set; }
        public string? Action { get; set; }
        [JsonIgnore]
        public long? ChatId { get; set; }
        [JsonIgnore]
        public string? Message { get; set; }
    }
}
