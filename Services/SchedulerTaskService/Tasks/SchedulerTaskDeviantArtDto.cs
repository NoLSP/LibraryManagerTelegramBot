using NLog;
using SpecialLibraryBot.DeviantArtApi;
using SpecialLibraryBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services.SchedulerTaskService
{
    public class SchedulerTaskDeviantArtDto
    {
        public Dictionary<string, DateTime>? AuthorsLastArtsDates;
        public List<string>? Authors;
        public DateTime LastUpdateDateTime;
    }
}
