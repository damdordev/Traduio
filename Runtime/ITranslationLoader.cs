using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Traduio.Runtime
{
    public interface ITranslationLoader
    {
        UniTask<IDictionary<string, string>> Load(CancellationToken cancellationToken);
        string Id { get; }
    }
}