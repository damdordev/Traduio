using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Traduio
{
    public class ResourceTranslationLoader : ITranslationLoader
    {
        public string Id => $"Resource[{path}]";
        
        private readonly string path;
        private readonly bool async;

        public ResourceTranslationLoader(string path, bool async)
        {
            this.path = path;
            this.async = async;
        }
        
        public async UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken)
        {
            TextAsset asset;
            if (async) asset = (TextAsset) await Resources.LoadAsync<TextAsset>(path);
            else asset = Resources.Load<TextAsset>(path);
            var text = asset.text;
            Resources.UnloadAsset(asset);
            
            var result = (Json.Deserialize(text) as Dictionary<string, object>)!.ToDictionary(
                k => k.Key,
                kvp => kvp.Value.ToString()
            );

            return new DictionaryTranslationLibrary(result);
        }
    }
}