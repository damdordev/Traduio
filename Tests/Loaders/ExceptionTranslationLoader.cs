using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Traduio.Tests
{
    public class ExceptionTranslationLoader : ITranslationLoader
    {
        private readonly float delay;
        private readonly Exception exception;

        public ExceptionTranslationLoader(float delay, Exception exception)
        {
            this.delay = delay;
            this.exception = exception;
        }

        public async UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken)
        {
            if(delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken:  cancellationToken);
            throw exception;
        }

        public string Id => "exception";
    }
}