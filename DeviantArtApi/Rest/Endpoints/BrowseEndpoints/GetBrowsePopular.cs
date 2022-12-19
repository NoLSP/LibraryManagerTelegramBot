using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse popular deviations
    /// </summary>
    public class GetBrowsePopular : PagedRestBase<GetBrowsePopular>, IMatureContent<GetBrowsePopular>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 120;

        public GetBrowsePopular(string apiBaseUri)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/popular");
        }

        public GetBrowsePopular WithCategoryPath(string categoryPath)
        {
            _uri = _uri.SetQueryParam("category_path", categoryPath);
            return this;
        }

        public GetBrowsePopular WithQuery(string query)
        {
            _uri = _uri.SetQueryParam("q", query);
            return this;
        }

        public GetBrowsePopular WithTimeRange(string timeRange)
        {
            _uri = _uri.SetQueryParam("timerange", timeRange);
            return this;
        }

        public GetBrowsePopular WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
