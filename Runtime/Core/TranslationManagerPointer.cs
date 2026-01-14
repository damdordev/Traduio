using System;

namespace Traduio
{
    public class TranslationManagerPointer
    {
        public event Action Changed = delegate { }; 
        
        public TranslationManager Manager { get; private set; }
        
        public void Setup(TranslationManager manager)
        {
            if (Manager == manager) return;
            Manager = manager;
            Changed();
        }
        
    }
}