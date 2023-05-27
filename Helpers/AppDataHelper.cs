using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using SpecialLibraryBot.Services.SchedulerTaskService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Helpers
{
    public static class AppDataCatalog
    {
        public const string DOWNLOADED = "Downloaded";
        public const string PROCESSING = "Processing";
        public const string MANUAL = "Manual";
        public const string ONWALL = "OnWall";
        public const string INALBUM = "InAlbum";
        public const string APPDATA = "AppData";
        public const string TELEGRAM = "Telegram";
        public const string MEMORY = "Memory";
        public const string PUBLICATIONMANAGER = "PublicationManager";
        public const string DEVIANTARTSCHEDULER = "DeviantArtScheduler";
    }

    public static class AppDataFile
    {
        public const string TELEGRAMCHATIDS = "ChatIds.txt";
        public const string PUBLICATIONMANAGER = "PublicationManager.json";
        public const string DEVIANTARTSCHEDULER = "DeviantArtScheduler.json";
    }

    public static class AppDataHelper
    {
        private static string? rootPath;
        private static string RootPath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(rootPath))
                    rootPath = AppDomain.CurrentDomain.BaseDirectory;

                return rootPath;
            }
        }

        private static ILogger? logger;
        private static ILogger Logger
        {
            get
            {
                if (logger == null)
                    logger = LogManager.GetCurrentClassLogger();

                return logger;
            }
        }

        private static string[] fileNameInvalidSymbols = new string[] { "+","=","[","]",":","*","?",";","«",",",",",".","/","\\","<",">","|" };


        public static void ObtainCatalogsStructure(Dictionary<string, List<string>> sources)
        {
            var appDataDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.APPDATA));

            foreach (var source in sources.Keys)
            {
                var sourceDirectoryInfo = ObtainDirectory(Path.Combine(appDataDirectoryInfo.FullName, source));

                foreach (var author in sources[source])
                {
                    var authorDirectoryInfo = ObtainDirectory(Path.Combine(sourceDirectoryInfo.FullName, author));

                    var downloadedDirectoryInfo = ObtainDirectory(Path.Combine(authorDirectoryInfo.FullName, AppDataCatalog.DOWNLOADED));
                    var processingDirectoryInfo = ObtainDirectory(Path.Combine(authorDirectoryInfo.FullName, AppDataCatalog.PROCESSING));
                    var manualDirectoryInfo = ObtainDirectory(Path.Combine(authorDirectoryInfo.FullName, AppDataCatalog.MANUAL));
                    var onWallDirectoryInfo = ObtainDirectory(Path.Combine(authorDirectoryInfo.FullName, AppDataCatalog.ONWALL));
                    var inAlbumDirectoryInfo = ObtainDirectory(Path.Combine(authorDirectoryInfo.FullName, AppDataCatalog.INALBUM));

                }
            }
        }

        public static bool MoveAuthorCatalog(string socialNetwork, string sourceAuthor, string destinationAuthor)
        {
            try
            {
                var appDataDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.APPDATA));
                var socialNetworkDirectoryInfo = ObtainDirectory(Path.Combine(appDataDirectoryInfo.FullName, socialNetwork));
                var sourceAuthorDirectoryInfo = ObtainDirectory(Path.Combine(socialNetworkDirectoryInfo.FullName, sourceAuthor));

                var sourceDownloadedDirectoryInfo = ObtainDirectory(Path.Combine(sourceAuthorDirectoryInfo.FullName, AppDataCatalog.DOWNLOADED));
                var sourceProcessingDirectoryInfo = ObtainDirectory(Path.Combine(sourceAuthorDirectoryInfo.FullName, AppDataCatalog.PROCESSING));
                var sourceManualDirectoryInfo = ObtainDirectory(Path.Combine(sourceAuthorDirectoryInfo.FullName, AppDataCatalog.MANUAL));
                var sourceOnWallDirectoryInfo = ObtainDirectory(Path.Combine(sourceAuthorDirectoryInfo.FullName, AppDataCatalog.ONWALL));
                var sourceInAlbumDirectoryInfo = ObtainDirectory(Path.Combine(sourceAuthorDirectoryInfo.FullName, AppDataCatalog.INALBUM));

                var destinationAuthorDirectoryInfo = ObtainDirectory(Path.Combine(socialNetworkDirectoryInfo.FullName, destinationAuthor));

                var destinationDownloadedDirectoryInfo = ObtainDirectory(Path.Combine(destinationAuthorDirectoryInfo.FullName, AppDataCatalog.DOWNLOADED));
                var destinationProcessingDirectoryInfo = ObtainDirectory(Path.Combine(destinationAuthorDirectoryInfo.FullName, AppDataCatalog.PROCESSING));
                var destinationManualDirectoryInfo = ObtainDirectory(Path.Combine(destinationAuthorDirectoryInfo.FullName, AppDataCatalog.MANUAL));
                var destinationOnWallDirectoryInfo = ObtainDirectory(Path.Combine(destinationAuthorDirectoryInfo.FullName, AppDataCatalog.ONWALL));
                var destinationInAlbumDirectoryInfo = ObtainDirectory(Path.Combine(destinationAuthorDirectoryInfo.FullName, AppDataCatalog.INALBUM));

                foreach(var file in sourceDownloadedDirectoryInfo.GetFiles())
                {
                    var newFilePath = Path.Combine(destinationDownloadedDirectoryInfo.FullName, file.Name);
                    if (File.Exists(newFilePath))
                        File.Delete(newFilePath);

                    File.Move(file.FullName, newFilePath);
                }

                foreach (var file in sourceProcessingDirectoryInfo.GetFiles())
                {
                    var newFilePath = Path.Combine(destinationProcessingDirectoryInfo.FullName, file.Name);
                    if (File.Exists(newFilePath))
                        File.Delete(newFilePath);

                    File.Move(file.FullName, newFilePath);
                }

                foreach (var file in sourceManualDirectoryInfo.GetFiles())
                {
                    var newFilePath = Path.Combine(destinationManualDirectoryInfo.FullName, file.Name);
                    if (File.Exists(newFilePath))
                        File.Delete(newFilePath);

                    File.Move(file.FullName, newFilePath);
                }

                foreach (var file in sourceOnWallDirectoryInfo.GetFiles())
                {
                    var newFilePath = Path.Combine(destinationOnWallDirectoryInfo.FullName, file.Name);
                    if (File.Exists(newFilePath))
                        File.Delete(newFilePath);

                    File.Move(file.FullName, newFilePath);
                }

                foreach (var file in sourceInAlbumDirectoryInfo.GetFiles())
                {
                    var newFilePath = Path.Combine(destinationInAlbumDirectoryInfo.FullName, file.Name);
                    if (File.Exists(newFilePath))
                        File.Delete(newFilePath);

                    File.Move(file.FullName, newFilePath);
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public static void DeleteAuthorCatalog(string socialNetwork, string author)
        {
            try
            {
                var appDataDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.APPDATA));
                var socialNetworkDirectoryInfo = ObtainDirectory(Path.Combine(appDataDirectoryInfo.FullName, socialNetwork));
                var authorDirectoryInfo = ObtainDirectory(Path.Combine(socialNetworkDirectoryInfo.FullName, author));

                if(authorDirectoryInfo.Exists)
                    authorDirectoryInfo.Delete(true);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw e;
            }
        }


        //Publications files manipulations

        public static string MoveToProcessing(string source, string author, string filePath)
        {
            return MoveTo(source, author, filePath, AppDataCatalog.PROCESSING);
        }

        public static string MoveToOnWall(string source, string author, string filePath)
        {
            return MoveTo(source, author, filePath, AppDataCatalog.ONWALL);
        }

        public static string MoveToInAlbum(string source, string author, string filePath)
        {
            return MoveTo(source, author, filePath, AppDataCatalog.INALBUM);
        }

        public static string MoveToManual(string source, string author, string filePath)
        {
            return MoveTo(source, author, filePath, AppDataCatalog.MANUAL);
        }

        public static string MoveTo(string source, string author, string filePath, string catalogMoveTo)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var newFilePath = Path.Combine(RootPath, AppDataCatalog.APPDATA, source, author, catalogMoveTo, fileName);

                if (File.Exists(newFilePath))
                    File.Delete(newFilePath);

                File.Move(filePath, newFilePath);

                return newFilePath;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw e;
            }
        }

        public static void DeletePublicationFile(string filePath)
        {
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        //Telegram chat ids

        public static List<long> ReadTelegramBotChatIds()
        {
            try
            {
                var telegramDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.MEMORY, AppDataCatalog.TELEGRAM));
                var chatIdsFile = new FileInfo(Path.Combine(telegramDirectoryInfo.FullName, AppDataFile.TELEGRAMCHATIDS));
                if (!chatIdsFile.Exists)
                    return new List<long>();

                var chatIdsString = File.ReadAllText(chatIdsFile.FullName);
                if (String.IsNullOrWhiteSpace(chatIdsString))
                    return new List<long>();

                var chatIds = chatIdsString.Split(',')
                    .Select(x => long.Parse(x))
                    .ToList();

                return chatIds;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return new List<long>();
            }
        }

        public static void SaveTelegramChatId(long chatId)
        {
            try
            {
                var telegramDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.MEMORY, AppDataCatalog.TELEGRAM));
                var filePath = Path.Combine(telegramDirectoryInfo.FullName, AppDataFile.TELEGRAMCHATIDS);
                if (!File.Exists(filePath))
                    File.Create(filePath);

                var chatIdsString = File.ReadAllText(filePath);
                var chatIds = new List<long>();
                if (!String.IsNullOrWhiteSpace(chatIdsString))
                {
                    chatIds = chatIdsString.Split(',')
                    .Select(x => long.Parse(x))
                    .ToList();
                }

                if (!chatIds.Contains(chatId))
                    chatIds.Add(chatId);

                chatIdsString = String.Join(",", chatIds);
                File.WriteAllText(filePath, chatIdsString);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }


        //Backup

        public static void SerializePublicationManager()
        {
            var publicationManagerDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.MEMORY, AppDataCatalog.PUBLICATIONMANAGER));
            var filePath = Path.Combine(publicationManagerDirectoryInfo.FullName, AppDataFile.PUBLICATIONMANAGER);
            if (File.Exists(filePath))
                File.Delete(filePath);

            var startTime = PublicationManager.Instance.StartPublicationTime;
            var endTime = PublicationManager.Instance.EndPublicationTime;
            var now = DateTime.UtcNow;

            var publicationManagerDto = new PublicationManagerDto()
            {
                Publications = PublicationManager.Instance.Publications,
                StartPublicationTime = startTime.ToString(),
                EndPublicationTime = endTime.ToString(),
                PublicationIntervalMinutes = PublicationManager.Instance.PublicationIntervalMinutes,
                LastPublicationDateTime = PublicationManager.Instance.LastPublicationDateTime
            };

            var serializer = new JsonSerializer();

            using (var sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, publicationManagerDto);
            }
        }

        public static PublicationManagerDto? DeserializePublicationManager() 
        {
            var publicationManagerDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.MEMORY, AppDataCatalog.PUBLICATIONMANAGER));
            var filePath = Path.Combine(publicationManagerDirectoryInfo.FullName, AppDataFile.PUBLICATIONMANAGER);
            if (!File.Exists(filePath))
                return null;

            try
            {
                var serializer = new JsonSerializer();

                using (var sw = new StreamReader(filePath))
                using (var reader = new JsonTextReader(sw))
                {
                    return (PublicationManagerDto?)serializer.Deserialize(reader, typeof(PublicationManagerDto));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        public static void SerializeDeviantArtSchedulerTask()
        {
            var deviantArtSchedulerTaskDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.MEMORY, AppDataCatalog.DEVIANTARTSCHEDULER));
            var filePath = Path.Combine(deviantArtSchedulerTaskDirectoryInfo.FullName, AppDataFile.DEVIANTARTSCHEDULER);
            if (File.Exists(filePath))
                File.Delete(filePath);

            var deviantArtSchedulerTaskDto = new SchedulerTaskDeviantArtDto()
            {
                AuthorsLastArtsDates = SchedulerTaskDeviantArt.Instance.AuthorsLastArtsDates,
                Authors = SchedulerTaskDeviantArt.Instance.Authors,
                LastUpdateDateTime = SchedulerTaskDeviantArt.Instance.LastUpdateDateTime
            };

            var serializer = new JsonSerializer();

            using (var sw = new StreamWriter(filePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, deviantArtSchedulerTaskDto);
            }
        }

        public static SchedulerTaskDeviantArtDto? DeserializeDeviantArtSchedulerTask()
        {
            var deviantArtSchedulerTaskDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.MEMORY, AppDataCatalog.DEVIANTARTSCHEDULER));
            var filePath = Path.Combine(deviantArtSchedulerTaskDirectoryInfo.FullName, AppDataFile.DEVIANTARTSCHEDULER);
            if (!File.Exists(filePath))
                return null;

            try
            {
                var serializer = new JsonSerializer();

                using (var sw = new StreamReader(filePath))
                using (var reader = new JsonTextReader(sw))
                {
                    return (SchedulerTaskDeviantArtDto?)serializer.Deserialize(reader, typeof(SchedulerTaskDeviantArtDto));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }


        //Utitlity

        private static string NormilizeFileName(string fileName)
        {
            var result = fileName;

            foreach (var invalidSymbol in fileNameInvalidSymbols)
                result = result.Replace(invalidSymbol, String.Empty);

            return result;
        }

        public static bool SaveFile(string source, string author, Stream stream, string fileName, string fileExtension, string catalog, out string? filePath)
        {
            try
            {
                fileName = $"{NormilizeFileName(fileName)}{fileExtension}";
                var catalogDirectoryInfo = ObtainDirectory(Path.Combine(RootPath, AppDataCatalog.APPDATA, source, author, catalog));
                var path = Path.Combine(catalogDirectoryInfo.FullName, fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }

                stream.Close();

                filePath = path;
                return true;
            }
            catch (Exception e)
            {
                filePath = null;
                Logger.Error(e);

                return false;
            }

        }

        private static DirectoryInfo ObtainDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            return directoryInfo;
        }
    }
}
