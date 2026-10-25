#if DAMDOR_TRADUIO_TYPIO

using System;
using System.Diagnostics.CodeAnalysis;
using Damdor.Typio;
using UnityEngine;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Damdor.Traduio
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class BaseTimerTranslationTarget : TranslationTarget
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
            if (target is null || key == null) return;
            lastTimeSpan = TimeToEnd;
            hasLastTimeSpan = true;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            target.SetParameter(key, SmartTranslationKey.Raw(Convert(lastTimeSpan)));
        }

        private void Update()
        {
            if (hasLastTimeSpan && Compare(TimeToEnd, lastTimeSpan)) return;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Refresh();
        }

        protected virtual DateTime GetNow() => DateTime.UtcNow;
        protected virtual string Convert(TimeSpan timeSpan) => ((int)timeSpan.TotalSeconds).ToString();
        protected virtual bool Compare(TimeSpan timeSpan1, TimeSpan timeSpan2) => (int) timeSpan1.TotalSeconds == (int) timeSpan2.TotalSeconds;
    }
}

#endif