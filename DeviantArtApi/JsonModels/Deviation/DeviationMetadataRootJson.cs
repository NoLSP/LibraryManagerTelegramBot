using System;
using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviationMetadataRootJson
    {
        public string? Html { get; set; }
        public string? Css { get; set; }
        [JsonProperty("css_fonts")]
        public string[]? CssFonts { get; set; }
    }

    public class DeviationMetadataJson
    {
        public string? DeviationId { get; set; }
        public string? PrintId { get; set; }
        public UserJson? Author { get; set; }
        [JsonProperty("is_watching")]
        public bool IsWatching { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? License { get; set; }
        [JsonProperty("allows_comments")]
        public bool AllowsComments { get; set; }
        public TagJson[]? Tags { get; set; }
        [JsonProperty("is_mature")]
        public bool IsMature { get; set; }
        [JsonProperty("is_favourited")]
        public bool IsFavorited { get; set; }
        public SubmissionJson? Submission { get; set; }
        public StatsJson? Stats { get; set; }
        public CameraJson? Camera { get; set; }
        public CollectionFolderJson[]? Collections { get; set; }

        public class CameraJson
        {
            public string? Make { get; set; }
            public string? Model { get; set; }
            [JsonProperty("shutter_speed")]
            public string? ShutterSpeed { get; set; }
            public string? Aperture { get; set; }
            [JsonProperty("focal_length")]
            public string? FocalLength { get; set; }
            [JsonProperty("date_taken")]
            public string? DateTaken { get; set; }
        }

        public class CollectionFolderJson
        {
            public string? FolderId { get; set; }
            public string? Name { get; set; }
        }

        public class StatsJson
        {
            public long Views { get; set; }
            public long ViewsToday { get; set; }
            [JsonProperty("favourites")]
            public long Favorites { get; set; }
            public long Comments { get; set; }
            public long Downloads { get; set; }
            [JsonProperty("downloads_today")]
            public long DownloadsToday { get; set; }
        }

        public class SubmissionJson
        {
            [JsonProperty("creation_time")]
            public DateTime CreationTime { get; set; }
            public string? Category { get; set; }
            [JsonProperty("file_size")]
            public string? FileSize { get; set; }
            public string? Resolution { get; set; }
            [JsonProperty("submitted_with")]
            public SubmittedWithJson? SubmittedWith { get; set; }

            public class SubmittedWithJson
            {
                public string? App { get; set; }
                [JsonProperty("url")]
                public string? Uri { get; set; }
            }
        }

        public partial class TagJson
        {
            [JsonProperty("tag_name")]
            public string? TagName { get; set; }
            public bool Sponsored { get; set; }
            public string? Sponsor { get; set; }
        }
    }
}
