using System;

namespace Traduio
{
    public delegate void LoaderErrorHandler(ITranslationLoader loader, Exception exception);
    public delegate void MissingTranslationErrorHandler(string id);
    public delegate string TranslationProcessor(string id, string translation, ITranslationLoader loader);
}