namespace Traduio
{
    public static class Traduio
    {
        public static TranslationManager MainManager { get; private set; }
        
        public static void SetMainManager(TranslationManager manager)
        {
            MainManager = manager;
        }
    }
}