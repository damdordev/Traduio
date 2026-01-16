using System.Collections.Generic;
using UnityEngine;

namespace Traduio
{
    /// <summary>
    /// Abstract base class for Unity components that display localized text.
    /// Automatically updates displayed text when translations change or when the translation manager refreshes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TranslationTarget connects UI components to the translation system, handling:
    /// - Automatic updates when translations changes
    /// - Parameter substitution in translations
    /// - Lifecycle management and event subscription
    /// - Deferred updates during batch operations
    /// </para>
    /// <para>
    /// Derived classes must implement:
    /// - Apply(string text): How to display the translated text
    /// - ManagerPointer property: Reference to the translation manager
    /// </para>
    /// </remarks>
    /// <example>
    /// Creating a custom translation target for TextMeshPro:
    /// <code>
    /// using TMPro;
    /// 
    /// public class TMProTranslationTarget : TranslationTarget
    /// {
    ///     public static TranslationManagerPointer managerPointer = new();
    ///
    ///     [SerializeField] private TMP_Text textComponent;
    ///     
    ///     protected override void Apply(string text)
    ///     {
    ///         if (textComponent != null)
    ///             textComponent.text = text;
    ///     }
    ///     
    ///     protected override TranslationManagerPointer ManagerPointer => managerPointer;
    /// }
    /// </code>
    /// 
    /// Using in Unity Inspector:
    /// <code>
    /// // Set Root to translation key "ui.welcome"
    /// // At runtime, the component will display the translated text
    /// 
    /// // Dynamic parameter updates:
    /// var target = GetComponent&lt;TMProTranslationTarget&gt;();
    /// target.SetParameter("playerName", TranslationRoot.Raw("Alice")); 
    /// // If translation is "Welcome, {playerName}!" 
    /// // Display will show: "Welcome, Alice!"
    /// </code>
    /// 
    /// Batch updates for better performance:
    /// <code>
    /// var target = GetComponent&lt;TranslationTarget&gt;();
    /// 
    /// target.StartChanges();  // Pause refresh
    /// target.SetParameter("score", TranslationRoot.Raw("100"));
    /// target.SetParameter("level", TranslationRoot.Raw("5"); });
    /// target.SetParameter("rank", TranslationRoot.Raw("Gold"); });
    /// target.CommitChanges(); // Single refresh for all changes
    /// </code>
    /// </example>
    public abstract class TranslationTarget : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the translation root that defines what text to display.
        /// Setting this property triggers refresh of the displayed text/
        /// </summary>
        /// <remarks>
        /// Triggers an immediate refresh unless called during a batch update (StartChanges).
        /// </remarks>
        /// <value>
        /// A TranslationRoot containing either a translation key or raw text,
        /// depending on its Mode property.
        /// </value>
        /// <example>
        /// <code>
        /// // Set to display a translation key
        /// target.Root = TranslationRoot.Translation(ui.title);
        /// 
        /// // Set to display raw text (not translated)
        /// target.Root = TranslationRoot.Raw("Hello world");
        /// </code>
        /// </example>
        public TranslationRoot Root
        {
            get => root;
            set
            {
                root = value;
                Refresh();
            }
        }

        [SerializeField] private TranslationRoot root;
        [SerializeField] private List<TranslationParameter> parameters = new();
        
        private TranslationManager manager;
        private bool needRefresh;
        private string currentText;
        private bool duringChanges;

        /// <summary>
        /// Begins a batch update operation, deferring text refresh until CommitChanges is called.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this when making multiple parameter changes to avoid redundant refreshes.
        /// Each call to SetParameter, ClearParameters, or setting Root normally triggers a refresh.
        /// </para>
        /// <para>
        /// Must be paired with CommitChanges() to apply the updates.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Without batching (3 refreshes):
        /// target.SetParameter("name", TranslationRoot.Translation(nameValue));
        /// target.SetParameter("score", TranslationRoot.Raw(scoreValue));
        /// target.SetParameter("time", TranslationRoot.Raw(timeValue));
        /// 
        /// // With batching (1 refresh):
        /// target.StartChanges();
        /// target.SetParameter("name", TranslationRoot.Translation(nameValue));
        /// target.SetParameter("score", TranslationRoot.Raw(scoreValue));
        /// target.SetParameter("time", TranslationRoot.Raw(timeValue)l
        /// target.CommitChanges();
        /// </code>
        /// </example>
        public void StartChanges()
        {
            duringChanges = true;
        }

        /// <summary>
        /// Completes a batch update operation and refreshes the displayed text.
        /// </summary>
        /// <remarks>
        /// Must be called after StartChanges() to apply all deferred updates.
        /// Triggers a single refresh regardless of how many changes were made.
        /// </remarks>
        /// <example>
        /// <code>
        /// target.SetParameter("player", TranslationRoot.Raw(playerName);
        /// target.SetParameter("health", TranslationRoot.Raw(health.ToString());
        /// target.Root = TranslationRoot.Translation("ui.status");
        /// target.CommitChanges(); 
        /// </code>
        /// </example>
        public void CommitChanges()
        {
            duringChanges = false;
            Refresh();
        }

        /// <summary>
        /// Removes all parameters and refreshes the displayed text.
        /// </summary>
        /// <remarks>
        /// This will cause any {placeholder} tokens in the translation to remain unreplaced.
        /// Triggers an immediate refresh unless called during a batch update (StartChanges).
        /// </remarks>
        /// <example>
        /// <code>
        /// // Setup with parameters
        /// target.SetParameter("name", TranslationRoot.Raw("Alice"));
        /// target.SetParameter("score", TranslationRoot.Raw("100");
        /// // Translation "Hello {name}, score: {score}" is "Hello Alice, score: 100"
        /// 
        /// // Clear all parameters
        /// target.ClearParameters();
        /// // Translation "Hello {name}, score: {score}" becomes "Hello {name}, score: {score}"
        /// </code>
        /// </example>
        public void ClearParameters()
        {
            parameters.Clear();
            Refresh();
        }

        /// <summary>
        /// Sets or updates a translation parameter for placeholder replacement.
        /// </summary>
        /// <param name="key">
        /// The parameter key that matches the placeholder in the translation (without braces).
        /// For example, use "name" to replace {name} in the translation.
        /// </param>
        /// <param name="value">
        /// The TranslationRoot containing the replacement value.
        /// Can be either raw text or another translation key.
        /// </param>
        /// <remarks>
        /// <para>
        /// If a parameter with the same key exists, it will be replaced.
        /// Otherwise, a new parameter is added.
        /// </para>
        /// <para>
        /// Triggers an immediate refresh unless called during a batch update (StartChanges).
        /// </para>
        /// </remarks>
        /// <example>
        /// Basic parameter setting:
        /// <code>
        /// // Translation: "Welcome, {username}!"
        /// target.SetParameter("username", "Alice");
        /// // Result: "Welcome, Alice!"
        /// </code>
        /// </example>
        public void SetParameter(string key, TranslationRoot value)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (key != parameters[i].Key) continue;
                
                parameters[i] = new TranslationParameter
                {
                    Key = key,
                    Value = value
                };
                Refresh();
                return;
            }

            parameters.Add(new TranslationParameter
            {
                Key = key,
                Value = value
            });
            Refresh();
        }

        /// <summary>
        /// Removes a translation parameter by key.
        /// </summary>
        /// <param name="key">The key of the parameter to remove.</param>
        /// <remarks>
        /// Triggers an immediate refresh unless called during a batch update (StartChanges).
        /// </remarks>
        /// <example>
        /// <code>
        /// target.SetParameter("tempValue", TranslationRoot.Raw(someValue));
        /// target.RemoveParameter("tempValue");
        /// 
        /// // Or use batch updates:
        /// target.StartChanges();
        /// target.RemoveParamater("param1");
        /// target.RemoveParamater("param2");
        /// target.CommitChanges(); // Single refresh
        /// </code>
        /// </example>
        public void RemoveParameter(string key)
        {
            parameters.RemoveAll(x => x.Key == key);
            Refresh();
        }
        
        /// <summary>
        /// Applies the translated text to the target component. Method has to be override in
        /// derived class.
        /// </summary>
        /// <param name="text">The fully translated and processed text to display.</param>
        /// <remarks>
        /// This method is called whenever the translation needs to be updated.
        /// Implementations should update their UI component with the provided text.
        /// </remarks>
        protected abstract void Apply(string text);
        
        /// <summary>
        /// Provides the reference to the translation manager.
        /// Method has to be override in derived class.
        /// </summary>
        /// <value>
        /// A TranslationManagerPointer that references the active translation manager.
        /// </value>
        /// <remarks>
        /// The manager pointer is used to subscribe to translation refresh events
        /// and to access the translation manager for text lookups.
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyTranslationTarget : TranslationTarget
        /// {
        ///     private static TranslationManagerPointer managerPointer = new();
        ///     
        ///     protected override TranslationManagerPointer ManagerPointer => managerPointer;
        /// }
        /// </code>
        /// </example>
        protected abstract TranslationManagerPointer ManagerPointer { get; }
        
        protected virtual void Awake()
        {
            ManagerPointer.Changed += OnManagerChanged;
            manager = ManagerPointer.Manager;
            if (manager != null) manager.Refresh += Refresh;
            Refresh();
        }

        protected virtual void OnEnable()
        {
            if (needRefresh) Refresh();
        }
        
        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnDestroy()
        {
            ManagerPointer.Changed -= OnManagerChanged;
            if (manager != null)
            {
                manager.Refresh -= Refresh;
                manager = null;
            }
        }

        private void OnManagerChanged()
        {
            if (manager != null)
            {
                manager.Refresh -= Refresh;
                manager = null;
            }
            manager = ManagerPointer.Manager;
            if (manager != null)
            {
                manager.Refresh += Refresh;
            }
        }

        protected void Refresh()
        {
            if (!isActiveAndEnabled || duringChanges)
            {
                needRefresh = true;
                return;
            }
            needRefresh = false;
            if (manager != null)
            {
                var text = manager.Translate(root, parameters);
                if (text == currentText) return;
                currentText = text;
                Apply(currentText);
            }
        }
    }
}