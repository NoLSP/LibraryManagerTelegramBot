using Microsoft.Extensions.Configuration;
using NLog;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.Telegram;
using SpecialLibraryBot.VK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot
{
    public class PublicationManager
    {
        private ILogger logger;
        private PublicationManagerConfiguration configuration;

        public Dictionary<string, PublicationEntity> Publications;
        public DateTime LastPublicationDateTime;
        public TimeOnly StartPublicationTime;
        public TimeOnly EndPublicationTime;
        public int PublicationIntervalMinutes;

        private VKApiClient vkApiClient;

        private static PublicationManager? instance;
        public static PublicationManager Instance
        {
            get
            {
                if(instance == null)
                    instance = new PublicationManager();

                return instance;
            }
        }

        private PublicationManager()
        {
            configuration = ConfigurationService.PublicationManagerConfiguration;
            logger = LogManager.GetCurrentClassLogger();
            vkApiClient = VKApiClient.Instance;

            //Сначала пытаемся восставновиться из сохранения
            var publicationManagerDto = AppDataHelper.DeserializePublicationManager();
            if (publicationManagerDto != null)
            {
                Publications = publicationManagerDto.Publications ?? new Dictionary<string, PublicationEntity>();
                LastPublicationDateTime = publicationManagerDto.LastPublicationDateTime;
                StartPublicationTime = TimeOnly.Parse(publicationManagerDto.StartPublicationTime);
                EndPublicationTime = TimeOnly.Parse(publicationManagerDto.EndPublicationTime);
                PublicationIntervalMinutes = publicationManagerDto.PublicationIntervalMinutes;
            }
            else
            {
                //Иначе инициализируемся
                Publications = new Dictionary<string, PublicationEntity>();
                LastPublicationDateTime = configuration.LastPublicationDateTimeUtc ?? DateTime.UtcNow;
                StartPublicationTime = configuration.StartPublicationTimeUtc ?? TimeOnly.Parse("10:00:00");
                EndPublicationTime = configuration.EndPublicationTimeUtc ?? TimeOnly.Parse("20:00:00");
                PublicationIntervalMinutes = configuration.PublicationIntervalMinutes ?? 240;
            }
        }

        public static async Task RegisterPublicationEntity(PublicationEntity publicationEntity)
        {
            if (await TelegramBot.SendPublicationEntity(publicationEntity))
            {
                var newImageFilePath = AppDataHelper.MoveToProcessing(publicationEntity.SocialNetwork, publicationEntity.Author, publicationEntity.ImageFilePath);
                publicationEntity.ImageFilePath = newImageFilePath;
                publicationEntity.State = PublicationState.Moderation;
            }
            else
            {
                publicationEntity.State = PublicationState.Downloaded;
            }

            Instance.Publications.Add(publicationEntity.Id, publicationEntity);
        }

        
        //Publication time managment
        
        private static DateTime GetActualPublicationDateTime()
        {
            var publicationDateTime = Instance.LastPublicationDateTime.AddMinutes(Instance.PublicationIntervalMinutes);

            if (!IsDateInActualPublicationPeriod(publicationDateTime))
                publicationDateTime = GetStartDateTimeInNextPeriod(publicationDateTime);

            return publicationDateTime;
        }

        private static DateTime GetStartDateTimeInNextPeriod(DateTime date)
        {
            var startTime = Instance.StartPublicationTime;
            date = TimeOnly.FromDateTime(date) <= startTime ? date : date.AddDays(1);
            var dateTime = new DateTime(date.Year, date.Month, date.Day, startTime.Hour, startTime.Minute, startTime.Second);

            return dateTime;
        }

        private static bool IsDateInActualPublicationPeriod(DateTime date)
        {
            var startTime = Instance.StartPublicationTime;
            var endTime = Instance.EndPublicationTime;

            return date.Date >= DateTime.UtcNow.Date &&
                (date.Hour > startTime.Hour || date.Hour == startTime.Hour && (date.Minute > startTime.Minute || date.Minute == startTime.Minute && (date.Second >= startTime.Second))) &&
                (date.Hour < endTime.Hour || date.Hour == endTime.Hour && (date.Minute < endTime.Minute || date.Minute == endTime.Minute && (date.Second <= endTime.Second)));
        }


        //Publication manipulations

        public static bool ObtainAuthorsAlbums(List<string> authors, out string reason)
        {
            return Instance.vkApiClient.ObtainAlbums(authors, out reason);
        }

        public static PublicationEntity? GetPublicationEntity(string id)
        {
            if (Instance.Publications.TryGetValue(id, out var publication))
                return publication;

            return null;
        }


        public static bool DeletePublication(PublicationEntity publication)
        {
            return DeletePublication(publication.Id);
        }

        public static bool DeletePublication(string publicationId)
        {
            if (Instance.Publications.TryGetValue(publicationId, out var publicaion) && publicaion.State == PublicationState.Moderation)
            {
                if(File.Exists(publicaion.ImageFilePath))
                    File.Delete(publicaion.ImageFilePath);

                Instance.Publications.Remove(publicationId);
                
                return true;
            }

            return false;
        }


        public static bool ManualProcessingPublication(PublicationEntity publication)
        {
            return ManualProcessingPublication(publication.Id);
        }

        public static bool ManualProcessingPublication(string publicationId)
        {
            if (Instance.Publications.TryGetValue(publicationId, out var publicaion) && publicaion.State == PublicationState.Moderation)
            {
                AppDataHelper.MoveToManual(publicaion.SocialNetwork, publicaion.Author, publicaion.ImageFilePath);

                publicaion.State = PublicationState.ManualProcessing;

                return true;
            }

            return false;
        }


        public static bool PublicatePublication(PublicationEntity publication)
        {
            return PublicatePublication(publication.Id);
        }

        public static bool PublicatePublication(string publicationId)
        {
            if (!Instance.Publications.TryGetValue(publicationId, out var publication) || publication.State != PublicationState.Moderation)
            {
                return false;
            }

            publication.PublicationDateTime = GetActualPublicationDateTime();

            if(!Instance.vkApiClient.PostPublication(publication))
            {
                return false;
            }

            publication.State = PublicationState.Published;
            Instance.LastPublicationDateTime = publication.PublicationDateTime.Value;

            publication.ImageFilePath = AppDataHelper.MoveToOnWall(publication.SocialNetwork, publication.Author, publication.ImageFilePath);

            return true;
        }


        public static bool LoadPublishedPublicationsToAlbums()
        {
            var publications = Instance.Publications.Values
                .Where(x => x.State == PublicationState.Published)
                .ToList();

            foreach(var publication in publications)
            {
                if(!Instance.vkApiClient.LoadPhotosToAlbum(publication.Author, new List<string>() { publication.ImageFilePath }, out var reason))
                {
                    Instance.logger.Error(reason);
                    continue;
                }

                MoveToInAlbum(publication);
                Instance.Publications.Remove(publication.Id);
            }

            return true;
        }



        public static bool MoveToAlbum(PublicationEntity publication)
        {
            return MoveToAlbum(publication.Id);
        }

        public static bool MoveToAlbum(string publicationId)
        {
            if (!Instance.Publications.TryGetValue(publicationId, out var publication) || publication.State != PublicationState.Moderation)
            {
                return false;
            }

            publication.State = PublicationState.Published;
            publication.PublicationDateTime = DateTime.UtcNow;

            //Перемещаем в папку "на стене" чтобы робот потом перенес в альбом
            publication.ImageFilePath = AppDataHelper.MoveToOnWall(publication.SocialNetwork, publication.Author, publication.ImageFilePath);

            return true;
        }


        public static bool MoveToInAlbum(PublicationEntity publication)
        {
            return MoveToInAlbum(publication.Id);
        }

        public static bool MoveToInAlbum(string publicationId)
        {
            if (!Instance.Publications.TryGetValue(publicationId, out var publicaion) || publicaion.State != PublicationState.Moderation)
            {
                return false;
            }

            AppDataHelper.MoveToInAlbum(publicaion.SocialNetwork, publicaion.Author, publicaion.ImageFilePath);

            return true;
        }

    }
}
