using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Returns the DeviantArt Terms of Service
    /// </summary>
    public class GetDataTermsOfService : RestBase
    {
        public GetDataTermsOfService(string apiBaseUri)
        {
            _uri = apiBaseUri.AppendPathSegment("/data/tos");
        }
    }
}
