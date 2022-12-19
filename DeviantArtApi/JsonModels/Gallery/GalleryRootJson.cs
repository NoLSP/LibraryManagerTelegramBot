using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Used by:
    ///   GET /gallery/{folderid}
    /// </summary>
    public class GalleryByFolderIdRootJson : PagedResponse<DeviationJson>
    {
    }

    /// <summary>
    /// Used by:
    ///   GET /gallery/all
    /// </summary>
    public class GalleryAllRootJson : PagedResponse<DeviationJson>
    {
    }

    /// <summary>
    /// Used by:
    ///   GET /gallery/folders
    /// </summary>
    public class GalleryFoldersRootJson : PagedResponse<GalleryFolderJson>
    {
    }

    /// <summary>
    /// Used by:
    ///   GET /gallery/folders/create
    /// </summary>
    public class GalleryFoldersCreateRootJson
    {
        public string? FolderId { get; set; }
        public string? Name { get; set; }
    }

    /// <summary>
    /// Used by:
    ///   GET /gallery/folders/remove/{folderid}
    /// </summary>
    public class GalleryFoldersRemoveRootJson
    {
        public bool Success { get; set; }
    }
}
