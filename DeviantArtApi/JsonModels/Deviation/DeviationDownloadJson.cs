using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviationDownloadJson
    {
        [JsonProperty("src")]
        public string? SourceUri { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }
        public long FileSize { get; set; }
    }
}
