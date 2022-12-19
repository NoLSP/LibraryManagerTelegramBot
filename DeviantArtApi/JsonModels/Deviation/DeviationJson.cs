using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviationJson
    {
        public string? DeviationId { get; set; }
        public string? PrintId { get; set; }
        [JsonProperty("url")]
        public Uri? Uri { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        [JsonProperty("category_path")]
        public string? CategoryPath { get; set; }
        [JsonProperty("is_favourited")]
        public bool? IsFavorited { get; set; }
        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }
        public UserJson? Author { get; set; }
        public DeviationStatsJson? Stats { get; set; }
        [JsonProperty("published_time")]
        //[JsonConverter(typeof(JavaScriptDateTimeConverter))] // JavaScriptDateTimeConverter, IsoDateTimeConverter
        public double? PublishedTime { get; set; }

        public DateTime? PublishedDateTime
        {
            get
            {
                if(PublishedTime.HasValue)
                {
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(PublishedTime.Value).ToUniversalTime();
                    return dateTime;
                }

                return null;
            }
        }
        public bool? AllowsComments { get; set; }
        public DeviationContentJson? Preview { get; set; }
        public DeviationContentJson? Content { get; set; }
        [JsonProperty("thumbs")]
        public DeviationContentJson[]? Thumbnails { get; set; }
        public DeviationVideoJson[]? Videos { get; set; }
        public DeviationFlashJson? Flash { get; set; }
        [JsonProperty("daily_deviation")]
        public DailyDeviationJson? DailyDeviation { get; set; }
        [JsonProperty("is_mature")]
        public bool? IsMature { get; set; }
        [JsonProperty("is_downloadable")]
        public bool? IsDownloadable { get; set; }
        [JsonProperty("download_filesize")]
        public long? DownloadFilesize { get; set; }
        public string? Excerpt { get; set; }

        // public DeviationChallenge Challenge { get; set; }
        // [JsonProperty("challenge_entry")]
        // public DeviationChallengeEntry ChallengeEntry { get; set; }
        // [JsonProperty("motion_book")]
        // public DeviationMotionBook MotionBook { get; set; }

        public class DailyDeviationJson
        {
            public string? Body { get; set; }
            public string? Time { get; set; }
            public UserJson? Giver { get; set; }
            public UserJson? Suggester { get; set; }
        }

        public class DeviationContentJson
        {
            [JsonProperty("src")]
            public Uri? SourceUri { get; set; }
            public long Height { get; set; }
            public long Width { get; set; }
            public bool? Transparency { get; set; }
            public long? FileSize { get; set; }
        }

        public class DeviationVideoJson
        {
            [JsonProperty("src")]
            public Uri? SourceUri { get; set; }
            public string? Quality { get; set; }
            public long? FileSize { get; set; }
            public long Duration { get; set; }
        }

        public class DeviationFlashJson
        {
            [JsonProperty("src")]
            public Uri? SourceUri { get; set; }
            public long Height { get; set; }
            public long Width { get; set; }
        }

        public class DeviationStatsJson
        {
            public long Comments { get; set; }
            [JsonProperty("favourites")]
            public long Favorites { get; set; }
        }

        // public class DeviationChallenge
        // {
        //     public string[] Type { get; set; }
        //     public bool Completed { get; set; }
        //     public string[] Tags { get; set; }
        //     public bool Locked { get; set; } = false;
        //     [JsonProperty("credit_deviation")]
        //     public string CreditDeviationId { get; set; }
        //     public string[] Media { get; set; }
        //     [JsonProperty("level_label")]
        //     public string LevelLabel { get; set; }
        //     [JsonProperty("time_limit")]
        //     public int TimeLimit { get; set; } = 0;
        //     public string[] Levels { get; set; }
        // }

        // public class DeviationChallengeEntry
        // {
        //     public string ChallengeId { get; set; }
        //     [JsonProperty("challenge_title")]
        //     public string ChallengeTitle { get; set; }
        //     public Deviation Challenge { get; set; }
        //     [JsonProperty("timed_duration")]
        //     public int TimedDuration { get; set; }
        //     [JsonProperty("submission_time")]
        //     public string SubmissionTime { get; set; }
        // }

        // public class DeviationMotionBook
        // {
        //     [JsonProperty("embed_url")]
        //     public string EmbedUrl { get; set; }
        // }
    }
}
