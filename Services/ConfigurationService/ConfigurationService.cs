using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Services
{
    public class ConfigurationService
    {
        //Instance
        
        private static ConfigurationService? instance;
        public static ConfigurationService Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConfigurationService();

                return instance;
            }
        }

        private readonly IConfiguration configuration;

        public ConfigurationService()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        //TelegramBot

        private static TelegramBotConfiguration? telegramBotConfiguration;
        public static TelegramBotConfiguration TelegramBotConfiguration
        {
            get
            {
                if (telegramBotConfiguration == null)
                    telegramBotConfiguration = Instance.configuration
                        .GetSection(nameof(TelegramBotConfiguration))
                        .Get<TelegramBotConfiguration>();

                if (telegramBotConfiguration == null)
                    throw new Exception("Cannot read TelegramBotConfiguration.");

                return telegramBotConfiguration;
            }
        }

        //DeviantArt

        private static DeviantArtConfiguration? deviantArtConfiguration;
        public static DeviantArtConfiguration DeviantArtConfiguration
        {
            get
            {
                if (deviantArtConfiguration == null)
                    deviantArtConfiguration = Instance.configuration
                        .GetSection(nameof(DeviantArtConfiguration))
                        .Get<DeviantArtConfiguration>();

                if (deviantArtConfiguration == null)
                    throw new Exception("Cannot read DeviantArtConfiguration.");

                return deviantArtConfiguration;
            }
        }

        //PublicationManager

        private static PublicationManagerConfiguration? publicationManagerConfiguration;
        public static PublicationManagerConfiguration PublicationManagerConfiguration
        {
            get
            {
                if (publicationManagerConfiguration == null)
                    publicationManagerConfiguration = Instance.configuration
                        .GetSection(nameof(PublicationManagerConfiguration))
                        .Get<PublicationManagerConfiguration>();

                if (publicationManagerConfiguration == null)
                    throw new Exception("Cannot read PublicationManagerConfiguration.");

                return publicationManagerConfiguration;
            }
        }

        //VK

        private static VKConfiguration? vkConfiguration;
        public static VKConfiguration VKConfiguration
        {
            get
            {
                if (vkConfiguration == null)
                    vkConfiguration = Instance.configuration
                        .GetSection(nameof(VKConfiguration))
                        .Get<VKConfiguration>();

                if (vkConfiguration == null)
                    throw new Exception("Cannot read VKConfiguration.");

                return vkConfiguration;
            }
        }

        //NLog

        private static IConfigurationSection? nlogConfiguration;
        public static IConfigurationSection NlogConfiguration
        {
            get
            {
                if (nlogConfiguration == null)
                    nlogConfiguration = Instance.configuration
                        .GetSection("NLog");

                if (nlogConfiguration == null)
                    throw new Exception("Cannot read NLogConfiguration.");

                return nlogConfiguration;
            }
        }
    }
}
