using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   GET /user/profile/{username}
    /// </summary>
    public class UserProfileJson
    {
        public UserJson? User { get; set; }
        [JsonProperty("is_watching")]
        public bool IsWatching { get; set; }
        [JsonProperty("profile_url")]
        public string? ProfileUri { get; set; }
        [JsonProperty("user_is_artist")]
        public bool UserIsArtist { get; set; }
        [JsonProperty("artist_level")]
        public string? ArtistLevel { get; set; }
        [JsonProperty("artist_specialty")]
        public string? ArtistSpecialty { get; set; }
        [JsonProperty("real_name")]
        public string? RealName { get; set; }
        public string? Tagline { get; set; }
        public long CountryId { get; set; }
        public string? Country { get; set; }
        public string? Website { get; set; }
        public string? Bio { get; set; }
        [JsonProperty("cover_photo")]
        public string? CoverPhoto { get; set; }
        [JsonProperty("profile_pic")]
        public DeviationJson? ProfilePic { get; set; }
        [JsonProperty("last_status")]
        public StatusJson? LastStatus { get; set; }
        public UserProfileStatsJson? Stats { get; set; }
        public CollectionFolderJson[]? Collections { get; set; }
        public GalleryFolderJson[]? Galleries { get; set; }

        public partial class UserProfileStatsJson
        {
            [JsonProperty("user_deviations")]
            public long UserDeviations { get; set; }

            [JsonProperty("user_favourites")]
            public long UserFavorites { get; set; }

            [JsonProperty("user_comments")]
            public long UserComments { get; set; }

            [JsonProperty("profile_pageviews")]
            public long ProfilePageViews { get; set; }

            [JsonProperty("profile_comments")]
            public long ProfileComments { get; set; }
        }
    }
}
