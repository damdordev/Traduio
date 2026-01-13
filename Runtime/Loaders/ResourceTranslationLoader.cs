using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Traduio
{
    public class ResourceTranslationLoader : ITranslationLoader
    {
        public string Id => $"Resource[{path}]";
        
        private readonly string path;

        public ResourceTranslationLoader(string path)
        {
            this.path = path;
        }
        
        public UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken)
        {
            var asset = Resources.Load<TextAsset>(path);
            var text = asset.text;
            Resources.UnloadAsset(asset);
            var result = JsonUtility.FromJson<Dictionary<string, string>>(text);
            return UniTask.FromResult<ITranslationLibrary>(new DictionaryTranslationLibrary(result));
        }
    }
}