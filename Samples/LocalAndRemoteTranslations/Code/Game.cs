using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Traduio.Samples
{
    public class Game : MonoBehaviour
    {
        public GameObject loadingScreen;
        public GameObject mainScreen;
        
        private WindowManager windowManager;
        
        public async UniTaskVoid Start()
        {
            windowManager = new WindowManager();
            await LoadLocalTranslations();
            windowManager.ChangeWindow(loadingScreen);
            await LoadRemoteTranslations();
            windowManager.ChangeWindow(mainScreen);
            OnFullyLoaded();
        }

        private async UniTask LoadLocalTranslations()
        {
            var manager = new TranslationManager();
            manager.AddLoader(new ResourceTranslationLoader("local_translations"));
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
                new ResourceTranslationLoader("remote_translations"))
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
