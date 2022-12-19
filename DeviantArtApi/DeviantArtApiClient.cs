using Microsoft.Extensions.Configuration;
using SpecialLibraryBot.Services;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class DeviantArtApiClient
    {
        private readonly string? clientId;
        private readonly string? clientSecret;
        private readonly DeviantArtApiRestClient restClient;
        private HttpClient httpClient;
        private DateTime TokenExpires;

        private static DeviantArtApiClient? instance;
        public static DeviantArtApiClient Instance
        {
            get
            {
                if (instance == null)
                {
                    var configuration = ConfigurationService.DeviantArtConfiguration;
                    

                    instance = new DeviantArtApiClient(configuration);
                }

                return instance;
            }
        }


        private DeviantArtApiClient(DeviantArtConfiguration configuration)
        {
            if (String.IsNullOrWhiteSpace(configuration.ApiClientId) || String.IsNullOrWhiteSpace(configuration.ApiClientSecret) ||
                String.IsNullOrWhiteSpace(configuration.ApiBaseUrl) || String.IsNullOrWhiteSpace(configuration.ApiAuthBaseUrl))
                throw new ArgumentException("CientId or ClientSecret or ApiBaseUrl or ApiAuthBaseUrl was null.");

            clientId = configuration.ApiClientId;
            clientSecret = configuration.ApiClientSecret;

            restClient = new DeviantArtApiRestClient(configuration.ApiBaseUrl, configuration.ApiAuthBaseUrl);
            httpClient = new HttpClient();
            TokenExpires = DateTime.UtcNow;
        }


        public async Task<bool> Authenticate()
        {
            var getTokenUri = restClient.GetToken(clientId!, clientSecret!);

            var tokenResponse = await httpClient.GetAsync(getTokenUri.Uri);
            if (tokenResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Token response status code - {tokenResponse.StatusCode}");
            }

            var authJson = (AuthJson?)null;
            try
            {
                authJson = JsonConvert.DeserializeObject<AuthJson>(await tokenResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception("Can't deserealize response to AuthJson", ex);
            }

            if (authJson == null)
                throw new Exception("Response deserialization returned null.");

            TokenExpires = DateTime.UtcNow.AddSeconds(authJson.ExpiresIn).AddHours(-1);//-1 чтобы заранее обновлял
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authJson.TokenType} {authJson.AccessToken}");

            return true;
        }

        public async Task<GalleryAllJson> GalleryAll(string author, int pageLimit = 5, int pageOffset = 0, bool withMatureContent = true)
        {
            if(DateTime.UtcNow >= TokenExpires && !await Authenticate())
                throw new Exception("Cant authenticate client.");

            var getGetGalleryAllUri = restClient.GetGetGalleryAll(author, pageLimit, pageOffset, withMatureContent);

            var galleryResponse = await httpClient.GetAsync(getGetGalleryAllUri.Uri);
            if (galleryResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Token response status code - {galleryResponse.StatusCode}");
            }

            var galleryJson = (GalleryAllJson?)null;
            try
            { 
                galleryJson = JsonConvert.DeserializeObject<GalleryAllJson>(await galleryResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception("Can't deserealize response to GallaryAllJson", ex);
            }

            if (galleryJson == null)
                throw new Exception("Response deserialization returned null.");

            return galleryJson;
        }
    }
}
