using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse deviants that you watch
    /// </summary>
    public class GetBrowseDeviantsYouWatch : PagedRestBase<GetBrowseDeviantsYouWatch>, IMatureContent<GetBrowseDeviantsYouWatch>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 50;

        public GetBrowseDeviantsYouWatch(string apiBaseUri)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/deviantsyouwatch");
        }

        public GetBrowseDeviantsYouWatch WithSession(bool withSession)
        {
            _uri = _uri.SetQueryParam("with_session", withSession);
            return this;
        }

        public GetBrowseDeviantsYouWatch WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
