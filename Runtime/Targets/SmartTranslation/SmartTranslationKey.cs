using System;
using UnityEngine;

namespace Traduio
{
    /// <summary>
    /// Represents a translation key that can be either a raw text string or a translation identifier.
    /// </summary>
    /// <remarks>
    /// This struct provides flexibility in the translation system by allowing components to switch between
    /// displaying literal text (useful for values like numbers or time) and translated content from the localization system.
    /// The mode determines how the <see cref="Value"/> property is interpreted.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a translation key that looks up localized text
    /// var translationKey = SmartTranslationKey.Translation("ui.button.start");
    /// 
    /// // Create a raw text key for literal display
    /// var rawKey = SmartTranslationKey.Raw("Debug Text");
    /// 
    /// // Direct construction
    /// var key = new SmartTranslationKey
    /// {
    ///     Mode = SmartTranslationKeyMode.Translation,
    ///     Value = "menu.settings"
    /// };
    /// </code>
    /// </example>
    [Serializable]
    public struct SmartTranslationKey
    {
        /// <summary>
        /// Gets or sets the mode determining how the <see cref="Value"/> should be interpreted.
        /// </summary>
        /// <value>
        /// A <see cref="SmartTranslationKeyMode"/> indicating whether the value is raw text or a translation identifier.
        /// </value>
        public SmartTranslationKeyMode Mode
        {
            get => mode;
            set => mode = value;
        }

        /// <summary>
        /// Gets or sets the string value, interpreted according to the <see cref="Mode"/>.
        /// </summary>
        /// <value>
        /// Either a literal text string (if Mode is RawText) or a translation key identifier (if Mode is Translation).
        /// </value>
        public string Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeField] private SmartTranslationKeyMode mode;
        [SerializeField] private string value;
        
        /// <summary>
        /// Creates a <see cref="SmartTranslationKey"/> configured for raw text display.
        /// </summary>
        /// <param name="text">The literal text to display without translation.</param>
        /// <returns>A new <see cref="SmartTranslationKey"/> with <see cref="Mode"/> set to <see cref="SmartTranslationKeyMode.RawText"/>.</returns>
        /// <remarks>
        /// Use this method when you need to display literal text that should not be passed through
        /// the translation system, such as numbers, time, debug messages, user-generated content, or temporary placeholders.
        /// </remarks>
        /// <example>
        /// <code>
        /// var debugKey = SmartTranslationKey.Raw("100");
        /// </code>
        /// </example>
        public static SmartTranslationKey Raw(string text) => new()
        {
            Mode = SmartTranslationKeyMode.RawText,
            Value = text
        };

        /// <summary>
        /// Creates a <see cref="SmartTranslationKey"/> configured for translation lookup.
        /// </summary>
        /// <param name="id">The translation key identifier to look up in the localization system.</param>
        /// <returns>A new <see cref="SmartTranslationKey"/> with <see cref="Mode"/> set to <see cref="SmartTranslationKeyMode.Translation"/>.</returns>
        /// <remarks>
        /// Use this method when you want the value to be looked up in the translation system,
        /// returning localized text based on the current language setting.
        /// </remarks>
        /// <example>
        /// <code>
        /// var menuKey = SmartTranslationKey.Translation("menu.main.title");
        /// 
        /// // Using with parameters
        /// var greetingKey = SmartTranslationKey.Translation("greeting.welcome");
        /// translation.Key = greetingKey;
        /// translation.SetParameter("username", SmartTranslationKey.Translation("player.name"));
        /// </code>
        /// </example>
        public static SmartTranslationKey Translation(string id) => new()
        {
            Mode = SmartTranslationKeyMode.Translation,
            Value = id
        };
    }
}