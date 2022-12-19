using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch MoreLikeThis result for a seed deviation
    /// </summary>
    public class GetBrowseMoreLikeThis : PagedRestBase<GetBrowseMoreLikeThis>, IMatureContent<GetBrowseMoreLikeThis>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 50;

        public GetBrowseMoreLikeThis(string apiBaseUri, string seedDeviationId)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/morelikethis")
                .SetQueryParam("seed", seedDeviationId);
        }

        public GetBrowseMoreLikeThis WithCategory(string category)
        {
            _uri = _uri.SetQueryParam("category", category);
            return this;
        }

        public GetBrowseMoreLikeThis WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
