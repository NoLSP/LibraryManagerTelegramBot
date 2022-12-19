using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Placebo call to confirm your access_token is valid
    /// </summary>
    public class GetPlacebo : RestBase
    {
        public GetPlacebo(string apiBaseUri)
        {
            _uri = apiBaseUri.AppendPathSegment("/placebo");
        }

        public GetPlacebo WithMatureContent(bool includeMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", includeMature);
            return this;
        }
    }
}
