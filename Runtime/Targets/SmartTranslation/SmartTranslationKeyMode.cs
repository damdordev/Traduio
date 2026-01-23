namespace Damdor.Traduio
{
    /// <summary>
    /// Defines how a <see cref="SmartTranslationKey"/> should interpret its value.
    /// </summary>
    public enum SmartTranslationKeyMode
    {
        /// <summary>
        /// The value is treated as literal text and displayed as-is without translation.
        /// </summary>
        RawText,
        
        /// <summary>
        /// The value is treated as a translation key identifier that will be looked up in the translation system.
        /// </summary>
        Translation
    }
}