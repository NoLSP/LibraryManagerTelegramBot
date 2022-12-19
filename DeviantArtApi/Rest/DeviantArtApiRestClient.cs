
namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviantArtApiRestClient
    {
        private readonly string apiBaseUri;
        private readonly string apiAuthBaseUri;

        public DeviantArtApiRestClient(string baseUrl, string authBaseUrl)
        {
            apiBaseUri = baseUrl;
            apiAuthBaseUri = authBaseUrl;
        }

        //Auth Endpoints

        /// <summary>
        /// GEt authentification token
        /// </summary>
        /// <param name="categoryPath"></param>
        /// <returns></returns>
        public GetToken GetToken(string clientId, string clientSecret)
        {
            return new GetToken(apiAuthBaseUri, clientId, clientSecret);
        }

        /// <summary>
        /// Get the "all" view of a users gallery
        /// </summary>
        /// <returns></returns>
        public GetGalleryAll GetGetGalleryAll(string userName, int pageLimit, int pageOffset, bool withMatureContent)
        {
            return new GetGalleryAll(apiBaseUri)
                .WithPageLimit(pageLimit)
                .WithPageOffset(pageOffset)
                .WithUsername(userName)
                .WithMatureContent(withMatureContent);
        }

        // Browse Endpoints

        /// <summary>
        /// Fetch category information for browsing
        /// </summary>
        /// <param name="categoryPath"></param>
        /// <returns></returns>
        public GetBrowseCategoryTree GetGetBrowseCategoryTree(string categoryPath)
        {
            return new GetBrowseCategoryTree(apiBaseUri, categoryPath);
        }

        /// <summary>
        /// Browse daily deviations
        /// </summary>
        /// <returns></returns>
        public GetBrowseDailyDeviations GetBrowseDailyDeviations()
        {
            return new GetBrowseDailyDeviations(apiBaseUri);
        }

        /// <summary>
        /// Browse what's hot deviations
        /// </summary>
        /// <returns></returns>
        public GetBrowseHot GetBrowseHot()
        {
            return new GetBrowseHot(apiBaseUri);
        }

        /// <summary>
        /// Fetch MoreLikeThis result for a seed deviation
        /// </summary>
        /// <param name="seedDeviationId"></param>
        /// <returns></returns>
        public GetBrowseMoreLikeThis GetBrowseMoreLikeThis(string seedDeviationId)
        {
            return new GetBrowseMoreLikeThis(apiBaseUri, seedDeviationId);
        }

        /// <summary>
        /// Fetch More Like This preview result for a seed deviation
        /// </summary>
        /// <param name="seedDeviationId"></param>
        /// <returns></returns>
        public GetBrowseMoreLikeThisPreview GetBrowseMoreLikeThisPreview(string seedDeviationId)
        {
            return new GetBrowseMoreLikeThisPreview(apiBaseUri, seedDeviationId);
        }

        /// <summary>
        /// Browse newest deviations
        /// </summary>
        /// <returns></returns>
        public GetBrowseNewest GetBrowseNewest()
        {
            return new GetBrowseNewest(apiBaseUri);
        }

        /// <summary>
        /// Browse popular deviations
        /// </summary>
        /// <returns></returns>
        public GetBrowsePopular GetBrowsePopular()
        {
            return new GetBrowsePopular(apiBaseUri);
        }

        /// <summary>
        /// Browse a tag
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public GetBrowseTags GetBrowseTags(string tagName)
        {
            return new GetBrowseTags(apiBaseUri, tagName);
        }

        /// <summary>
        /// Autocomplete tags
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public GetBrowseTagsSearch GetBrowseTagsSearch(string tagName)
        {
            return new GetBrowseTagsSearch(apiBaseUri, tagName);
        }

        /// <summary>
        /// Browse undiscovered deviations
        /// </summary>
        /// <returns></returns>
        public GetBrowseUndiscovered GetBrowseUndiscovered()
        {
            return new GetBrowseUndiscovered(apiBaseUri);
        }

        /// <summary>
        /// Browse journals of a user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public GetBrowseUserJournals GetBrowseUserJournals(string username)
        {
            return new GetBrowseUserJournals(apiBaseUri, username);
        }

        // Collection Endpoints

        /// <summary>
        /// Fetch collection folder contents
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public GetCollectionFolder GetCollectionFolder(string folderId)
        {
            return new GetCollectionFolder(apiBaseUri, folderId);
        }

        /// <summary>
        /// Fetch collection folders
        /// </summary>
        /// <returns></returns>
        public GetCollectionFolderList GetCollectionFolderList()
        {
            return new GetCollectionFolderList(apiBaseUri);
        }

        /// <summary>
        /// Fetch a deviation
        /// </summary>
        /// <param name="deviationId"></param>
        /// <returns></returns>
        public GetDeviation GetDeviation(string deviationId)
        {
            return new GetDeviation(apiBaseUri, deviationId);
        }

        /// <summary>
        /// Get the original file download (if allowed)
        /// </summary>
        /// <param name="deviationId"></param>
        /// <returns></returns>
        public GetDeviationDownload GetDeviationDownload(string deviationId)
        {
            return new GetDeviationDownload(apiBaseUri, deviationId);
        }

        /// <summary>
        /// Fetch deviation metadata for a deviation
        /// </summary>
        /// <param name="deviationId"></param>
        /// <returns></returns>
        public GetDeviationMetadata GetDeviationMetadata(string deviationId)
        {
            return new GetDeviationMetadata(apiBaseUri, deviationId);
        }

        /// <summary>
        /// Fetch deviation metadata for a set of deviations
        /// </summary>
        /// <param name="deviationIds"></param>
        /// <returns></returns>
        public GetDeviationMetadata GetDeviationMetadata(string[] deviationIds)
        {
            return new GetDeviationMetadata(apiBaseUri, deviationIds);
        }

        /// <summary>
        /// Fetch gallery folder contents
        /// </summary>
        /// <returns></returns>
        public GetGalleryFolder GetGalleryFolder()
        {
            return new GetGalleryFolder(apiBaseUri);
        }

        /// <summary>
        /// Fetch gallery folder contents
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        public GetGalleryFolder GetGalleryFolder(string folderId)
        {
            return new GetGalleryFolder(apiBaseUri, folderId);
        }

        /// <summary>
        /// Fetch gallery folders
        /// </summary>
        /// <returns></returns>
        public GetGalleryFolderList GetGalleryFolderList()
        {
            return new GetGalleryFolderList(apiBaseUri);
        }

        /// <summary>
        /// Get the user's list of friends
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public GetUserFriendList GetUserFriendList(string username)
        {
            return new GetUserFriendList(apiBaseUri, username);
        }

        /// <summary>
        /// Get user profile information
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public GetUserProfile GetUserProfile(string username)
        {
            return new GetUserProfile(apiBaseUri, username);
        }

        /// <summary>
        /// Placebo call to confirm your access_token is valid
        /// </summary>
        /// <returns></returns>
        public GetPlacebo GetPlacebo()
        {
            return new GetPlacebo(apiBaseUri);
        }
    }
}
