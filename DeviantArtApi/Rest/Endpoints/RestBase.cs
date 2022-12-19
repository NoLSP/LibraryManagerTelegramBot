namespace SpecialLibraryBot.DeviantArtApi
{
    public abstract class RestBase
    {
        protected string _uri;
        public string Uri => _uri;

        public RestBase()
        {
        }
    }
}
