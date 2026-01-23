using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Damdor.Traduio.Samples.LocalAndRemoteTranslations
{
    public class Game : MonoBehaviour
    {
        public BaseSmartTranslation text;
        
        public async UniTaskVoid Start()
        {
            text.Key = SmartTranslationKey.Translation("loading");
            await LoadLocalTranslations();
            await LoadRemoteTranslations();
            text.Key = SmartTranslationKey.Translation("loaded");
            OnFullyLoaded();
        }

        private async UniTask LoadLocalTranslations()
        {
            var manager = new TranslationManager();
            manager.AddLoader(new ResourceTranslationLoader("Samples/LocalAndRemoteTranslations/local_translations"));
            manager.Load();
            await manager.WaitForLoad();
            TraduioMainManager.Pointer.Setup(manager);
            manager.RefreshTargets();
        }
        
        private async UniTask LoadRemoteTranslations()
        {
            var manager = new TranslationManager();
            manager.AddLoader(new DelayedTranslationLoader(
                TimeSpan.FromSeconds(10),
                new ResourceTranslationLoader("Samples/LocalAndRemoteTranslations/remote_translations"))
            );
            manager.Load();
            await manager.WaitForLoad();
            TraduioMainManager.Pointer.Setup(manager);
            manager.RefreshTargets();
        }
        
        private void OnFullyLoaded()
        {
            Debug.Log("Translations are fully loaded");
            Debug.Log("test: " + TraduioMainManager.Pointer.Manager.GetTranslation("test"));
        }
        
    }
}
