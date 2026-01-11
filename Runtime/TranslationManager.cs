using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Traduio.Runtime
{
    public class TranslationManager
    {
        private class LoaderData
        {
            public ITranslationLoader Loader;
            public IDictionary<string, string> Translations;
            public bool LoadFinished;
            public CancellationTokenSource CancellationTokenSource;
        }

        private readonly List<LoaderData> loaders = new();
        
        public void AddLoader(ITranslationLoader loader)
        {
            loaders.Add(new LoaderData { Loader = loader });
        }

        public void RemoveLoader(ITranslationLoader loader)
        {
            
        }

        public void Load()
        {
            CancellationTokenSource cts = new();
            foreach (var data in loaders)
            {
                Load(data, cts.Token).Forget();
            }
        }

        public string GetTranslation(string key)
        {
            foreach (var data in loaders)
            {
                if (data.LoadFinished && data.Translations != null && data.Translations.TryGetValue(key, out var translation))
                {
                    return translation;
                }
            }

            return "";
        }

        public UniTask WaitForLoad(CancellationToken cancellationToken = default)
            => IsLoadFinished()
                ? UniTask.CompletedTask
                : UniTask.WaitUntil(IsLoadFinished, cancellationToken: cancellationToken);

        private async UniTask Load(LoaderData data, CancellationToken cancellationToken)
        {
            if (data.LoadFinished) return;
            try
            {
                data.Translations = await data.Loader.Load(cancellationToken);
                data.LoadFinished = true;
            }
            catch (Exception e)
            {
                data.LoadFinished = true;
                data.Translations = null;
            }
        }

        private bool IsLoadFinished()
        {
            foreach (var data in loaders)
            {
                if(!data.LoadFinished) return false;
            }
            return true;
        }
        
    }
}