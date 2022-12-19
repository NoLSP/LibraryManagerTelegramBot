using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Get a list of countries
    /// </summary>
    public class GetDataCountries : RestBase
    {
        public GetDataCountries(string apiBaseUri)
        {
            _uri = apiBaseUri.AppendPathSegment("/data/countries");
        }
    }
}
