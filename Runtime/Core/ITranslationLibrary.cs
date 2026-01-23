namespace Damdor.Traduio
{
    public interface ITranslationLibrary
    {
        bool HasTranslation(string key);
        string GetTranslation(string key);
    }
}