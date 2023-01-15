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
    public class SchedulerTaskOnModerationPublicationsClear : SchedulerTask
    {
        private DateTime nextExecutionDateTime;
        private int executionIntervalHours = 1;

        private static SchedulerTaskOnModerationPublicationsClear? instance;
        public static SchedulerTaskOnModerationPublicationsClear Instance
        {
            get
            {
                if (instance == null)
                    instance = new SchedulerTaskOnModerationPublicationsClear(LogManager.GetCurrentClassLogger());

                return instance;
            }
        }

        public SchedulerTaskOnModerationPublicationsClear(ILogger logger) : base(logger) 
        {
            nextExecutionDateTime = DateTime.MinValue;
        }

        public override async Task Execute()
        {
            try
            {
                Logger.Info("Start execute.");

                nextExecutionDateTime = DateTime.UtcNow.AddHours(executionIntervalHours);

                PublicationManager.ClearOldOnModerationPublications();
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
