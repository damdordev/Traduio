using System;
using UnityEngine;

namespace Traduio
{
    [Serializable]
    public struct SmartTranslationParameter
    {
        [field: SerializeField] public string Key { get; set; }
        [field: SerializeField] public SmartTranslationKey Value { get; set; }
    }
}