using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Autocomplete tags
    /// </summary>
    public class GetBrowseTagsSearch : RestBase, IMatureContent<GetBrowseTagsSearch>
    {
        public GetBrowseTagsSearch(string apiBaseUri, string tagName)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/tags/search")
                .SetQueryParam("tag_name", tagName);
        }

        public GetBrowseTagsSearch WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
