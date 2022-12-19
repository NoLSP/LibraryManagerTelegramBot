using Flurl;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Fetch deviation metadata for a set of deviations
    /// 
    /// This endpoint is limited to 50 deviations per query when fetching the base data and 10 when fetching extended data.
    /// </summary>
    public class GetDeviationMetadata : RestBase, IMatureContent<GetDeviationMetadata>
    {
        public GetDeviationMetadata(string apiBaseUri, string deviationId)
            : this(apiBaseUri, new string[] { deviationId })
        {
        }

        public GetDeviationMetadata(string apiBaseUri, string[] deviationIds)
        {
            _uri = apiBaseUri.AppendPathSegment($"/deviation/metadata");

            foreach (var id in deviationIds)
            {
                _uri = _uri.SetQueryParam("deviationids[]", id);
            }
        }

        /// <summary>
        /// Return extended information - submission information
        /// </summary>
        /// <param name="extendedSubmission"></param>
        /// <returns></returns>
        public GetDeviationMetadata WithExtendedSubmission(bool extendedSubmission = true)
        {
            _uri = _uri.SetQueryParam("ext_submission", extendedSubmission ? "true" : "false");
            return this;
        }

        /// <summary>
        /// Return extended information - EXIF information (if available)
        /// </summary>
        /// <param name="extendedCamera"></param>
        /// <returns></returns>
        public GetDeviationMetadata WithExtendedCamera(bool extendedCamera = true)
        {
            _uri = _uri.SetQueryParam("ext_camera", extendedCamera ? "true" : "false");
            return this;
        }

        /// <summary>
        /// Return extended information - deviation statistics
        /// </summary>
        /// <param name="extendedStats"></param>
        /// <returns></returns>
        public GetDeviationMetadata WithExtendedStats(bool extendedStats = true)
        {
            _uri = _uri.SetQueryParam("ext_stats", extendedStats ? "true" : "false");
            return this;
        }

        /// <summary>
        /// Return extended information - favourited folder information (only available for logged in sessions)
        /// </summary>
        /// <param name="extendedCollection"></param>
        /// <returns></returns>
        public GetDeviationMetadata WithExtendedCollection(bool extendedCollection = true)
        {
            _uri = _uri.SetQueryParam("ext_collection", extendedCollection ? "true" : "false");
            return this;
        }

        public GetDeviationMetadata WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
