using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class StatusJson
    {
        public string? StatusId { get; set; }
        public string? Body { get; set; }
        [JsonProperty("ts")]
        public string? TimeStamp { get; set; }
        [JsonProperty("url")]
        public string? Uri { get; set; }
        [JsonProperty("comments_count")]
        public long CommentsCount { get; set; }
        [JsonProperty("is_share")]
        public bool IsShare { get; set; }
        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }
        public UserJson? Author { get; set; }
        public object[]? Items { get; set; }
    }
}
