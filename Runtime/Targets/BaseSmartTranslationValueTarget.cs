using System.Diagnostics.CodeAnalysis;
using Damdor.Fundation;
using UnityEngine;

namespace Damdor.Traduio
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class BaseSmartTranslationValueTarget<T> : TranslationTarget, ISetupable<T>
    {
        public abstract T Value { get; set; }

        public BaseSmartTranslation Target
        {
            get => target;
            set
            {
                target = value;
                Refresh();
            }
        }

        public string Key
        {
            get => key;
            set
            {
                key = value;
                Refresh();
            }
        }
        
        [SerializeField] private BaseSmartTranslation target;
        [SerializeField] private string key;
        
        public void Setup(T value)
        {
            Value = value;
        }
        
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeNullComparison")]
        protected override void Apply()
        {
            if (target == null || key == null) return;
            target.SetParameter(key, SmartTranslationKey.Raw(Convert(Value)));
        }

        protected abstract string Convert(T value);
    }
}