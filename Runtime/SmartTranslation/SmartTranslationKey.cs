using System;
using UnityEngine;

namespace Traduio
{
    [Serializable]
    public struct SmartTranslationKey
    {
        [field: SerializeField] public SmartTranslationKeyMode Mode { get; set; }
        [field: SerializeField] public string Value { get; set; }

        public static SmartTranslationKey Raw(string text) => new()
        {
            Mode = SmartTranslationKeyMode.RawText,
            Value = text
        };

        public static SmartTranslationKey Translation(string id) => new()
        {
            Mode = SmartTranslationKeyMode.Translation,
            Value = id
        };
    }
}