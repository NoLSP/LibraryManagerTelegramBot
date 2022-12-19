namespace SpecialLibraryBot.DeviantArtApi
{
    public interface IMatureContent<T> where T : class
    {
        T WithMatureContent(bool allowMature = true);
    }
}
