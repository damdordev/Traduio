using System;
using Cysharp.Threading.Tasks;
using Traduio.Runtime;

namespace Traduio
{
    public delegate void ErrorHandler(string error, Exception exception);
    
    public interface ITranslationManager
    {
        void AddLoader(ITranslationLoader loader);
        void RemoveLoader(ITranslationLoader loader);
        void Load();
        UniTask WaitForLoad();
        string GetTranslation(string key);
        void SetErrorHandler(ErrorHandler errorHandler);
    }
}