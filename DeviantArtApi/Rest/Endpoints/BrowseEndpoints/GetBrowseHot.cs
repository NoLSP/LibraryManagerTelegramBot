using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse what's hot deviations
    /// </summary>
    public class GetBrowseHot : PagedRestBase<GetBrowseHot>, IMatureContent<GetBrowseHot>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 120;

        public GetBrowseHot(string apiBaseUri)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/hot");
        }

        public GetBrowseHot WithCategoryPath(string categoryPath)
        {
            _uri = _uri.SetQueryParam("category_path", categoryPath);
            return this;
        }

        public GetBrowseHot WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
