using System;

namespace Damdor.Traduio
{
    public delegate void LoaderErrorHandler(ITranslationLoader loader, Exception exception);
    public delegate void MissingTranslationErrorHandler(string id);
    public delegate void RefreshTargetErrorHandler(TranslationTarget target, Exception exception);
    public delegate string TranslationProcessor(string id, string translation, ITranslationLoader loader);
}