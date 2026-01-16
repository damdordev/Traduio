using System.Collections.Generic;

namespace Traduio
{
    public class TestSmartTranslation : SmartTranslation
    {
        public static TranslationManagerPointer Pointer = new();
        
        public List<string> Applies = new();

        protected override TranslationManagerPointer ManagerPointer => Pointer;
        
        protected override void Apply(string text)
        {
            Applies.Add(text);
        }
        
    }
}