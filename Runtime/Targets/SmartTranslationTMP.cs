using TMPro;

#if TRADUIO_TEXT_MESH_PRO

namespace Damdor.Traduio
{
    public class SmartTranslationTMP : BaseSmartTranslation
    {
        private TMP_Text target;
        
        protected override void Apply(string text)
        {
            if(target == null) target = GetComponent<TMP_Text>();
            if (target == null) return;
            target.text = text;
        }
    }
}

#endif