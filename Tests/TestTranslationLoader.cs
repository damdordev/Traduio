using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Traduio.Runtime;

namespace Traduio
{
    public class TestTranslationLoader : ITranslationLoader
    {
        private readonly IDictionary<string, string> translations;
        private readonly float delay; 

        public TestTranslationLoader(IDictionary<string, string> translations, float delay, string id = "test")
        {
            this.translations = translations;
            this.delay = delay;
            Id = id;
        }
        
        public async UniTask<IDictionary<string, string>> Load(CancellationToken cancellationToken)
        {
            if(delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken : cancellationToken);
            return translations;
        }

        public string Id { get; }
    }
}