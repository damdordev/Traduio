using System;
using System.Threading;

namespace Traduio
{
    public interface ITranslationLoaderSettings
    {
        ITranslationLoaderSettings WithPriority(int priority);
        ITranslationLoaderSettings WithRetries(int retries);
        ITranslationLoaderSettings WithTimeout(TimeSpan timeout);
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
        public TimeSpan Timeout = TimeSpan.Zero;
            
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

        public ITranslationLoaderSettings WithTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public void Cancel()
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = null;
        }
        
    }
    
}