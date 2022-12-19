using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   GET /user/watchers/{username}
    /// </summary>
    public class UserWatchersOfUsername
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }
        [JsonProperty("next_offset")]
        public long? NextOffset { get; set; }
        public UserWatchersJson[]? Results { get; set; }

        public class UserWatchersJson
        {
            public UserJson? User { get; set; }
            [JsonProperty("is_watching")]
            public bool IsWatching { get; set; }
            public string? LastVisit { get; set; }
            public WatchJson? Watch { get; set; }
        }
    }
}
