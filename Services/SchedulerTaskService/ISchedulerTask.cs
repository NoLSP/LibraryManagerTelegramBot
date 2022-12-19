using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services.SchedulerTaskService
{
    public abstract class SchedulerTask
    {
        protected ILogger Logger;

        protected SchedulerTask(ILogger logger)
        {
            Logger = logger;
        }

        public abstract DateTime NextExecutionDateTime();
        public virtual async Task Execute() { }
    }
}
