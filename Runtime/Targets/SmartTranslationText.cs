#if TRADUIO_UGUI

using System.Diagnostics.CodeAnalysis;
using UnityEngine.UI;

namespace Damdor.Traduio
{
    public class SmartTranslationText : BaseSmartTranslation
    {
        private Text target;
        
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeNullComparison")]
        protected override void Apply(string text)
        {
            if(target == null) target = GetComponent<Text>();
            if (target == null) return;
            target.text = text;
        }
    }
}

#endif