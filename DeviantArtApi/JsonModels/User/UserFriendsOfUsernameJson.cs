using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   GET /user/friends/{username}
    /// </summary>
    public class UserFriendsOfUsernameJson
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }
        [JsonProperty("next_offset")]
        public long? NextOffset { get; set; }
        public UserFriendsJson[]? Results { get; set; }

        public class UserFriendsJson
        {
            public UserJson? User { get; set; }
            [JsonProperty("is_watching")]
            public bool IsWatching { get; set; }
            [JsonProperty("watches_you")]
            public bool WatchesYou { get; set; }
            [JsonProperty("watch")]
            public WatchJson? Watch { get; set; }
        }
    }
}
