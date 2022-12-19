using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch category information for browsing
    /// </summary>
    public class GetBrowseCategoryTree : RestBase, IMatureContent<GetBrowseCategoryTree>
    {
        public GetBrowseCategoryTree(string apiBaseUri, string categoryPath)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/categorytree")
                .SetQueryParam("catpath", categoryPath);
        }

        public GetBrowseCategoryTree WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
