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
    public class SchedulerTaskBackup : SchedulerTask
    {
        private DateTime nextExecutionDateTime;
        private int executionIntervalMinutes = 60;

        private static SchedulerTaskBackup? instance;
        public static SchedulerTaskBackup Instance
        {
            get
            {
                if (instance == null)
                    instance = new SchedulerTaskBackup(LogManager.GetCurrentClassLogger());

                return instance;
            }
        }

        public SchedulerTaskBackup(ILogger logger) : base(logger) 
        {
            nextExecutionDateTime = DateTime.MinValue;
        }

        public override async Task Execute()
        {
            try
            {
                Logger.Info("Start execute.");

                nextExecutionDateTime = DateTime.UtcNow.AddMinutes(executionIntervalMinutes);

                AppDataHelper.SerializePublicationManager();
                AppDataHelper.SerializeDeviantArtSchedulerTask();

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
