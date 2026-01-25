using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Traduio
{
    public class DictionaryTranslationLoader : ITranslationLoader
    {
        private readonly IDictionary<string, string> translations;

        public DictionaryTranslationLoader(IDictionary<string, string> translations, string id = null)
        {
            this.translations = new Dictionary<string, string>(translations);
            Id = string.IsNullOrEmpty(id) ? "Dictionary" : $"Dictionary[{id}]";
        }

        public UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken)
            => UniTask.FromResult<ITranslationLibrary>(new DictionaryTranslationLibrary(translations));

        public string Id { get; }
    }
}