using System.Threading;

namespace Traduio
{
    public interface ITranslationLoaderSettings
    {
        ITranslationLoaderSettings WithPriority(int priority);
        ITranslationLoaderSettings WithRetries(int retries);
    }
    
    internal class LoaderData : ITranslationLoaderSettings
    {
        public ITranslationLoader Loader;
        public ITranslationLibrary Translations;
        public bool LoadFinished;
        public CancellationTokenSource CancellationTokenSource;
        public bool IsRemoved;
        public int Priority;
        public int RetriesLeft;
            
        public ITranslationLoaderSettings WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        public ITranslationLoaderSettings WithRetries(int retries)
        {
            RetriesLeft = retries;
            return this;
        }
    }
    
}