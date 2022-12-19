using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot
{
    public enum PublicationState
    {
        Downloaded, //Скачано из источника
        Moderation, //Ждет обработки в телеграм боте
        Published, //Опубликовано в группе
        ManualProcessing //В каталоге ручной обработки
    }

    public class PublicationEntityCallBackDto
    {
        public string? PublicationId { get; set; }
        public string? Action { get; set; }
    }

    public class PublicationEntity
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string SocialNetwork { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public DateTime? PublicationDateTime { get; set; }
        public string ImageFilePath { get; set; }
        public PublicationState State { get; set; }

        public PublicationEntity() { }

        private PublicationEntity(string author, string socialNetwork, string source, string title, string imageFilePhysicalPath)
        {
            Id = DateTime.UtcNow.Ticks.ToString().Substring(5);
            Author = author;
            SocialNetwork = socialNetwork;
            Source = source;
            Title = title;
            ImageFilePath = imageFilePhysicalPath;
            State = PublicationState.Downloaded;
        }

        public static PublicationEntity DeviantArt( string author, string title, string imageFilePhysicalPath)
        {
            return new PublicationEntity(
                author,
                "DeviantArt",
                "www.deviantart.com",
                title,
                imageFilePhysicalPath
            );
        }
    }
}
