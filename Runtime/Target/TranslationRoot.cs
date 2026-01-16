using System;
using UnityEngine;

namespace Traduio
{
    public enum TranslationRootMode
    {
        RawText,
        Translation
    }
    
    [Serializable]
    public struct TranslationRoot
    {
        [field: SerializeField] public TranslationRootMode Mode { get; set; }
        [field: SerializeField] public string Value { get; set; }
    }

    [Serializable]
    public struct TranslationParameter
    {
        [field: SerializeField] public string Key { get; set; }
        [field: SerializeField] public TranslationRoot Value { get; set; }
    }
}