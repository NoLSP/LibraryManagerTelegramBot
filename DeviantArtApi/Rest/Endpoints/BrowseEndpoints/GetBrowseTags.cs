using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse a tag
    /// </summary>
    public class GetBrowseTags : PagedRestBase<GetBrowseTags>, IMatureContent<GetBrowseTags>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 50;

        public GetBrowseTags(string apiBaseUri, string tagName)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/tags")
                .SetQueryParam("tag", tagName);
        }

        public GetBrowseTags WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
