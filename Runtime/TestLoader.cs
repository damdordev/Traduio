using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Traduio.Runtime
{
    public class TestLoader : ITranslationLoader
    {
        public async UniTask<IDictionary<string, string>> Load(CancellationToken cancellationToken =  default(CancellationToken))
        {
            // await UniTask.Delay(1000);
            Debug.Log("Throw exception");
            throw new System.NotImplementedException();
        }

        public string Id { get; }
    }
}