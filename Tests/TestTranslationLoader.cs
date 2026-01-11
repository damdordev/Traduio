using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Traduio
{
    public class TestTranslationLoader : ITranslationLoader
    {
        public bool IsCancelled => cancellationToken.IsCancellationRequested;
        public int LoadCounter { get; private set; }
        
        private readonly IDictionary<string, string> translations;
        private readonly float delay;
        private CancellationToken cancellationToken;

        public TestTranslationLoader(IDictionary<string, string> translations, float delay)
        {
            this.translations = translations;
            this.delay = delay;
        }
        
        public async UniTask<IDictionary<string, string>> Load(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            ++LoadCounter;
            if(delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken : cancellationToken);
            return translations;
        }

        public string Id => "test";
    }
}