using System.Threading;
using Cysharp.Threading.Tasks;

namespace Traduio
{
    public interface ITranslationLoader
    {
        UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken);
        string Id { get; }
    }
}