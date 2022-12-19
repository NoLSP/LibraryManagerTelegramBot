using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Get authentification token
    /// </summary>
    public class GetToken : RestBase
    {
        public GetToken(string apiBaseUri, string clientId, string clientSecret)
        {
            _uri = apiBaseUri.AppendPathSegment($"/token");
            _uri = _uri.SetQueryParam("grant_type", "client_credentials");
            _uri = _uri.SetQueryParam("client_id", clientId);
            _uri = _uri.SetQueryParam("client_secret", clientSecret);
        }
    }
}