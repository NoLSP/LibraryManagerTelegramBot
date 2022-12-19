using Flurl;
using Microsoft.Win32;
using MimeTypes;
using SpecialLibraryBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SpecialLibraryBot.Helpers
{
    public static class DeviantArtImageHelper
    {
        private static readonly string? backendDeviantArtUri = ConfigurationService.DeviantArtConfiguration.BackendUrl;

        private const string urlObtainRegexpPrefix = "\"url\":\"";
        private const string urlObtainRegexpPostfix = "\"";
        private const string urlObtainRegexp = $"{urlObtainRegexpPrefix}[^\"]*{urlObtainRegexpPostfix}";

        private const string urlParameterWidthPrefix = "w_";
        private const string urlParameterHeightPrefix = "h_";
        private const string urlParameterQualityPrefix = "q_";

        private const string urlParameterWidthRegexp = $"{urlParameterWidthPrefix}\\d*";
        private const string urlParameterHeightRegexp = $"{urlParameterHeightPrefix}\\d*";
        private const string urlParameterQualityRegexp = $"{urlParameterQualityPrefix}\\d*";

    
        public static async Task<string?> GetImageFromSourceUrl(string sourceUrl, string authorName, string fileName)
        {
            //Заменяем параметр качества на 100, если есть
            var imageQualityParameterString = ObtainRegexp(sourceUrl, urlParameterQualityRegexp);
            if (!String.IsNullOrWhiteSpace(imageQualityParameterString))
            {
                if (int.TryParse(imageQualityParameterString.Replace(urlParameterQualityPrefix, String.Empty), out var qualityParameter))
                    sourceUrl = sourceUrl.Replace($"{urlParameterQualityPrefix}{qualityParameter}", $"{urlParameterQualityPrefix}100");
            }

            var response = await HttpHelper.GetAsync(sourceUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var contentType = response.Content.Headers.ContentType?.ToString();
                var fileExtension = MimeTypeMap.GetExtension(contentType);
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png" && fileExtension != ".webp")
                    return null;

                var responseStream = await response.Content.ReadAsStreamAsync();

                if (AppDataHelper.SaveFile("DeviantArt", authorName, responseStream, fileName, fileExtension, AppDataCatalog.DOWNLOADED, out var filePath))
                    return filePath;
            }

            return null;
        }


        private static async Task<string?> GetImageUrl(string publicationUrl)
        {
            if (backendDeviantArtUri == null)
                throw new Exception("BackendDeviantArtUrl was null. Check DeviantArt configuration.");

            var getImageInfoUrl = backendDeviantArtUri.SetQueryParam("url", publicationUrl);

            var response = await HttpHelper.GetAsync(getImageInfoUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var imageUrl = ObtainRegexp(responseString, urlObtainRegexp);
                if (!String.IsNullOrWhiteSpace(imageUrl))
                {
                    imageUrl = imageUrl.Replace(urlObtainRegexpPrefix, String.Empty);
                    imageUrl = imageUrl.Replace(urlObtainRegexpPostfix, String.Empty);
                    imageUrl = imageUrl.Replace("\\/", "/");

                    return imageUrl;
                }
            }

            return null;
        }

        /// <summary>
        /// Достает изображение из ссылки на публикацию, перебирая параметры размеров до поулчения максимального качества
        /// </summary>
        /// <param name="publicationUrl"></param>
        /// <param name="authorName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string?> GetImageFromPublicationUrl(string publicationUrl, string authorName, string fileName)
        {
            var imageUrl = await GetImageUrl(publicationUrl);

            if (!String.IsNullOrWhiteSpace(imageUrl))
            {
                var startWidthParameterValue = (int?)null;
                var startHeightParameterValue = (int?)null;
                var startQualityParameterValue = (int?)null;

                var imageWidthParameterString = ObtainRegexp(imageUrl, urlParameterWidthRegexp);
                if (!String.IsNullOrWhiteSpace(imageWidthParameterString))
                {
                    if (int.TryParse(imageWidthParameterString.Replace(urlParameterWidthPrefix, String.Empty), out var widthParameter))
                        startWidthParameterValue = widthParameter;
                }

                var imageHeightParameterString = ObtainRegexp(imageUrl, urlParameterHeightRegexp);
                if (!String.IsNullOrWhiteSpace(imageHeightParameterString))
                {
                    if (int.TryParse(imageHeightParameterString.Replace(urlParameterHeightPrefix, String.Empty), out var heightParameter))
                        startHeightParameterValue = heightParameter;
                }

                var imageQualityParameterString = ObtainRegexp(imageUrl, urlParameterQualityRegexp);
                if (!String.IsNullOrWhiteSpace(imageQualityParameterString))
                {
                    if (int.TryParse(imageQualityParameterString.Replace(urlParameterQualityPrefix, String.Empty), out var qualityParameter))
                        startQualityParameterValue = qualityParameter;
                }

                if (!startWidthParameterValue.HasValue || !startHeightParameterValue.HasValue || !startQualityParameterValue.HasValue)
                    return null;

                var offsetWidthParameterValue = startWidthParameterValue.Value;
                var offsetHeightParameterValue = startHeightParameterValue.Value;

                var additionalOffsetWidthParameterValue = startWidthParameterValue.Value;
                var additionalOffsetHeightParameterValue = startHeightParameterValue.Value;

                var currentWidthParameterValue = startWidthParameterValue.Value;
                var currentHeightParameterValue = startHeightParameterValue.Value;

                imageUrl = imageUrl.Replace($"{urlParameterQualityPrefix}{startQualityParameterValue}", $"{urlParameterQualityPrefix}100");
                var optimalImageUrl = imageUrl;
                var requestCounter = 0;

                var response = (HttpResponseMessage?)null;

                while (additionalOffsetWidthParameterValue != 0 && additionalOffsetHeightParameterValue != 0)
                {
                    var newWidthParameterValue = startWidthParameterValue.Value + offsetWidthParameterValue;
                    var newHeightParameterValue = startHeightParameterValue.Value + offsetHeightParameterValue;

                    imageUrl = imageUrl.Replace($"{urlParameterWidthPrefix}{currentWidthParameterValue}", $"{urlParameterWidthPrefix}{newWidthParameterValue}");
                    imageUrl = imageUrl.Replace($"{urlParameterHeightPrefix}{currentHeightParameterValue}", $"{urlParameterHeightPrefix}{newHeightParameterValue}");

                    currentWidthParameterValue = newWidthParameterValue;
                    currentHeightParameterValue = newHeightParameterValue;

                    response = await HttpHelper.GetAsync(imageUrl);
                    requestCounter++;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        optimalImageUrl = imageUrl;

                        offsetWidthParameterValue += additionalOffsetWidthParameterValue - 1;
                        offsetHeightParameterValue += additionalOffsetHeightParameterValue - 1;
                    }
                    else
                    {
                        additionalOffsetWidthParameterValue = additionalOffsetWidthParameterValue / 2;
                        additionalOffsetHeightParameterValue = additionalOffsetHeightParameterValue / 2;

                        offsetWidthParameterValue = offsetWidthParameterValue - additionalOffsetWidthParameterValue;
                        offsetHeightParameterValue = offsetHeightParameterValue - additionalOffsetHeightParameterValue;
                    }
                }

                response = await HttpHelper.GetAsync(optimalImageUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var contentType = response.Content.Headers.ContentType?.ToString();
                    var fileExtension = MimeTypeMap.GetExtension(contentType);
                    var responseStream = await response.Content.ReadAsStreamAsync();

                    if (AppDataHelper.SaveFile("DeviantArt", authorName, responseStream, fileName, fileExtension, AppDataCatalog.DOWNLOADED, out var filePath))
                        return filePath;
                }
            }

            return null;
        }


        //Utitlity

        private static string? ObtainRegexp(string source, string regexp)
        {
            Regex regex = new Regex(regexp);
            MatchCollection matches = regex.Matches(source);
            if (matches.Count > 0)
            {
                return matches[0].Value;
            }

            return null;
        }
    }
}
