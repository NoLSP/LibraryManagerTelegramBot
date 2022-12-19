using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse journals of a user
    /// </summary>
    public class GetBrowseUserJournals : PagedRestBase<GetBrowseUserJournals>, IMatureContent<GetBrowseUserJournals>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 50;

        public GetBrowseUserJournals(string apiBaseUri, string username)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment("/browse/user/journals")
                .SetQueryParam("username", username);
        }

        public GetBrowseUserJournals WithFeatured(bool featured = true)
        {
            _uri = _uri.SetQueryParam("featured", featured ? "true" : "false");
            return this;
        }

        public GetBrowseUserJournals WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
