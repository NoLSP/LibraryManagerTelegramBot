using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviationEmbeddedContentJson : PagedResponse<DeviationJson>
    {
        [JsonProperty("has_less")]
        public bool HasLess { get; set; }
        [JsonProperty("prev_offset")]
        public long? PreviousOffset { get; set; }
    }
}
