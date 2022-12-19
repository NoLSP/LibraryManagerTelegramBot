using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   POST /user/friends/watch/{username}
    /// </summary>
    public class UserFriendsWatchJson
    {
        [JsonProperty("error_description")]
        public string? ErrorDescription { get; set; }
        public bool Success { get; set; }
    }
}
