using System;
using UnityEngine;

namespace Traduio
{
    public enum TranslationPointerMode
    {
        RawText,
        Translation
    }
    
    [Serializable]
    public class TranslationPointer
    {
        [field: SerializeField] public TranslationPointerMode Mode { get; set; }
        [field: SerializeField] public string Value { get; set; }
    }

    [Serializable]
    public class TranslationParameter
    {
        [field: SerializeField] public string Key { get; set; }
        [field: SerializeField] public TranslationPointer Value { get; set; }
    }
}