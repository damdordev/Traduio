using System.Collections.Generic;

namespace Damdor.Traduio.Tests
{
    public class TestBaseSmartTranslation : BaseSmartTranslation
    {
        public static readonly TranslationManagerPointer Pointer = new();
        
        public List<string> Applies = new();

        protected override TranslationManagerPointer ManagerPointer => Pointer;
        
        protected override void Apply(string text)
        {
            Applies.Add(text);
        }
        
    }
}