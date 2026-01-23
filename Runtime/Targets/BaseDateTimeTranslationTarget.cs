using System;
using Damdor.Foundation;
using UnityEngine;

namespace Damdor.Traduio
{
    public class BaseDateTimeTranslationTarget : BaseSmartTranslationValueTarget<DateTime>
    {
        public override DateTime Value
        {
            get => value;
            set
            {
                this.value = value;
                Refresh();
            }
        }

        [SerializeField] private SerializableDateTime value;
        
        protected override string Convert(DateTime value) => value.ToString("yyyy-MM-dd HH:mm:ss");
    }
}