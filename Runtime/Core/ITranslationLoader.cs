using System.Threading;
using Cysharp.Threading.Tasks;

namespace Damdor.Traduio
{
    public interface ITranslationLoader
    {
        UniTask<ITranslationLibrary> Load(CancellationToken cancellationToken);
        string Id { get; }
    }
}