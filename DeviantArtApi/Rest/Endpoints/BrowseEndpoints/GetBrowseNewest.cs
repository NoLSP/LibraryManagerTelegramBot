using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse newest deviations
    /// </summary>
    public class GetBrowseNewest : PagedRestBase<GetBrowseNewest>, IMatureContent<GetBrowseNewest>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 120;

        public GetBrowseNewest(string apiBaseUri)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/newest");
        }

        public GetBrowseNewest WithCategoryPath(string categoryPath)
        {
            _uri = _uri.SetQueryParam("category_path", categoryPath);
            return this;
        }

        public GetBrowseNewest WithQuery(string query)
        {
            _uri = _uri.SetQueryParam("q", query);
            return this;
        }

        public GetBrowseNewest WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
