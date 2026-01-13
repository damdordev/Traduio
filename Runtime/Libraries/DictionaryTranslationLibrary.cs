using System.Collections.Generic;

namespace Traduio
{
    public class DictionaryTranslationLibrary : ITranslationLibrary
    {
        private readonly IDictionary<string, string> translations;

        public DictionaryTranslationLibrary(IDictionary<string, string> translations)
        {
            this.translations = translations;
        }

        public bool HasTranslation(string key) => translations.ContainsKey(key);

        public string GetTranslation(string key)
        {
           translations.TryGetValue(key, out var translation);
            return translation;
        }
    }
}