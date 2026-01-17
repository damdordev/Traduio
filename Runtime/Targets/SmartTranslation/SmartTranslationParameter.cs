using System;
using UnityEngine;

namespace Traduio
{
    /// <summary>
    /// Represents a named parameter for use in parameterized translations.
    /// </summary>
    /// <remarks>
    /// Translation parameters allow dynamic content to be inserted into translation strings.
    /// Each parameter consists of a key (the placeholder name in the translation string) and
    /// a value (which is itself a <see cref="SmartTranslationKey"/>, allowing for nested translations).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Translation string: "Welcome back, {playerName}! You have {itemCount} items."
    /// 
    /// 
    /// // Using with SmartTranslation
    /// translation.SetParameter("playerName", SmartTranslationKey.Translation("player.current.name"));
    /// translation.SetParameter("itemCount", SmartTranslationKey.Raw("42"));
    /// </code>
    /// </example>
    [Serializable]
    public struct SmartTranslationParameter
    {
        /// <summary>
        /// Gets or sets the parameter identifier used as a placeholder in translation strings.
        /// </summary>
        /// <value>
        /// The parameter name that will be replaced in the translation string (e.g., "playerName", "score", "itemCount").
        /// </value>
        /// <remarks>
        /// This key should match the placeholder name in the translation string, typically enclosed in curly braces.
        /// For example, if the translation is "Score: {playerScore}", the key should be "playerScore".
        /// </remarks>
        public string Key
        {
            get => key;
            set => key = value;
        }

        /// <summary>
        /// Gets or sets the translation key that provides the value for this parameter.
        /// </summary>
        /// <value>
        /// A <see cref="SmartTranslationKey"/> that will be resolved to provide the parameter's replacement value.
        /// </value>
        /// <remarks>
        /// The value can be either a raw text string or another translation key, enabling fully localized
        /// dynamic content where even parameter values are translated based on the current language.
        /// </remarks>
        public SmartTranslationKey Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeField] private string key;
        [SerializeField] private SmartTranslationKey value;
    }
}