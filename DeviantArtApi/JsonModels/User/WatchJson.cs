using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   GET /user/friends/{username}
    /// </summary>
    public class WatchJson
    {
        public bool Friend { get; set; }
        public bool Deviations { get; set; }
        public bool Journals { get; set; }
        [JsonProperty("forum_threads")]
        public bool ForumThreads { get; set; }
        public bool Critiques { get; set; }
        public bool Scraps { get; set; }
        public bool Activity { get; set; }
        public bool Collections { get; set; }
    }
}
