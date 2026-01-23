using System;

namespace Damdor.Traduio
{
    public class TranslationLoaderException : Exception
    {
        protected TranslationLoaderException(string message) : base(message) {}
    }
    
    public class EmptyTranslationException : TranslationLoaderException
    {
        public EmptyTranslationException() : base("Translations are empty.") { }
    }
}