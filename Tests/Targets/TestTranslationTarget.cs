namespace Damdor.Traduio.Tests
{
    public class TestTranslationTarget : TranslationTarget
    {
        public int Applies;
        public static TranslationManagerPointer Pointer = new();
        
        protected override TranslationManagerPointer ManagerPointer => Pointer;

        protected override void Apply()
        {
            ++Applies;
        }
        
        public new void Refresh()
        {
            base.Refresh();
        }
    }
}