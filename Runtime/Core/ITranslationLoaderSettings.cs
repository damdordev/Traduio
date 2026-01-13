namespace Traduio
{
    public interface ITranslationLoaderSettings
    {
        ITranslationLoaderSettings WithPriority(int priority);
        ITranslationLoaderSettings WithRetries(int retries);
    }
}