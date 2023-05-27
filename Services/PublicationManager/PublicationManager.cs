using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Configuration;
using NLog;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.Telegram;
using SpecialLibraryBot.VK;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;

namespace SpecialLibraryBot
{
    public class PublicationManager
    {
        private ILogger logger;
        private PublicationManagerConfiguration configuration;

        public Dictionary<string, PublicationEntity> Publications;
        private Semaphore Semaphore;
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
            Semaphore = new Semaphore(1, 1);

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

        
        //Publication time managment
        
        private static DateTime GetActualPublicationDateTime()
        {
            if (Instance.LastPublicationDateTime.Date < DateTime.UtcNow.Date)
                Instance.LastPublicationDateTime = DateTime.UtcNow;

            var publicationDateTime = GetOccurrence(Instance.LastPublicationDateTime, Instance.StartPublicationTime, Instance.EndPublicationTime, Instance.PublicationIntervalMinutes, out var reason);
            if (publicationDateTime == null)
                throw new Exception($"Could not get next publication datetime. LastPublicationDateTime = {Instance.LastPublicationDateTime}, StartTime = {Instance.StartPublicationTime}, EndTime = {Instance.EndPublicationTime}, IntervalMinutes = {Instance.PublicationIntervalMinutes}");

            return publicationDateTime.Value;
        }

        static DateTime? GetOccurrence(DateTime lastPublicationDateTime, TimeOnly startTime, TimeOnly endTime, int intervalMinutes, out string reason)
        {
            var startDateTime = lastPublicationDateTime;
            var endDateTime = startDateTime.AddMonths(1);

            var recurenceRule = new RecurrencePattern(Ical.Net.FrequencyType.Minutely, intervalMinutes);

            while (startDateTime <= endDateTime)
            {
                var vEvent = new CalendarEvent
                {
                    Start = new CalDateTime(new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startTime.Hour, startTime.Minute, startTime.Second)),
                    End = new CalDateTime(new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, endTime.Hour, endTime.Minute, endTime.Second)),
                    RecurrenceRules = new List<RecurrencePattern>() { recurenceRule }
                };

                //Получаем события для текущего дня
                var occurrences = vEvent.GetOccurrences(startDateTime, endDateTime)
                    .Where(x => new TimeOnly(x.Period.StartTime.Hour, x.Period.StartTime.Minute, x.Period.StartTime.Second) >= startTime &&
                        new TimeOnly(x.Period.StartTime.Hour, x.Period.StartTime.Minute, x.Period.StartTime.Second) <= endTime)
                    .ToList();

                foreach (var occurrence in occurrences)
                {
                    var occurrDateTime = new DateTime(occurrence.Period.StartTime.Year, occurrence.Period.StartTime.Month, occurrence.Period.StartTime.Day,
                        occurrence.Period.StartTime.Hour, occurrence.Period.StartTime.Minute, occurrence.Period.StartTime.Second);

                    if (occurrDateTime > lastPublicationDateTime && occurrDateTime > DateTime.UtcNow)
                    {
                        reason = "";
                        return occurrDateTime;
                    }
                }

                startDateTime.AddDays(1);
            }

            reason = "Occurences not found";
            return null;
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

        public static async Task<bool> RegisterPublicationEntity(PublicationEntity publicationEntity)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (await TelegramBotManager.SendPublicationEntity(publicationEntity))
                {
                    var newImageFilePath = AppDataHelper.MoveToProcessing(publicationEntity.SocialNetwork, publicationEntity.Author, publicationEntity.ImageFilePath);
                    publicationEntity.ImageFilePath = newImageFilePath;
                    publicationEntity.State = PublicationState.Moderation;
                    publicationEntity.StateDateTime = DateTime.UtcNow;
                }
                else
                {
                    publicationEntity.State = PublicationState.Downloaded;
                    publicationEntity.StateDateTime = DateTime.UtcNow;
                }

                Instance.Publications.Add(publicationEntity.Id, publicationEntity);
                return true;

            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool ChangeAuthorCatalog(string authorOldName, string authorNewName)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                foreach (var publication in instance!.Publications.Values.Where(x => x.Author == authorOldName))
                {
                    publication.ImageFilePath.Replace(authorOldName, authorNewName);
                }

                return true;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool DeleteAuthorPublications(string authorName)
        {
            Instance.Semaphore.WaitOne();

            try
            { 
                var publicationsToDeleteIds = instance!.Publications.Values
                    .Where(x => x.Author == authorName)
                    .Select(x => x.Id);

                foreach (var publicationId in publicationsToDeleteIds)
                {
                    instance!.Publications.Remove(publicationId);
                }

                return true;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool ObtainAuthorsAlbums(List<string> authors, out string reason)
        {
            return Instance.vkApiClient.ObtainAlbums(authors, out reason);
        }

        public static bool TryGetPublicationEntity(string id, out PublicationEntity? publication)
        {
            Instance.Semaphore.WaitOne();

            try
            { 
                if (Instance.Publications.TryGetValue(id, out publication))
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                publication = null;
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }


        public static bool DeletePublication(PublicationEntity publication, bool deleteFile)
        {
            Instance.Semaphore.WaitOne();

            try
            { 
                if (deleteFile)
                    AppDataHelper.DeletePublicationFile(publication.ImageFilePath);

                Instance.Publications.Remove(publication.Id);
                return true;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool DeletePublication(string publicationId, bool deleteFile)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (Instance.Publications.TryGetValue(publicationId, out var publication))
                {
                    Instance.Semaphore.Release();
                    return DeletePublication(publication, deleteFile);
                }

                return false;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }


        public static bool ManualProcessingPublication(PublicationEntity publication)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if(publication.State != PublicationState.Moderation)
                    return false;

                AppDataHelper.MoveToManual(publication.SocialNetwork, publication.Author, publication.ImageFilePath);

                publication.State = PublicationState.ManualProcessing;
                publication.StateDateTime = DateTime.UtcNow;

                return true;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool ManualProcessingPublication(string publicationId)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (Instance.Publications.TryGetValue(publicationId, out var publication))
                {
                    Instance.Semaphore.Release();
                    return ManualProcessingPublication(publication);
                }

                return false;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }


        public static bool PublicatePublication(PublicationEntity publication)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (publication.State != PublicationState.Moderation)
                    return false;

                publication.PublicationDateTime = GetActualPublicationDateTime();

                if (!Instance.vkApiClient.PostPublication(publication))
                {
                    return false;
                }

                publication.State = PublicationState.Published;
                publication.StateDateTime = DateTime.UtcNow;
                Instance.LastPublicationDateTime = publication.PublicationDateTime.Value;

                publication.ImageFilePath = AppDataHelper.MoveToOnWall(publication.SocialNetwork, publication.Author, publication.ImageFilePath);

                return true;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool PublicatePublication(string publicationId)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (Instance.Publications.TryGetValue(publicationId, out var publication))
                {
                    Instance.Semaphore.Release();
                    return PublicatePublication(publication);
                }

                return false;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }


        public static bool LoadPublishedPublicationsToAlbums()
        {
            Instance.Semaphore.WaitOne();

            var publications = Instance.Publications.Values
                .Where(x => x.State == PublicationState.Published)
                .ToList();

            Instance.Semaphore.Release();

            foreach (var publication in publications)
            {
                PublicateToAlbum(publication);
            }

            return true;
        }

        public static bool ClearOldOnModerationPublications()
        {
            Instance.Semaphore.WaitOne();

            var publications = Instance.Publications.Values
                .Where(x => x.State == PublicationState.Moderation)
                .Where(x => x.StateDateTime == null || x.StateDateTime <= DateTime.UtcNow.AddDays(-7))
                .ToList();

            Instance.Semaphore.Release();

            foreach (var publication in publications)
            {
                DeletePublication(publication, true);
            }

            return true;
        }


        public static bool MoveToAlbum(PublicationEntity publication)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (publication.State != PublicationState.Moderation)
                    return false;

                publication.State = PublicationState.Published;
                publication.StateDateTime = DateTime.UtcNow;
                publication.PublicationDateTime = DateTime.UtcNow;

                //Перемещаем в папку "на стене" чтобы робот потом перенес в альбом
                publication.ImageFilePath = AppDataHelper.MoveToOnWall(publication.SocialNetwork, publication.Author, publication.ImageFilePath);

                return true;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }

        public static bool MoveToAlbum(string publicationId)
        {
            Instance.Semaphore.WaitOne();

            try
            {
                if (Instance.Publications.TryGetValue(publicationId, out var publication))
                {
                    Instance.Semaphore.Release();
                    return MoveToAlbum(publication);
                }

                return false;
            }
            catch (Exception e)
            {
                Instance.logger.Error(e);
                return false;
            }
            finally
            {
                Instance.Semaphore.Release();
            }
        }


        public static bool PublicateToAlbum(PublicationEntity publication)
        {
            if(publication.State != PublicationState.Published)
                return false;

            if (!Instance.vkApiClient.LoadPhotosToAlbum(publication.Author, new List<string>() { publication.ImageFilePath }, out var reason))
            {
                Instance.logger.Error(reason);
                return false;
            }
            publication.ImageFilePath = AppDataHelper.MoveToInAlbum(publication.SocialNetwork, publication.Author, publication.ImageFilePath);
            DeletePublication(publication, false);

            return true;
        }

        public static bool PublicateToAlbum(string publicationId)
        {
            if (Instance.Publications.TryGetValue(publicationId, out var publication))
            {
                return PublicateToAlbum(publication);
            }

            return false;
        }

    }
}
