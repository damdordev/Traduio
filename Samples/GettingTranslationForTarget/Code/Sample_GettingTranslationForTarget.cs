using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Traduio.Samples
{
    public class Sample_GettingTranslationFromCode : MonoBehaviour
    {
        private Dictionary<string, string> Translations = new Dictionary<string, string>()
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
            await translationManager.WaitForLoad();
        }
        
        private void OnLoaderLoader()
        {
            Debug.Log(Translations["hello"]);
        }

    }
}
