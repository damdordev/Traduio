using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Traduio
{
    public class TranslationManager : IDisposable
    {
        public event Action Updated = () => { };

        public bool LogLoaderErrors { get; set; } = true;

        private List<LoaderData> loaders = new();
        private LoaderErrorHandler loaderErrorHandler;
        private MissingTranslationErrorHandler missingTranslationErrorHandler;
        private TranslationProcessor translationProcessor;
        
        public ITranslationLoaderSettings AddLoader(ITranslationLoader loader)
        {
            foreach (var loaderData in loaders)
            {
                if (loaderData.Loader == loader) return loaderData;
            }

            var data = new LoaderData { Loader = loader };
            loaders.Add(data);
            return data;
        }

        public void RemoveLoader(ITranslationLoader loader)
        {
            for (var i = 0; i < loaders.Count; i++)
            {
                if(loaders[i].Loader != loader) continue;
                loaders[i].IsRemoved = true;
                if(loaders[i].CancellationTokenSource != null) loaders[i].CancellationTokenSource.Cancel();
                loaders.RemoveAt(i);
                --i;
            }
        }

        public void Load()
        {
            loaders = loaders
                .OrderBy(l => l.Priority)
                .ToList();
            foreach (var data in loaders)
            {
                Load(data).Forget();
            }
        }

        public void Dispose()
        {
            foreach (var t in loaders)
            {
                t.CancellationTokenSource?.Cancel();
            }

            loaders.Clear();
        }

        public string GetTranslation(string key)
        {
            if (key == null) key = "";
            foreach (var data in loaders)
            {
                if (data.LoadFinished && data.Translations != null && data.Translations.HasTranslation(key))
                {
                    return Process(key, data.Translations.GetTranslation(key), data.Loader);
                }
            }

            try
            {
                missingTranslationErrorHandler?.Invoke(key);
            }
            catch (Exception e)
            {
                
            }

            return Process(key, "{missing:" + key + "}", null);
        }

        public bool HasTranslation(string key)
        {
            if (key == null) key = "";
            foreach (var data in loaders)
            {
                if (data.LoadFinished && data.Translations != null && data.Translations.HasTranslation(key)) return true;
            }

            return false;
        }

        public async UniTask WaitForLoad(CancellationToken cancellationToken = default)
        {
            if (IsLoadFinished(loaders)) return;
            var currentLoaders = ListPool<LoaderData>.Get();
            foreach (var loaderData in loaders)
            {
                if(!loaderData.LoadFinished && loaderData.CancellationTokenSource != null) currentLoaders.Add(loaderData);
            }

            await UniTask.WaitUntil(() => IsLoadFinished(currentLoaders), cancellationToken: cancellationToken);
            ListPool<LoaderData>.Release(currentLoaders);
        }

        public void SetLoaderErrorHandler(LoaderErrorHandler handler)
        {
            loaderErrorHandler = handler;
        }

        public void SetMissingTranslationErrorHandler(MissingTranslationErrorHandler handler)
        {
            missingTranslationErrorHandler = handler;
        }

        public void SetTranslationProcessor(TranslationProcessor processor)
        {
            translationProcessor = processor;
            Updated();
        }

        private async UniTask Load(LoaderData data)
        {
          while (!data.LoadFinished && !data.IsRemoved)
          {
              await TryLoad(data);
              if (data.Translations == null) --data.RetriesLeft;
              if (data.Translations != null || data.RetriesLeft < 0) data.LoadFinished = true;
          }
        }

        private async UniTask TryLoad(LoaderData data)
        {
            if (data.LoadFinished) return;
            data.CancellationTokenSource = new CancellationTokenSource();
            try
            {
                data.Translations = await data.Loader.Load(data.CancellationTokenSource.Token);
                if (data.Translations == null) throw new EmptyTranslationException();
            }
            catch (Exception e)
            {
                data.Translations = null;
                if (LogLoaderErrors)
                    Debug.LogError($"Error during loading translation from [{data.Loader.Id}]: {e.Message}");
                try
                {
                    loaderErrorHandler?.Invoke(data.Loader, e);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            data.CancellationTokenSource?.Dispose();
            data.CancellationTokenSource = null;
            if (data.Translations != null) Updated();
        }

        private static bool IsLoadFinished(List<LoaderData> loaders)
        {
            foreach (var data in loaders)
            {
                if(!data.IsRemoved && !data.LoadFinished) return false;
            }
            return true;
        }
        
        private string Process(string key, string translation, ITranslationLoader loader)
        {
            if (translationProcessor == null) return translation;
            try
            {
                translation = translationProcessor(key, translation, loader);
            }
            catch (Exception)
            {
                // ignore
            }
            return translation;
        }
        
    }
}