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

        public override async Task Execute()
        {
            Logger.Info("Start execute.");

            nextExecutionDateTime = DateTime.UtcNow.AddMinutes(executionIntervalMinutes);

            var deviantArtApiClient = DeviantArtApiClient.Instance;

            foreach(var author in Authors)
            {
                if(!AuthorsLastArtsDates.TryGetValue(author, out var lastArtsDate))
                {
                    lastArtsDate = LastUpdateDateTime;
                }

                var hasMoreWorks = true;
                var pageOffset = 0;
                var newLastArtDate = lastArtsDate;

                try
                {
                    while(hasMoreWorks)
                    {
                        var authorGallary = await deviantArtApiClient.GalleryAll(author, pageOffset: pageOffset);

                        if(authorGallary.Results == null)
                        {
                            Logger.Error($"Author ({author}) gallary deviantions was null.");
                            break;
                        }

                        for(var i = 0; i < authorGallary.Results.Length; i++)
                        {
                            var deviationJson = authorGallary.Results[i];
                            if(deviationJson.PublishedDateTime == null || deviationJson.PublishedDateTime <= lastArtsDate)
                            {
                                hasMoreWorks = false;
                                break;
                            }

                            var imageDownloadUrl = deviationJson.Content?.SourceUri;
                            if(imageDownloadUrl == null)
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
        }

        public override DateTime NextExecutionDateTime()
        {
            return nextExecutionDateTime;
        }
    }
}
