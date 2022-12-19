using Newtonsoft.Json;

namespace Nullforce.Api.DeviantArt.JsonModels
{
    /// <summary>
    /// Used by:
    ///   GET /user/friends/unwatch/{username}
    /// </summary>
    public class UserFriendsUnwatchJson
    {
        [JsonProperty("error_description")]
        public string? ErrorDescription { get; set; }
        public bool Success { get; set; }
    }
}
