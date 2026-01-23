using System;
using Damdor.Foundation;
using UnityEngine;

namespace Damdor.Traduio
{
    public class BaseTimeSpanTranslationTarget : BaseSmartTranslationValueTarget<TimeSpan>
    {
        public override TimeSpan Value
        {
            get => value;
            set
            {
                this.value = value;
                Refresh();
            }
        }

        [SerializeField] private SerializableTimeSpan value;
        
        protected override string Convert(TimeSpan value) => value.ToString("g");
    }
}