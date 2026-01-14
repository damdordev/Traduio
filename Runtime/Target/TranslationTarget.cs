using UnityEngine;

namespace Traduio
{
    public abstract class TranslationTarget : MonoBehaviour
    {
        public string Key
        {
            get => key;
            set => key = value;
        }
        
        [SerializeField] private string key;
        
        protected abstract void Apply(string text);
        protected abstract TranslationManagerPointer ManagerPointer { get; }

        private TranslationManager manager;
        private bool needRefresh;
        private string currentText;

        protected virtual void Awake()
        {
            ManagerPointer.Changed += OnManagerChanged;
            manager = ManagerPointer.Manager;
            if (manager != null) manager.Refresh += OnManagerRefresh;
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
                manager.Refresh -= OnManagerRefresh;
                manager = null;
            }
        }

        private void OnManagerChanged()
        {
            if (manager != null)
            {
                manager.Refresh -= OnManagerRefresh;
                manager = null;
            }
            manager = ManagerPointer.Manager;
            if (manager != null)
            {
                manager.Refresh += OnManagerRefresh;
            }
        }
        
        private void OnManagerRefresh()
        {
            if (isActiveAndEnabled) Refresh();
            else needRefresh = true;
        }

        protected void Refresh()
        {
            needRefresh = false;
            if (manager != null)
            {
                var text = manager.GetTranslation(key);
                if (text == currentText) return;
                currentText = text;
                Apply(currentText);
            }
        }
    }
}