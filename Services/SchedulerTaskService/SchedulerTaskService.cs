using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services.SchedulerTaskService
{
    public class SchedulerTaskService
    {
        private static ILogger logger;
        private static SchedulerTaskService? instance;
        public static SchedulerTaskService Instance
        {
            get
            {
                if(instance == null)
                    instance = new SchedulerTaskService();

                return instance;
            }
        }

        private List<SchedulerTask> tasks = new List<SchedulerTask>();

        private readonly int sleepIntervalMiliseconds = 30 * 1000;

        private SchedulerTaskService()
        {
            logger = LogManager.GetCurrentClassLogger();

            tasks.Add(SchedulerTaskDeviantArt.Instance);
            tasks.Add(SchedulerTaskBackup.Instance);
            tasks.Add(SchedulerTaskAlbumLoader.Instance);
            tasks.Add(SchedulerTaskOnModerationPublicationsClear.Instance);
        }

        public async Task Start()
        {
            var schedulerThread = new Thread(async () =>
            {
                while (true)
                {
                    var now = DateTime.UtcNow;

                    foreach (var task in tasks)
                    {
                        if (now >= task.NextExecutionDateTime())
                        {
                            await task.Execute();//лучше пусть выполняются последовательно
                        }
                    }

                    Thread.Sleep(sleepIntervalMiliseconds);
                }
            });

            schedulerThread.Start();
        }
    }
}
