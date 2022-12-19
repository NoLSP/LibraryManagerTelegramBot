using NLog;
using SpecialLibraryBot.DeviantArtApi;
using SpecialLibraryBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services.SchedulerTaskService
{
    public class SchedulerTaskAlbumLoader : SchedulerTask
    {
        private DateTime nextExecutionDateTime;
        private int executionIntervalDays = 7;

        private static SchedulerTaskAlbumLoader? instance;
        public static SchedulerTaskAlbumLoader Instance
        {
            get
            {
                if (instance == null)
                    instance = new SchedulerTaskAlbumLoader(LogManager.GetCurrentClassLogger());

                return instance;
            }
        }

        public SchedulerTaskAlbumLoader(ILogger logger) : base(logger) 
        {
            nextExecutionDateTime = DateTime.MinValue;
        }

        public override async Task Execute()
        {
            try
            {
                Logger.Info("Start execute.");

                nextExecutionDateTime = DateTime.UtcNow.AddDays(executionIntervalDays);

                PublicationManager.LoadPublishedPublicationsToAlbums();

            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}");
            }
        }

        public override DateTime NextExecutionDateTime()
        {
            return nextExecutionDateTime;
        }
    }
}
