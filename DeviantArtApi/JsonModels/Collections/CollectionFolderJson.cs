using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class CollectionFolderJson
    {
        public string? FolderId { get; set; }
        public string? Name { get; set; }
        public long? Size { get; set; }
        public DeviationJson[]? Deviations { get; set; }
    }
}
