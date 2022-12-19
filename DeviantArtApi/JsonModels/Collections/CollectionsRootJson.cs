using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class CollectionsRootJson : PagedResponse<DeviationJson>
    {
    }

    public class CollectionsFaveRootJson
    {
        public bool Success { get; set; }
        [JsonProperty("favourites")]
        public long Favorites { get; set; }
    }

    public class CollectionsFoldersRootJson : PagedResponse<CollectionFolderJson>
    {
    }

    public class CollectionsFoldersCreateRootJson
    {
        [JsonProperty("folderid")]
        public string? FolderId { get; set; }
        public string? Name { get; set; }
    }

    public class CollectionsFoldersRemoveRootJson
    {
        public bool Success { get; set; }
    }

    public class CollectionsUnfaveRootJson
    {
        public bool Success { get; set; }
        [JsonProperty("favourites")]
        public long Favorites { get; set; }
    }
}
