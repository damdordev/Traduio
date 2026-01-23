using System.Globalization;
using UnityEngine;

namespace Damdor.Traduio
{
    public class BaseNumberTranslationTarget : BaseSmartTranslationValueTarget<float>
    {
        public override float Value
        {
            get => value;
            set
            {
                this.value = value;
                Refresh();
            }
        }

        [SerializeField] private float value;
        
        protected override string Convert(float x) => x.ToString(CultureInfo.InvariantCulture);
    }
}