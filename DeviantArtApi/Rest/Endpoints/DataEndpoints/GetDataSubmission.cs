using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Returns the DeviantArt Submission Policy
    /// </summary>
    public class GetDataSubmission : RestBase
    {
        public GetDataSubmission(string apiBaseUri)
        {
            _uri = apiBaseUri.AppendPathSegment("/data/submission");
        }
    }
}
