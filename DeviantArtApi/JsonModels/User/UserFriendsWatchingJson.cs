namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   GET /user/friends/watching/{username}
    /// </summary>
    public class UserFriendsWatchingJson
    {
        public bool Watching { get; set; }
    }
}
