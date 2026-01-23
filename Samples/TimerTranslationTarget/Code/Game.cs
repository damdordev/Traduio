using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Damdor.Traduio.Samples.TimerTranslationTarget
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private BaseTimerTranslationTarget time;
        [SerializeField] private Button resetButton1;
        [SerializeField] private Button resetButton2;
        
        private void Awake()
        {
            time.TimeToEnd = TimeSpan.FromMinutes(5);
            InitTranslations().Forget();
            resetButton1.onClick.AddListener(() => time.TimeToEnd = TimeSpan.FromMinutes(1));
            resetButton2.onClick.AddListener(() => time.TimeToEnd = TimeSpan.FromMinutes(10));
        }

        private static async UniTask InitTranslations()
        {
            var manager = new TranslationManager();
            manager.AddLoader(new ResourceTranslationLoader("Samples/TimerTranslationTarget/translations"));
            manager.Load();
            await manager.WaitForLoad();
            TraduioMainManager.Pointer.Setup(manager);
            manager.RefreshTargets();
        }

      
        
    }
}
