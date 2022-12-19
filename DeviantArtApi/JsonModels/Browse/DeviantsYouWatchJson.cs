using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviantsYouWatchJson
    {
        [JsonProperty("has_more ")]
        public bool HasMore { get; set; }
        [JsonProperty("next_offset ")]
        public int? NextOffset { get; set; }
        [JsonProperty("error_code ")]
        public int ErrorCode { get; set; }
        [JsonProperty("results")]
        public DeviationJson[]? Results { get; set; }
    }
}
