using System.Diagnostics.CodeAnalysis;
using TMPro;

namespace Damdor.Traduio
{
    public class SmartTranslationTMP : BaseSmartTranslation
    {
        private TMP_Text target;
        
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeNullComparison")]
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        protected override void Apply(string text)
        {
            if(target == null) target = GetComponent<TMP_Text>();
            if (target == null) return;
            target.text = text;
        }
    }
}
