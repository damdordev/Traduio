using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Traduio.Tests
{
    public class RetryTranslationLoader : ITranslationLoader
    {
        public int LoadCount { get; private set; }
        
        private readonly Dictionary<string, string> translations;
        private readonly float delay;
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
            if (retries <= 0) return new DictionaryTranslationLibrary(translations);
            --retries;
            throw new ArgumentException();

        }

        public string Id => "retry";
    }
}