using Flurl;
using System;

namespace SpecialLibraryBot.DeviantArtApi
{
    /// <summary>
    /// Browse daily deviations
    /// </summary>
    public class GetBrowseDailyDeviations : RestBase, IMatureContent<GetBrowseDailyDeviations>
    {
        public GetBrowseDailyDeviations(string apiBaseUrl)
        {
            _uri = apiBaseUrl.AppendPathSegment("/browse/dailydeviations");
        }

        public GetBrowseDailyDeviations WithDate(DateTime date)
        {
            _uri = _uri.SetQueryParam("date", date.ToString("yyyy-MM-dd"));
            return this;
        }

        public GetBrowseDailyDeviations WithMatureContent(bool allowMature = true)
        {
            _uri = _uri.SetQueryParam("mature_content", allowMature ? "true" : "false");
            return this;
        }
    }
}
