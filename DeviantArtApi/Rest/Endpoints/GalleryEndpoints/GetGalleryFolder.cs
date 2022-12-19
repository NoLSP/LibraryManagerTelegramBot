using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch gallery folder contents
    /// </summary>
    public class GetGalleryFolder : PagedRestBase<GetGalleryFolder>, IMatureContent<GetGalleryFolder>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 24;

        /// <summary>
        /// All folders
        /// </summary>
        /// <param name="apiBaseUri"></param>
        public GetGalleryFolder(string apiBaseUri)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/gallery");
        }

        public GetGalleryFolder(string apiBaseUri, string folderId)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment($"/gallery/{folderId}");
        }

        public GetGalleryFolder WithUsername(string username)
        {
            _uri = _uri.SetQueryParam("username", username);
            return this;
        }

        public GetGalleryFolder WithSortMode(string sortMode)
        {
            _uri = _uri.SetQueryParam("mode", sortMode);
            return this;
        }

        public GetGalleryFolder WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
