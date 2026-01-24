using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Traduio.Samples.LocalAndRemoteTranslations
{
    public class DelayedTranslationLoader : ITranslationLoader
    {
        private readonly TimeSpan delay;
        private readonly ResourceTranslationLoader innerLoader;

        public DelayedTranslationLoader(TimeSpan delay, ResourceTranslationLoader innerLoader)
        {
            this.delay = delay;
            this.innerLoader = innerLoader;
        }

        public async UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken)
        {
            await UniTask.Delay(delay, cancellationToken: cancellationToken);
            return await innerLoader.Load(cancellationToken);
        }

        public string Id => innerLoader.Id;
    }
}