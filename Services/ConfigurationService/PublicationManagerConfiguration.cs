using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services
{
    public class PublicationManagerConfiguration
    {
        public DateTime? LastPublicationDateTimeUtc { get; set; }
        public TimeOnly? StartPublicationTimeUtc { get; set; }
        public TimeOnly? EndPublicationTimeUtc { get; set; }
        public int? PublicationIntervalMinutes { get; set; }
    }
}
