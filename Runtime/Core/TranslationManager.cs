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
        public event Action Refresh = () => { };

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
                loaders[i].Cancel();
                loaders.RemoveAt(i);
                --i;
            }
        }

        public void Load()
        {
            loaders = loaders
                .OrderBy(l => l.Priority)
                .ToList();
            foreach (var data in loaders) Load(data).Forget();
        }

        public void Dispose()
        {
            foreach (var t in loaders) t.Cancel();
            loaders.Clear();
        }

        public string GetTranslation(string key)
        {
            if (key == null) key = "";
            foreach (var data in loaders)
            {
                if (data.Translations != null && data.Translations.HasTranslation(key))
                {
                    return Process(key, data.Translations.GetTranslation(key), data.Loader);
                }
            }

            try { missingTranslationErrorHandler?.Invoke(key); }
            catch (Exception) { /* ignore */ }

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

        public string Translate(TranslationPointer pointer, List<TranslationParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(pointer.Value)) return "";
            var value = EvaluateTextPointer(pointer);
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if(string.IsNullOrWhiteSpace(parameter.Key)) continue;
                    value = value.Replace("{" + parameter.Key + "}", EvaluateTextPointer(parameter.Value));
                }
            }

            return value;

        }
        
        public string EvaluateTextPointer(TranslationPointer pointer)
        {
            if (string.IsNullOrWhiteSpace(pointer.Value)) return "";
            return pointer.Mode switch
            {
                TranslationPointerMode.RawText => pointer.Value,
                TranslationPointerMode.Translation => GetTranslation(pointer.Value),
                _ => throw new ArgumentOutOfRangeException($"Incorrect pointer mode: {pointer.Mode}")
            };
        }


        public async UniTask WaitForLoad(CancellationToken cancellationToken = default)
        {
            if (IsLoadFinished(loaders)) return;
            var currentLoaders = ListPool<LoaderData>.Get();
            foreach (var loaderData in loaders)
            {
                if(loaderData.CancellationTokenSource != null) currentLoaders.Add(loaderData);
            }

            await UniTask.WaitUntil(() => IsLoadFinished(currentLoaders), cancellationToken: cancellationToken);
            ListPool<LoaderData>.Release(currentLoaders);
        }

        public void SetLoaderErrorHandler(LoaderErrorHandler handler) 
            => loaderErrorHandler = handler;

        public void SetMissingTranslationErrorHandler(MissingTranslationErrorHandler handler) 
            => missingTranslationErrorHandler = handler;

        public void SetTranslationProcessor(TranslationProcessor processor) 
            => translationProcessor = processor;
        
        public void RefreshTargets() => Refresh.Invoke();

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
                var task = data.Loader.Load(data.CancellationTokenSource.Token);
                if(data.Timeout != TimeSpan.Zero) task = task.Timeout(data.Timeout);
                data.Translations = await task;
                if (data.Translations == null) throw new EmptyTranslationException();
            }
            catch (Exception e)
            {
                data.Translations = null;
                if (LogLoaderErrors) Debug.LogError($"Error during loading translation from [{data.Loader.Id}]: {e.Message}");
                
                try { loaderErrorHandler?.Invoke(data.Loader, e); }
                catch (Exception) { /* ignored */ }
            }

            data.CancellationTokenSource?.Dispose();
            data.CancellationTokenSource = null;
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
            try { translation = translationProcessor(key, translation, loader); }
            catch (Exception) { /* ignore */ }
            return translation;
        }
        
    }
}