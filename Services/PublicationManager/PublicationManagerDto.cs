using Microsoft.Extensions.Configuration;
using NLog;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.Telegram;
using SpecialLibraryBot.VK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot
{
    public class PublicationManagerDto
    {
        public Dictionary<string, PublicationEntity>? Publications { get; set; }
        public DateTime LastPublicationDateTime { get; set; }
        public string StartPublicationTime { get; set; }
        public string EndPublicationTime { get; set; }
        public int PublicationIntervalMinutes { get; set; }

        public PublicationManagerDto() { }
    }
}
