using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Get user profile information
    /// </summary>
    public class GetUserProfile : RestBase, IMatureContent<GetUserProfile>
    {
        public GetUserProfile(string apiBaseUri, string username)
        {
            _uri = apiBaseUri.AppendPathSegment($"/user/profile/{username}");
        }

        public GetUserProfile WithExtendedCollections(bool extendedCollections = true)
        {
            _uri = _uri.SetQueryParam("ext_collections", extendedCollections ? "true" : "false");
            return this;
        }

        public GetUserProfile WithExtendedGalleries(bool extendedGalleries = true)
        {
            _uri = _uri.SetQueryParam("ext_galleries", extendedGalleries ? "true" : "false");
            return this;
        }

        public GetUserProfile WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}