using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class TagNameJson
    {
        [JsonProperty("tag_name")]
        public string? TagName { get; set; }
    }
}
