using Damdor.Fundation;
using UnityEngine;

namespace Damdor.Traduio
{
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
        
        protected override void Apply()
        {
            if (target == null || key == null) return;
            target.SetParameter(key, SmartTranslationKey.Raw(Convert(Value)));
        }

        protected abstract string Convert(T value);
    }
}