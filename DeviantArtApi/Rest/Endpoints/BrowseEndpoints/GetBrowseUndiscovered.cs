using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse undiscovered deviations
    /// </summary>
    public class GetBrowseUndiscovered : PagedRestBase<GetBrowseUndiscovered>, IMatureContent<GetBrowseUndiscovered>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 120;

        public GetBrowseUndiscovered(string apiBaseUri)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/undiscovered");
        }

        public GetBrowseUndiscovered WithCategoryPath(string categoryPath)
        {
            _uri = _uri.SetQueryParam("category_path", categoryPath);
            return this;
        }

        public GetBrowseUndiscovered WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
