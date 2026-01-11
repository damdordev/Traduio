using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Traduio
{
    public class NullTranslationLoader : ITranslationLoader
    {
        private readonly float delay;

        public NullTranslationLoader(float delay)
        {
            this.delay = delay;
        }
        
        public async UniTask<IDictionary<string, string>> Load(CancellationToken cancellationToken)
        {
            if(delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken:  cancellationToken);
            return null;
        }

        public string Id => "null";
    }
}