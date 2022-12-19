using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch collection folder contents
    /// </summary>
    public class GetCollectionFolder : PagedRestBase<GetCollectionFolder>, IMatureContent<GetCollectionFolder>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 24;

        public GetCollectionFolder(string apiBaseUri, string folderId)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment($"/gallery/{folderId}");
        }

        public GetCollectionFolder WithUsername(string username)
        {
            _uri = _uri.SetQueryParam("username", username);
            return this;
        }

        public GetCollectionFolder WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
