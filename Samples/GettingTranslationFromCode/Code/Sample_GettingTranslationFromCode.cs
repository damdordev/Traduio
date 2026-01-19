using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Traduio.Samples
{
    public class Sample_GettingTranslationFromCode : MonoBehaviour
    {
        private Dictionary<string, string> Translations = new()
        {
            { "hello", "This is a simple sample of using Traduio" },
        };
        
        private TranslationManager translationManager;

        private void Awake()
        {
            LoadManager()
                .ContinueWith(OnLoaderLoader)
                .Forget();
        }

        private async UniTask LoadManager()
        {
            translationManager = new TranslationManager();
            translationManager.AddLoader(new DictionaryTranslationLoader(Translations));
            translationManager.Load();
            TraduioMainManager.Pointer.Setup(translationManager);
            await translationManager.WaitForLoad();
            translationManager.RefreshTargets();
        }
        
        private void OnLoaderLoader()
        {
            Debug.Log(Translations["hello"]);
        }

    }
}
