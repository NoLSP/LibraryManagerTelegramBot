using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class CategoryJson
    {
        [JsonProperty("catpath")]
        public string? CategoryPath { get; set; }
        public string? Title { get; set; }
        [JsonProperty("has_subcategory")]
        public bool HasSubcategory { get; set; }
        [JsonProperty("parent_catpath")]
        public string? ParentCategoryPath { get; set; }
    }
}
