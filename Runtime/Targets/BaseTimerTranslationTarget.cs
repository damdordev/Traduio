using System;
using Damdor.Foundation;
using Damdor.Fundation;
using UnityEngine;

namespace Damdor.Traduio
{
    public class BaseTimerTranslationTarget : TranslationTarget, ISetupable<DateTime>, ISetupable<TimeSpan>
    {
        public DateTime EndTime
        {
            get => endTime;
            set
            {
                endTime = value;
                Refresh();
            }
        }
        
        public TimeSpan TimeToEnd
        {
            get => endTime.DateTime.Subtract(GetNow());
            set
            {
                endTime = GetNow().Add(value);
                Refresh();
            }
        }

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

        public void Setup(DateTime model) => EndTime = model;
        public void Setup(TimeSpan model) => TimeToEnd = model;

        [SerializeField] private BaseSmartTranslation target;
        [SerializeField] private string key;
        [SerializeField] private SerializableDateTime endTime;

        private bool hasLastTimeSpan;
        private TimeSpan lastTimeSpan;
        
        protected override void Apply()
        {
            if (target == null || key == null) return;
            lastTimeSpan = TimeToEnd;
            target.SetParameter(key, SmartTranslationKey.Raw(Convert(lastTimeSpan)));
        }

        private void Update()
        {
            if (hasLastTimeSpan && Compare(TimeToEnd, lastTimeSpan)) return;
            Refresh();
        }

        protected virtual DateTime GetNow() => DateTime.UtcNow;
        protected virtual string Convert(TimeSpan timeSpan) => ((int)timeSpan.TotalSeconds).ToString();
        protected virtual bool Compare(TimeSpan timeSpan1, TimeSpan timeSpan2) => (int) timeSpan1.TotalSeconds == (int) timeSpan2.TotalSeconds;
    }
}