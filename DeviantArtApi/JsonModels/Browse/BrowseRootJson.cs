using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class BrowseCategoryTreeRootJson
    {
        public CategoryJson[]? Categories { get; set; }
    }

    public class BrowseDailyDeviationsRootJson
    {
        public DeviationJson[]? Results { get; set; }
    }

    public class BrowseHotRootJson : PagedResponse<DeviationJson>
    {
    }

    public class BrowseMoreLikeThisRootJson : PagedResponse<DeviationJson>
    {
    }

    public class BrowseMoreLikeThisPreviewRootJson
    {
        public string? Seed { get; set; }
        public UserJson? Author { get; set; }
        [JsonProperty("more_from_artist")]
        public DeviationJson[]? MoreFromArtist { get; set; }
        [JsonProperty("more_from_da")]
        public DeviationJson[]? MoreFromDa { get; set; }
    }

    public class BrowseNewestRootJson : PagedResponse<DeviationJson>
    {
    }

    public class BrowsePopularRootJson : PagedResponse<DeviationJson>
    {
    }

    public class BrowseTagsRootJson : PagedResponse<DeviationContentJson>
    {
    }

    public class BrowseTagSearchRootJson
    {
        public TagNameJson[]? Results { get; set; }
    }

    public class BrowseUndiscoveredRootJson : PagedResponse<DeviationContentJson>
    {
    }

    public class BrowseUserJournalsRootJson : PagedResponse<DeviationContentJson>
    {
    }
}
