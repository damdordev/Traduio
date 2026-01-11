using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Traduio.Runtime;

namespace Traduio
{
    public class ExceptionTranslationLoader : ITranslationLoader
    {
        private readonly float delay;
        private readonly Exception exception;

        public ExceptionTranslationLoader(float delay, Exception exception, string id = "exception")
        {
            this.delay = delay;
            this.exception = exception;
            Id = id;
        }

        public async UniTask<IDictionary<string, string>> Load(CancellationToken cancellationToken)
        {
            if(delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken:  cancellationToken);
            throw exception;
        }

        public string Id { get; }
    }
}