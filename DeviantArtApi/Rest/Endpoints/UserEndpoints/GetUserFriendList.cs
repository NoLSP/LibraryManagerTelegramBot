using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Get the user's list of friends
    /// </summary>
    public class GetUserFriendList : PagedRestBase<GetUserFriendList>, IMatureContent<GetUserFriendList>
    {
        public const int MaxOffset = 50_000;
        public const int MaxLimit = 50;

        public GetUserFriendList(string apiBaseUri, string username)
            : base(maxOffset: MaxOffset, maxLimit: MaxLimit)
        {
            _uri = apiBaseUri.AppendPathSegment($"/user/friends/{username}");
        }

        public GetUserFriendList WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
