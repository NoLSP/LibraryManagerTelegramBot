using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch gallery folders
    /// 
    /// You can preload up to 5 deviations from each folder by passing ext_preload parameter. It is mainly useful to reduce number of requests to API.
    /// </summary>
    public class GetGalleryFolderList : PagedRestBase<GetGalleryFolderList>, IMatureContent<GetGalleryFolderList>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 50;

        public GetGalleryFolderList(string apiBaseUrl)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUrl.AppendPathSegment("/gallery/folders");
        }

        public GetGalleryFolderList WithUsername(string username)
        {
            _uri = _uri.SetQueryParam("username", username);
            return this;
        }

        public GetGalleryFolderList WithCalculateSize(bool calculateSize = true)
        {
            _uri = _uri.SetQueryParam("calculate_size", calculateSize ? "true" : "false");
            return this;
        }

        public GetGalleryFolderList WithExtendedPreload(bool extendedPreload = true)
        {
            _uri = _uri.SetQueryParam("ext_preload", extendedPreload ? "true" : "false");
            return this;
        }

        public GetGalleryFolderList WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
