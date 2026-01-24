using System.Collections.Generic;
using UnityEngine;

namespace Damdor.Traduio
{
    /// <summary>
    /// Abstract base class for translation targets that support parameterized translations.
    /// Extends <see cref="TranslationTarget"/> to provide dynamic text replacement with translation keys.
    /// </summary>
    /// <remarks>
    /// SmartTranslation enables localization with dynamic content by supporting parameters that can be
    /// substituted into translation strings. Parameters are themselves translation keys, allowing for
    /// fully localized dynamic content.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class TextSmartTranslation : SmartTranslation
    /// {
    ///     [SerializeField] private TMP_Text textComponent;
    ///     
    ///     protected override void Apply(string text)
    ///     {
    ///         textComponent.text = text;
    ///     }
    /// }
    /// 
    /// // Usage in a MonoBehaviour
    /// var translation = GetComponent&lt;TextSmartTranslation&gt;();
    /// translation.Key = SmartTranslationKey.Translation("welcome_message");
    /// translation.SetParameter("username", SmartTranslationKey.Raw(player_name));
    /// // If "welcome_message" = "Hello, {username}!" and "player_name" = "John"
    /// // Result: "Hello, John!"
    /// </code>
    /// </example>
    public abstract class BaseSmartTranslation : TranslationTarget
    {
        /// <summary>
        /// Gets or sets the translation key used to retrieve the localized text.
        /// </summary>
        /// <remarks>
        /// Setting this property automatically triggers a refresh of the displayed text.
        /// </remarks>
        /// <example>
        /// <code>
        /// translation.Key = SmartTranslationKey.Translation("ui.button.confirm");
        /// </code>
        /// </example>
        public SmartTranslationKey Key
        {
            get => key;
            set
            {
                key = value;
                Refresh();
            }
        }

        [SerializeField] private SmartTranslationKey key;
        [SerializeField] private List<SmartTranslationParameter> parameters = new();
        
        private string currentText;
        
        /// <summary>
        /// Removes all translation parameters and refreshes the displayed text.
        /// </summary>
        /// <remarks>
        /// This method clears all dynamic parameters, reverting the translation to its base form
        /// without any substitutions.
        /// </remarks>
        /// <example>
        /// <code>
        /// translation.SetParameter("score", SmartTranslationKey.Raw("current_score"));
        /// translation.SetParameter("level", SmartTranslationKey.Raw("current_level"));
        /// translation.ClearParameters(); // Removes both parameters
        /// </code>
        /// </example>
        public void ClearParameters()
        {
            parameters.Clear();
            Refresh();
        }
        
        /// <summary>
        /// Sets or updates a translation parameter with the specified key and value.
        /// </summary>
        /// <param name="key">The parameter identifier to set or update.</param>
        /// <param name="value">The translation key to use as the parameter value.</param>
        /// <remarks>
        /// If a parameter with the given key already exists, it will be updated.
        /// Otherwise, a new parameter will be added. The text is automatically refreshed
        /// after the parameter is set.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Translation string: "You have {itemCount} items in your inventory"
        /// translation.SetParameter("itemCount", SmartTranslationKey.Translation("items.count.5"));
        /// 
        /// // Update the same parameter
        /// translation.SetParameter("itemCount", SmartTranslationKey.Translation("items.count.10");
        /// </code>
        /// </example>
        public void SetParameter(string key, SmartTranslationKey value)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (key != parameters[i].Key) continue;
                
                parameters[i] = new SmartTranslationParameter
                {
                    Key = key,
                    Value = value
                };
                Refresh();
                return;
            }

            parameters.Add(new SmartTranslationParameter
            {
                Key = key,
                Value = value
            });
            Refresh();
        }
        
        /// <summary>
        /// Removes all parameters matching the specified key and refreshes the displayed text.
        /// </summary>
        /// <param name="key">The parameter identifier to remove.</param>
        /// <remarks>
        /// If no parameter with the given key exists, this method has no effect.
        /// The text is automatically refreshed after removal.
        /// </remarks>
        /// <example>
        /// <code>
        /// translation.SetParameter("playerName", SmartTranslationKey.Translation("name.player1"));
        /// translation.RemoveParameter("playerName");
        /// </code>
        /// </example>
        public void RemoveParameter(string key)
        {
            parameters.RemoveAll(x => x.Key == key);
            Refresh();
        }
        
        /// <summary>
        /// When implemented in a derived class, applies the translated text to the target component.
        /// </summary>
        /// <param name="text">The fully translated and parameterized text to apply.</param>
        /// <remarks>
        /// This method must be implemented by derived classes to define how the translated text
        /// is displayed (e.g., setting it to a TextMeshPro component, UI Text, etc.).
        /// </remarks>
        /// <example>
        /// <code>
        /// public class TextSmartTranslation : SmartTranslation
        /// {
        ///     [SerializeField] private TMP_Text textComponent;
        ///     
        ///     protected override void Apply(string text)
        ///     {
        ///         textComponent.text = text;
        ///     }
        /// }
        /// </code>
        /// </example>
        protected abstract void Apply(string text);

        protected sealed override void Apply()
        {
            var text = manager.Translate(key, parameters);
            if (text == currentText) return;
            currentText = text;
            Apply(currentText);
        }

        protected override void BeforeAllTargetsApply()
            => StartChanges();

        protected override void AfterAllTargetsApply()
            => CommitChanges();
    }
}