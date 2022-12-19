using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services
{
    public class DeviantArtConfiguration
    {
        public string[]? Authors { get; set; }
        public string? BackendUrl { get; set; }
        public string? ApiClientId { get; set; }
        public string? ApiClientSecret { get; set; }
        public DateTime? LastUpdateDateTimeUtc { get; set; }
        public string? Source { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? ApiAuthBaseUrl { get; set; }

    }
}
