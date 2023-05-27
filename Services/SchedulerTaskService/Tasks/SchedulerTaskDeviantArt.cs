using NLog;
using SpecialLibraryBot.DeviantArtApi;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services.SchedulerTaskService
{
    public class SchedulerTaskDeviantArt : SchedulerTask
    {
        private DateTime nextExecutionDateTime;
        private int executionIntervalMinutes = 10;
        public Dictionary<string, DateTime> AuthorsLastArtsDates;
        public List<string> Authors;
        private static Semaphore Semaphore = new Semaphore(1, 1);
        private DeviantArtConfiguration configuration;
        public DateTime LastUpdateDateTime;

        private static SchedulerTaskDeviantArt? instance;
        public static SchedulerTaskDeviantArt Instance
        {
            get
            {
                if (instance == null)
                    instance = new SchedulerTaskDeviantArt(LogManager.GetCurrentClassLogger());

                return instance;
            }
        }

        private SchedulerTaskDeviantArt(ILogger logger) : base(logger) 
        {
            nextExecutionDateTime = DateTime.MinValue;
            configuration = ConfigurationService.DeviantArtConfiguration;

            //Сначала пытаемся восставновиться из сохранения
            var schedulerTaskDeviantArtDto = AppDataHelper.DeserializeDeviantArtSchedulerTask();
            if (schedulerTaskDeviantArtDto != null)
            {
                AuthorsLastArtsDates = schedulerTaskDeviantArtDto.AuthorsLastArtsDates ?? new Dictionary<string, DateTime>();
                Authors = schedulerTaskDeviantArtDto.Authors ?? new List<string>();
                LastUpdateDateTime = schedulerTaskDeviantArtDto.LastUpdateDateTime;
            }
            else
            {
                //Иначе инициализируемся
                AuthorsLastArtsDates = new Dictionary<string, DateTime>();
                
                if (configuration.Authors == null)
                {
                    throw new Exception("Authors is empty.");
                }
                Authors = configuration.Authors.ToList();

                if (!configuration.LastUpdateDateTimeUtc.HasValue)
                {
                    throw new Exception("LastUpdateDateTime was null.");
                }
                LastUpdateDateTime = configuration.LastUpdateDateTimeUtc.Value;
            }

            if(!PublicationManager.ObtainAuthorsAlbums(Authors, out var reason))
                logger.Error(reason);
        }

        public static bool AddAuthor(string authorName, out string reason)
        {
            Semaphore.WaitOne();
            if (instance!.Authors.Contains(authorName))
            {
                reason = "Автор с таким имененем уже есть в базе.";
                Semaphore.Release();
                return false;
            }

            instance!.Authors.Add(authorName);
            instance!.AuthorsLastArtsDates.Add(authorName, new DateTime());
            AppDataHelper.ObtainCatalogsStructure(new Dictionary<string, List<string>>()
            {
                ["DeviantArt"] = instance!.Authors
            });
            reason = "";
            Semaphore.Release();
            return true;
        }

        public static bool EditAuthor(string authorOldName, string authorNewName, out string reason)
        {
            Semaphore.WaitOne();
            if (!instance!.Authors.Contains(authorOldName))
            {
                reason = "Автор с таким имененем не найден в базе.";
                Semaphore.Release();
                return false;
            }

            if (instance!.Authors.Contains(authorNewName))
            {
                reason = "Автор с таким имененем уже есть в базе.";
                Semaphore.Release();
                return false;
            }

            instance!.Authors.Remove(authorOldName);
            instance!.Authors.Add(authorNewName);
            instance!.AuthorsLastArtsDates[authorNewName] = instance!.AuthorsLastArtsDates[authorOldName];
            instance!.AuthorsLastArtsDates.Remove(authorOldName);

            AppDataHelper.MoveAuthorCatalog("DeviantArt", authorOldName, authorNewName);
            PublicationManager.ChangeAuthorCatalog(authorOldName, authorNewName);

            reason = "";
            Semaphore.Release();
            return true;
        }

        public static bool DeleteAuthor(string authorName, out string reason)
        {
            Semaphore.WaitOne();
            if (!instance!.Authors.Contains(authorName))
            {
                reason = "Автор с таким имененем не найден в базе.";
                Semaphore.Release();
                return false;
            }

            instance!.Authors.Remove(authorName);
            instance!.AuthorsLastArtsDates.Remove(authorName);
            AppDataHelper.DeleteAuthorCatalog("DeviantArt", authorName);
            reason = "";
            Semaphore.Release();
            return true;
        }

        public override async Task Execute()
        {
            Semaphore.WaitOne();
            Logger.Info("Start execute.");


            var deviantArtApiClient = DeviantArtApiClient.Instance;

            foreach (var author in Authors)
            {
                if (!AuthorsLastArtsDates.TryGetValue(author, out var lastArtsDate))
                {
                    lastArtsDate = LastUpdateDateTime;
                }

                var hasMoreWorks = true;
                var pageOffset = 0;
                var newLastArtDate = lastArtsDate;

                try
                {
                    while (hasMoreWorks)
                    {
                        var authorGallary = await deviantArtApiClient.GalleryAll(author, pageOffset: pageOffset);

                        if (authorGallary.Results == null)
                        {
                            Logger.Error($"Author ({author}) gallary deviantions was null.");
                            break;
                        }

                        if (authorGallary.Results.Length == 0)
                        {
                            hasMoreWorks = false;
                            break;
                        }

                        for (var i = 0; i < authorGallary.Results.Length; i++)
                        {
                            var deviationJson = authorGallary.Results[i];
                            if (deviationJson.PublishedDateTime == null || deviationJson.PublishedDateTime <= lastArtsDate)
                            {
                                hasMoreWorks = false;
                                break;
                            }

                            var imageDownloadUrl = deviationJson.Content?.SourceUri;
                            if (imageDownloadUrl == null)
                            {
                                Logger.Error($"ImageUrl was null {author}.");
                            }
                            else
                            {
                                var publicationTitle = deviationJson.Title!;
                                var imagePhysicalPath = await DeviantArtImageHelper.GetImageFromSourceUrl(imageDownloadUrl.ToString(), author, publicationTitle!);
                                if (imagePhysicalPath == null)
                                {
                                    Logger.Error($"Could not save image ({author}) ({publicationTitle}) ({imageDownloadUrl})");
                                }
                                else
                                {
                                    var publicationEntity = PublicationEntity.DeviantArt(
                                        author,
                                        publicationTitle,
                                        imagePhysicalPath
                                        );

                                    await PublicationManager.RegisterPublicationEntity(publicationEntity);
                                }

                                if (newLastArtDate < deviationJson.PublishedDateTime)
                                    newLastArtDate = deviationJson.PublishedDateTime.Value;
                            }

                            if (i == authorGallary.Results.Length - 1)
                            {
                                pageOffset += 5;
                                hasMoreWorks = true;
                            }
                        }
                    }

                    AuthorsLastArtsDates[author] = newLastArtDate;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    await TelegramBotManager.SendException($"Выгрузка автора {author}", ex.Message);
                }
            }

            nextExecutionDateTime = DateTime.UtcNow.AddMinutes(executionIntervalMinutes);

            Semaphore.Release();
        }

        public override DateTime NextExecutionDateTime()
        {
            return nextExecutionDateTime;
        }
    }
}
