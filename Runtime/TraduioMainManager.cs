using UnityEngine;

namespace Traduio
{
    public static class TraduioMainManager
    {
        // [SerializeField] private TMP_te
        
        public static TranslationManagerPointer Pointer { get; } = new();
    }
}