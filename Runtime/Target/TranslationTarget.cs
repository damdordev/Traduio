using UnityEngine;

namespace Traduio
{
    public abstract class TranslationTarget : MonoBehaviour
    {
        private bool duringChanges;
        private bool needRefresh;
        protected TranslationManager manager { get; private set; }
        
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

        protected void OnValidate()
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

        protected void Refresh()
        {
            if (!isActiveAndEnabled || duringChanges)
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