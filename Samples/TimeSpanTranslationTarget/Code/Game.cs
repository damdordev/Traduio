using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Damdor.Traduio.Samples.TimeSpanTranslationTarget
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private BaseTimeSpanTranslationTarget time;
        [SerializeField] private Button lessButton;
        [SerializeField] private Button moreButton;
        
        private void Awake()
        {
            InitTranslations().Forget();
            lessButton.onClick.AddListener(() => time.Value = time.Value.Add(-TimeSpan.FromMinutes(1)));
            moreButton.onClick.AddListener(() => time.Value = time.Value.Add(TimeSpan.FromMinutes(1)));
        }

        private static async UniTask InitTranslations()
        {
            var manager = new TranslationManager();
            manager.AddLoader(new ResourceTranslationLoader("Samples/TimeSpanTranslationTarget/translations"));
            manager.Load();
            await manager.WaitForLoad();
            TraduioMainManager.Pointer.Setup(manager);
            manager.RefreshTargets();
        }

      
        
    }
}
