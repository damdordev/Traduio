using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Traduio.Tests
{
    public class RetryTranslationLoader : ITranslationLoader
    {
        public int LoadCount { get; private set; }
        
        private Dictionary<string, string> translations;
        private float delay;
        private int retries;

        public RetryTranslationLoader(Dictionary<string, string> translations, float delay, int retries)
        {
            this.translations = translations;
            this.delay = delay;
            this.retries = retries;
        }

        public async UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken)
        {
            ++LoadCount;
            if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken : cancellationToken);
            if (retries > 0)
            {
                --retries;
                throw new ArgumentException();
            }

            return new DictionaryTranslationLibrary(translations);
        }

        public string Id => "retry";
    }
}