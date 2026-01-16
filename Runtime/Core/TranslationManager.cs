using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Traduio
{
    /// <summary>
    /// Manages translation loading, retrieval, and processing for localization.
    /// Supports multiple translation loaders with priority-based ordering and automatic retries.
    /// All methods in TranslationManager need to be invoked from unity main thread.
    /// </summary>
    /// <remarks>
    /// The TranslationManager handles:
    /// - Multiple translation sources (loaders) with configurable priorities
    /// - Asynchronous loading with timeout and retry support
    /// - Translation key lookup with fallback behavior
    /// - Custom error handling for missing translations and loader failures
    /// - Translation text processing through custom processors
    /// </remarks>
    /// <example>
    /// Basic usage:
    /// <code>
    /// var manager = new TranslationManager();
    /// 
    /// // Add a JSON file loader
    /// var jsonLoader = ResourceTranslationLoader("translations");
    /// manager.AddLoader(jsonLoader)
    ///     .WithPriority(10)
    ///     .WithRetries(3)
    ///     .WithTimeout(TimeSpan.FromSeconds(5));
    /// 
    /// // Load translations asynchronously
    /// manager.Load();
    /// await manager.WaitForLoad();
    /// 
    /// // Get translations
    /// string greeting = manager.GetTranslation("ui.greeting");
    /// // Returns: "Hello, World!" (or "{missing:ui.greeting}" if not found)
    /// 
    /// // Clean up when done
    /// manager.Dispose();
    /// </code>
    /// 
    /// </example>
    public class TranslationManager : IDisposable
    {
        /// <summary>
        /// Event raised when translation targets should refresh their displayed text.
        /// Subscribe to this event to update UI elements when translations are reloaded.
        /// Event is used mostly for supporting translation targets.
        /// </summary>
        /// <example>
        /// <code>
        /// // Subscribe to refresh events
        /// manager.Refresh += OnTranslationsRefreshed;
        /// 
        /// void OnTranslationsRefreshed()
        /// {
        ///     // Update all UI text elements
        ///     titleText.text = manager.GetTranslation("ui.title");
        ///     buttonText.text = manager.GetTranslation("ui.submit");
        /// }
        /// 
        /// </code>
        /// </example>
        public event Action Refresh = () => { };

        /// <summary>
        /// Gets or sets whether loader errors should be logged to Unity's Debug console.
        /// Default is true.
        /// </summary>
        public bool LogLoaderErrors { get; set; } = true;

        private List<LoaderData> loaders = new();
        private LoaderErrorHandler loaderErrorHandler;
        private MissingTranslationErrorHandler missingTranslationErrorHandler;
        private TranslationProcessor translationProcessor;
        
        /// <summary>
        /// Adds a translation loader to the manager with configurable settings.
        /// </summary>
        /// <param name="loader">The translation loader to add. Must not be null.</param>
        /// <returns>Settings interface to configure priority, timeout, and retry behavior for this loader.</returns>
        /// <remarks>
        /// If the same loader instance is added multiple times, returns the existing configuration.
        /// Loaders are automatically sorted by priority after addition.
        /// </remarks>
        /// <example>
        /// <code>
        /// manager.AddLoader(new MyTranslationLoader())
        ///     .WithPriority(10)              // Lower priority = loads first
        ///     .WithRetries(3)                // Retry 3 times on failure
        ///     .WithTimeout(TimeSpan.FromSeconds(10));
        /// </code>
        /// </example>
        public ITranslationLoaderSettings AddLoader(ITranslationLoader loader)
        {
            foreach (var loaderData in loaders)
            {
                if (loaderData.Loader == loader) return loaderData;
            }

            var data = new LoaderData { Loader = loader };
            loaders.Add(data);
            SortLoaders();
            return data;
        }

        /// <summary>
        /// Removes a translation loader from the manager and cancels any ongoing load operations.
        /// </summary>
        /// <param name="loader">The loader to remove.</param>
        /// <remarks>
        /// This method cancels any in-progress loading tasks for the specified loader
        /// and removes all instances of it from the manager.
        /// </remarks>
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

        /// <summary>
        /// Initiates asynchronous loading of translations from all registered loaders.
        /// </summary>
        /// <remarks>
        /// Each loader runs independently and retries according to its configured settings.
        /// This method returns immediately; use WaitForLoad() to await completion.
        /// </remarks>
        public void Load()
        {
            loaders = loaders
                .OrderBy(l => l.Priority)
                .ToList();
            foreach (var data in loaders) Load(data).Forget();
        }
        
        /// <summary>
        /// Releases all resources, cancels ongoing operations, and clears all loaders.
        /// </summary>
        public void Dispose()
        {
            foreach (var t in loaders) t.Cancel();
            loaders.Clear();
        }

        /// <summary>
        /// Retrieves a translated string for the specified key.
        /// </summary>
        /// <param name="key">The translation key to lookup. Null is treated as empty string.</param>
        /// <returns>
        /// The translated string if found, or "{missing:key}" format if not found.
        /// The result is processed through any registered TranslationProcessor.
        /// </returns>
        /// <remarks>
        /// Searches loaders in priority order and returns the first match found.
        /// Invokes the missing translation error handler if no translation is found.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Basic usage
        /// string welcomeMsg = manager.GetTranslation("ui.welcome");
        /// // Returns: "Welcome!" or "{missing:ui.welcome}"
        /// </code>
        /// </example>
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

        /// <summary>
        /// Checks if a translation exists for the specified key.
        /// </summary>
        /// <param name="key">The translation key to check. Null is treated as empty string.</param>
        /// <returns>True if any loaded translation source contains the key; otherwise false.</returns>
        /// <remarks>
        /// Only checks loaders that have finished loading successfully.
        /// Does not trigger missing translation error handlers.
        /// </remarks>
        public bool HasTranslation(string key)
        {
            if (key == null) key = "";
            foreach (var data in loaders)
            {
                if (data.LoadFinished && data.Translations != null && data.Translations.HasTranslation(key)) return true;
            }

            return false;
        }

        /// <summary>
        /// Translates a TranslationRoot with optional parameter substitution.
        /// </summary>
        /// <param name="root">The root translation object containing the text or translation key.</param>
        /// <param name="parameters">Optional list of parameters for placeholder replacement in format {key}.</param>
        /// <returns>
        /// The evaluated and processed translation string with all parameters substituted.
        /// Returns empty string if root value is null or whitespace.
        /// </returns>
        /// <remarks>
        /// Parameters are replaced in the format {parameterKey} with their evaluated values.
        /// Parameter values can themselves be translation keys or raw text based on their mode.
        /// </remarks>
        /// <example>
        /// Basic translation with parameters:
        /// <code>
        /// // Translation file contains: "welcome_user": "Welcome, {username}!"
        /// 
        /// var root = new TranslationRoot
        /// {
        ///     Mode = TranslationRootMode.Translation,
        ///     Value = "welcome_user"
        /// };
        /// 
        /// var parameters = new List&lt;TranslationParameter&gt;
        /// {
        ///     new TranslationParameter
        ///     {
        ///         Key = "username",
        ///         Value = new TranslationRoot
        ///         {
        ///             Mode = TranslationRootMode.RawText,
        ///             Value = "Alice"
        ///         }
        ///     }
        /// };
        /// 
        /// string result = manager.Translate(root, parameters);
        /// // Returns: "Welcome, Alice!"
        /// </code>
        /// 
        /// Using raw text mode:
        /// <code>
        /// var root = new TranslationRoot
        /// {
        ///     Mode = TranslationRootMode.RawText,
        ///     Value = "This is raw text, not a translation key"
        /// };
        /// 
        /// string result = manager.Translate(root);
        /// // Returns: "This is raw text, not a translation key"
        /// </code>
        /// 
        /// Nested translations as parameters:
        /// <code>
        /// // Translation file contains:
        /// // "greeting": "Hello, {name}!"
        /// // "user_role": "Administrator"
        /// 
        /// var parameters = new List&lt;TranslationParameter&gt;
        /// {
        ///     new TranslationParameter
        ///     {
        ///         Key = "name",
        ///         Value = new TranslationRoot
        ///         {
        ///             Mode = TranslationRootMode.Translation,
        ///             Value = "user_role"  // This will be translated too
        ///         }
        ///     }
        /// };
        /// 
        /// string result = manager.Translate(
        ///     new TranslationRoot { Mode = TranslationRootMode.Translation, Value = "greeting" },
        ///     parameters
        /// );
        /// // Returns: "Hello, Administrator!"
        /// </code>
        /// </example>
        public string Translate(TranslationRoot root, List<TranslationParameter> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(root.Value)) return "";
            var value = Evaluate(root);
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    if(string.IsNullOrWhiteSpace(parameter.Key)) continue;
                    value = value.Replace("{" + parameter.Key + "}", Evaluate(parameter.Value));
                }
            }

            return value;

        }
        
        /// <summary>
        /// Asynchronously waits for all current loaders to finish loading.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the wait operation.</param>
        /// <returns>A UniTask that completes when all loaders have finished loading or been cancelled.</returns>
        /// <remarks>
        /// Only waits for loaders that were active when this method was called.
        /// Loaders added during the wait are not included.
        /// </remarks>
        /// <example>
        /// Basic usage:
        /// <code>
        /// var manager = new TranslationManager();
        /// manager.AddLoader(MyLoader());
        /// 
        /// manager.Load();
        /// await manager.WaitForLoad(); // Waits until loading completes
        /// 
        /// // Now safe to use translations
        /// string text = manager.GetTranslation("ui.title");
        /// </code>
        /// </example>
        public async UniTask WaitForLoad(CancellationToken cancellationToken = default)
        {
            if (IsLoadFinished(loaders)) return;
            var currentLoaders = ListPool<LoaderData>.Get();
            foreach (var loaderData in loaders)
            {
                if(loaderData.CancellationTokenSource != null) currentLoaders.Add(loaderData);
            }

            try
            {
                await UniTask.WaitUntil(() => IsLoadFinished(currentLoaders), cancellationToken: cancellationToken);
            }
            finally
            {
                ListPool<LoaderData>.Release(currentLoaders);
            }
        }

        /// <summary>
        /// Sets a custom error handler for loader failures.
        /// </summary>
        /// <param name="handler">Delegate called when a loader fails. Receives the loader and exception.</param>
        /// <remarks>
        /// The handler is invoked for each failed load attempt, including retries.
        /// Exceptions thrown by the handler are silently ignored.
        /// </remarks>
        /// <example>
        /// <code>
        /// manager.SetLoaderErrorHandler((loader, exception) =>
        /// {
        ///     Debug.LogError($"Failed to load translations from {loader.Id}: {exception.Message}");
        ///     
        ///     // Log to analytics service
        ///     Analytics.LogError("TranslationLoadFailed", new Dictionary&lt;string, object&gt;
        ///     {
        ///         { "loaderId", loader.Id },
        ///         { "error", exception.Message }
        ///     });
        /// });
        /// </code>
        /// </example>
        public void SetLoaderErrorHandler(LoaderErrorHandler handler) 
            => loaderErrorHandler = handler;

        /// <summary>
        /// Sets a custom error handler for missing translation keys.
        /// </summary>
        /// <param name="handler">Delegate called when GetTranslation() doesn't find a key.</param>
        /// <remarks>
        /// Exceptions thrown by the handler are silently ignored.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Log missing translations for debugging
        /// manager.SetMissingTranslationErrorHandler(key =>
        /// {
        ///     Debug.LogWarning($"Missing translation for key: {key}");
        /// });
        /// </code>
        /// </example>
        public void SetMissingTranslationErrorHandler(MissingTranslationErrorHandler handler) 
            => missingTranslationErrorHandler = handler;

        /// <summary>
        /// Sets a custom processor to transform translations before they are returned.
        /// </summary>
        /// <param name="processor">
        /// Delegate that receives the key, translation, and loader, and returns a processed string.
        /// </param>
        /// <remarks>
        /// The processor is applied to all translations retrieved via GetTranslation().
        /// Exceptions thrown by the processor are silently ignored and the unprocessed translation is returned.
        /// </remarks>
        /// <example>
        /// Replace placeholders with runtime values:
        /// <code>
        /// manager.SetTranslationProcessor((key, translation, loader) =>
        /// {
        ///     // Replace {{PLAYER_NAME}} with actual player name
        ///     translation = translation.Replace("{{PLAYER_NAME}}", PlayerData.CurrentPlayerName);
        ///     
        ///     // Replace {{GAME_VERSION}} with version
        ///     translation = translation.Replace("{{GAME_VERSION}}", Application.version);
        ///     
        ///     return translation;
        /// });
        /// 
        /// </code>
        /// </example>       
        public void SetTranslationProcessor(TranslationProcessor processor) 
            => translationProcessor = processor;
        
        /// <summary>
        /// Triggers the Refresh event to notify all subscribers to update their translations.
        /// </summary>
        /// <remarks>
        /// Call this method after reloading translations to update UI elements.
        /// </remarks>
        public void RefreshTargets() => Refresh.Invoke();

        private string Evaluate(TranslationRoot root)
        {
            if (string.IsNullOrWhiteSpace(root.Value)) return "";
            return root.Mode switch
            {
                TranslationRootMode.RawText => root.Value,
                TranslationRootMode.Translation => GetTranslation(root.Value),
                _ => throw new ArgumentOutOfRangeException($"Incorrect pointer mode: {root.Mode}")
            };
        }
        
        private void SortLoaders()
        {
            var tmpLoaders = ListPool<LoaderData>.Get();
            tmpLoaders.AddRange(loaders.OrderBy(l => l.Priority));
            loaders.Clear();
            loaders.AddRange(tmpLoaders);
            ListPool<LoaderData>.Release(tmpLoaders);
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
            await UniTask.SwitchToMainThread();
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