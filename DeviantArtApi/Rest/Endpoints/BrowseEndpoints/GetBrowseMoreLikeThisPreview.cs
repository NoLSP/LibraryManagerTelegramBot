using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch More Like This preview result for a seed deviation
    /// </summary>
    public class GetBrowseMoreLikeThisPreview : RestBase, IMatureContent<GetBrowseMoreLikeThisPreview>
    {
        public GetBrowseMoreLikeThisPreview(string apiBaseUri, string seedDeviationId)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/morelikethis/preview")
                .SetQueryParam("seed", seedDeviationId);
        }

        public GetBrowseMoreLikeThisPreview WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
