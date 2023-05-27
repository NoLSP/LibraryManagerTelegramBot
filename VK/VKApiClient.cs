using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.RequestParams;
using VkNet.Model;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.Attachments;
using SpecialLibraryBot.Services;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using NLog;

namespace SpecialLibraryBot.VK
{
    public class VKApiClient
    {
        private readonly string userName;
        private readonly long userId;
        private readonly ulong groupId;
        private readonly VkApi vkApi;
        private Dictionary<string, long> authorsAlbums;

        private ILogger logger;

        private static VKApiClient? client;
        public static VKApiClient Instance
        {
            get
            {
                if (client == null)
                {
                    client = new VKApiClient();
                }

                return client;
            }
        }

        private VKApiClient()
        {
            logger = LogManager.GetCurrentClassLogger();

            var configuration = ConfigurationService.VKConfiguration;
            if (String.IsNullOrWhiteSpace(configuration.UserName) || String.IsNullOrWhiteSpace(configuration.AccessToken) || String.IsNullOrWhiteSpace(configuration.UserName))
                throw new Exception("VK Api - userName or accessToken or groupName was null.");

            userName = configuration.UserName;

            vkApi = new VkApi();

            vkApi.Authorize(new ApiAuthParams
            {
                AccessToken = configuration.AccessToken
            });

            var user = vkApi.Users.Get(new List<string> { userName }, ProfileFields.All).FirstOrDefault();
            if (user == null)
                throw new Exception($"VK Api - user ({userName}) was null.");
            userId = user.Id;

            var group = vkApi.Groups.Get(new GroupsGetParams() { UserId = userId, Extended = true, Filter = GroupsFilters.Moderator, Fields = GroupsFields.All })
                .FirstOrDefault(x => x.ScreenName == configuration.GroupName);

            if (group == null)
                throw new Exception("VK Api - can't get group.");

            groupId = (ulong)group.Id;

            authorsAlbums = new Dictionary<string, long>();
        }


        public bool PostPublication(PublicationEntity publication)
        {
            try
            {
                var photos = UploadPhotosToWall(new List<string>() { publication.ImageFilePath });
                if (photos == null)
                {
                    return false;
                }

                var postId = vkApi.Wall.Post(new WallPostParams()
                {
                    OwnerId = (-1) * (long)groupId,
                    FromGroup = true,
                    Message = publication.Title,
                    PublishDate = publication.PublicationDateTime!.Value.AddHours(5),
                    Copyright = publication.Source,
                    Attachments = photos
                });

                return true;
            }
            catch (Exception ex)
            {
                Instance.logger.Error(ex);
                return false;
            }
        }

        public bool ObtainAlbums(List<string> authors, out string reason)
        {
            try
            {
                var albums = Instance.vkApi.Photo.GetAlbums(new PhotoGetAlbumsParams
                {
                    OwnerId = (-1) * (long)Instance.groupId
                });

                var completeAuthors = new List<string>();

                foreach (var album in albums)
                {
                    var author = authors.FirstOrDefault(x => x == album.Title);
                    if(author != null)
                    {
                        if(!authorsAlbums.ContainsKey(author))
                        {
                            authorsAlbums.Add(author, album.Id);
                        }
                        completeAuthors.Add(author);
                    }
                }

                if(completeAuthors.Count != authors.Count())
                {
                    reason = $"Не всем авторам нашелся альбом, проверьте коректность названий альбомов, либо создайте новый. Авторы без альбома: {String.Join(",", authors.Where(x => !completeAuthors.Contains(x)))}";
                    return false;
                }

                reason = "";
                return true;
            }
            catch(Exception e)
            {
                reason = e.Message;
                return false;
            }
        }

        public bool LoadPhotosToAlbum(string author, List<string> filePaths, out string reason)
        {
            try
            {
                if (!authorsAlbums.TryGetValue(author, out var albumId))
                {
                    reason = $"Не удалось найти альбом для автора {author}";
                    return false;
                }

                var uploadServer = Instance.vkApi.Photo.GetUploadServer(albumId, (long)Instance.groupId);
                var uploadUrl = uploadServer.UploadUrl;

                foreach (var filePath in filePaths)
                {
                    var responseString = UploadFile(uploadUrl, filePath);

                    var photos = Instance.vkApi.Photo.Save(new PhotoSaveParams
                    {
                        SaveFileResponse = responseString,
                        AlbumId = albumId,
                        GroupId = (long)groupId
                    });
                }

                reason = "";
                return true;
            }
            catch (Exception e)
            {
                reason = e.Message;
                return false;
            }
        }


        private List<Photo>? UploadPhotosToWall(List<string> filePaths)
        {
            try
            {
                var result = new List<Photo>();
                var uploadServer = Instance.vkApi.Photo.GetWallUploadServer((long)Instance.groupId);
                var uploadUrl = uploadServer.UploadUrl;

                foreach (var filePath in filePaths)
                {
                    var responseString = UploadFile(uploadUrl, filePath);
                    var photo = Instance.vkApi.Photo.SaveWallPhoto(responseString, null, Instance.groupId).FirstOrDefault();

                    if (photo == null)
                    {
                        Instance.logger.Error($"Can't upload photo - {filePath}");
                        return null;
                    }

                    result.Add(photo);
                }

                return result;
            }
            catch(Exception ex)
            {
                Instance.logger.Error(ex);
                return null;
            }
        }

        private string UploadFile(string uploadUrl, string filePath)
        {
            WebClient webClient = new WebClient();
            byte[] responseArray = webClient.UploadFile(uploadUrl, filePath);
            return System.Text.Encoding.ASCII.GetString(responseArray);
        }

    }
}
