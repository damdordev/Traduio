using System.Globalization;
using Damdor.Fundation;
using UnityEngine;

namespace Damdor.Traduio
{
    public class BaseNumberTranslationTarget : TranslationTarget, ISetupable<float>
    {
        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                Refresh();
            }
        }

        public SmartTranslationText Target
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
        
        [SerializeField] private float value;
        [SerializeField] private SmartTranslationText target;
        [SerializeField] private string key;
        
        public void Setup(float value)
        {
            Value = value;
        }

        protected override void Apply()
        {
            if (target == null || key == null) return;
            target.SetParameter(key, SmartTranslationKey.Raw(Convert(Value)));
        }

        protected virtual string Convert(float x) => x.ToString(CultureInfo.InvariantCulture);

    }
}