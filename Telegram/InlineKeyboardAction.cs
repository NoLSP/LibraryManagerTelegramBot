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
    public class InlineKeyboardAction
    {
        public string? PublicationId { get; set; }
        public long ChatId { get; set; }
        public string Type { get; set; }
        public string? SocialNetwork { get; set; }
        public string? Author { get; set; }
        public int Stage { get; set; }

        public InlineKeyboardAction(string? publicationId, long chatId,string type, int stage = 0, string? socialNetwork = null, string? author = null)
        {
            PublicationId = publicationId;
            ChatId = chatId;
            Type = type;
            Stage = stage;
            SocialNetwork = socialNetwork;
            Author = author;
        }
    }
}
