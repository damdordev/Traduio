using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Damdor.Traduio.Samples.NumberTranslationTarget
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private BaseNumberTranslationTarget number;
        [SerializeField] private Button lessButton;
        [SerializeField] private Button moreButton;
        
        private void Awake()
        {
            InitTranslations().Forget();
            lessButton.onClick.AddListener(() => --number.Value);
            moreButton.onClick.AddListener(() => ++number.Value);
        }

        private static async UniTask InitTranslations()
        {
            var manager = new TranslationManager();
            manager.AddLoader(new ResourceTranslationLoader("Samples/NumberTranslationTarget/translations"));
            manager.Load();
            await manager.WaitForLoad();
            TraduioMainManager.Pointer.Setup(manager);
            manager.RefreshTargets();
        }

      
        
    }
}
