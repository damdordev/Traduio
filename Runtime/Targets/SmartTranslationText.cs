#if TRADUIO_UGUI

using UnityEngine.UI;

namespace Damdor.Traduio
{
    public class SmartTranslationText : BaseSmartTranslation
    {
        private Text target;
        
        protected override void Apply(string text)
        {
            if(target == null) target = GetComponent<Text>();
            if (target == null) return;
            target.text = text;
        }
    }
}

#endif