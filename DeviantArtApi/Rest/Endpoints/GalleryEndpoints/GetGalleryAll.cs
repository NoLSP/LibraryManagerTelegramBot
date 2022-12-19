using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Get the "all" view of a users gallery
    /// </summary>
    public class GetGalleryAll : PagedRestBase<GetGalleryAll>, IMatureContent<GetGalleryAll>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 24;

        public GetGalleryAll(string apiBaseUrl)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUrl.AppendPathSegment("/gallery/all");
        }

        public GetGalleryAll WithUsername(string username)
        {
            _uri = _uri.SetQueryParam("username", username);
            return this;
        }

        public GetGalleryAll WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
