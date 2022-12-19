using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using Nullforce.Api.DeviantArt.JsonModels;
using SpecialLibraryBot;
using SpecialLibraryBot.Helpers;
using SpecialLibraryBot.Services;
using SpecialLibraryBot.Services.SchedulerTaskService;
using SpecialLibraryBot.Telegram;
using SpecialLibraryBot.VK;
using System.Net.Http.Headers;


AppDomain.CurrentDomain.ProcessExit += new EventHandler((sender, eventArgs) =>
{
    AppDataHelper.SerializePublicationManager();
    AppDataHelper.SerializeDeviantArtSchedulerTask();
});

//Logger
LogManager.Configuration = new NLogLoggingConfiguration(ConfigurationService.NlogConfiguration);
var logger = LogManager.GetCurrentClassLogger();

//AppData
var deviantartConfiguration = ConfigurationService.DeviantArtConfiguration;
if (deviantartConfiguration.Authors != null)
{
    var sources = new Dictionary<string, List<string>>()
    {
        ["DeviantArt"] = deviantartConfiguration.Authors.ToList()
    };

    AppDataHelper.ObtainCatalogsStructure(sources);
}

//PublicationManager
logger.Info("Initializing Publication Manager...");
var publicationManager = PublicationManager.Instance;

//Telegram
var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

logger.Info("Starting telegram bot...");
TelegramBot.Instance.StartRecieving(cancellationToken);


//Scheduler
logger.Info("Starting schedulers...");
SchedulerTaskService.Instance.Start();


Console.ReadLine();