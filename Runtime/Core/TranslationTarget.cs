using UnityEngine;

namespace Traduio
{
    /// <summary>
    /// Abstract base class for Unity components update their content based on a <see cref="TranslationManager"/>.
    /// Automatically updates content when translations change or when the translation manager refreshes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// TranslationTarget connects UI components to the translation system, handling:
    /// - Automatic updates when translations changes
    /// - Lifecycle management and event subscription
    /// - Deferred updates during batch operations
    /// </para>
    /// <para>
    /// Derived classes must implement:
    /// - Apply(): How to modify content
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
    ///     [SerializeField] private string id;
    ///     
    ///     protected override void Apply()
    ///     {
    ///         if (textComponent != null)
    ///             textComponent.text = manager.GetTranslation(id);
    ///     }
    ///     
    ///     protected override TranslationManagerPointer ManagerPointer => managerPointer;
    /// }
    /// </code>
    /// 
    /// 
    /// Batch updates for better performance:
    /// <code>
    /// var target = GetComponent&lt;MyTranslationTarget&gt;();
    /// 
    /// target.StartChanges();  // Pause refresh
    /// target.DoChange1();
    /// target.DoChange2();
    /// target.DoChange3();
    /// target.CommitChanges(); // Single refresh for all changes
    /// </code>
    /// </example>
    public abstract class TranslationTarget : MonoBehaviour
    {
        private int changesLevel;
        private bool needRefresh;
        
        /// <summary>
        /// Gets the current <see cref="TranslationManager"/> instance assigned to this target.
        /// </summary>
        protected TranslationManager manager { get; private set; }
        
        /// <summary>
        /// Begins a batch update operation, deferring text refresh until CommitChanges is called.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this when making multiple parameter changes to avoid redundant refreshes.
        /// </para>
        /// <para>
        /// Must be paired with CommitChanges() to apply the updates.
        /// </para>
        /// <para>
        /// StartChanges() and CommitChanges() can be nested. Each StartChange() needs a corresponding CommitChanged();
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Without batching (3 refreshes):
        /// target.DoChange1();
        /// target.DoChange2();
        /// target.DoChange3();
        /// 
        /// // With batching (1 refresh):
        /// target.StartChanges();
        /// target.DoChange1();
        /// target.DoChange2();
        /// target.DoChange3();
        /// target.CommitChanges();
        ///
        /// // With nesting batching:
        /// target.StartChanges();
        /// target.StartChanges();
        /// target.DoChange1();
        /// target.DoChange2();
        /// target.DoChange3();
        /// target.CommitChanges();  // no refresh done here
        /// target.CommitChanges();  // refresh done here
        /// </code>
        /// </example>
        public void StartChanges()
        {
            ++changesLevel;
        }
        
        /// <summary>
        /// Resumes the <see cref="Refresh"/> mechanism and triggers an immediate refresh of the content.
        /// </summary>
        public void CommitChanges()
        {
            --changesLevel;
            if(changesLevel <= 0 && needRefresh) Refresh();
        }
        
        /// <summary>
        /// Provides the reference to the translation manager.
        /// Method has to be overriden in derived class.
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
        ///     protected override void Apply()
        ///     {
        ///         Debug.Log("modify content");
        ///     }
        /// }
        /// </code>
        /// </example>
        protected abstract TranslationManagerPointer ManagerPointer { get; }
        
        /// <summary>
        /// Implements the logic to update the content with the localized data from the <see cref="manager"/>.
        /// Method has to be overriden in derived class.
        /// </summary>
        protected abstract void Apply();
        
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

        protected virtual void OnValidate()
        {
            Refresh();
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

        /// <summary>
        /// Evaluates whether the target should be updated. 
        /// Updates are skipped if the object is inactive or if <see cref="StartChanges"/> has been called.
        /// </summary>
        protected void Refresh()
        {
            if (!isActiveAndEnabled || changesLevel > 0)
            {
                needRefresh = true;
                return;
            }
            needRefresh = false;
            if (manager == null) return;
            Apply();
        }
        
    }
}