using System.Collections.Generic;
using UnityEngine;

namespace Traduio
{
    public abstract class TranslationTarget : MonoBehaviour
    {
        public TranslationRoot Root
        {
            get => root;
            set
            {
                root = value;
                Refresh();
            }
        }
        
        protected abstract void Apply(string text);
        protected abstract TranslationManagerPointer ManagerPointer { get; }

        [SerializeField] private TranslationRoot root;
        [SerializeField] private List<TranslationParameter> parameters;
        
        private TranslationManager manager;
        private bool needRefresh;
        private string currentText;
        private bool duringChanges;

        public void StartChanges()
        {
            duringChanges = true;
        }

        public void CommitChanges()
        {
            duringChanges = false;
            Refresh();
        }

        public void ClearParameters()
        {
            parameters.Clear();
            Refresh();
        }

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
                return;
            }

            parameters.Add(new TranslationParameter
            {
                Key = key,
                Value = value
            });
        }

        public void RemoveParamater(string key)
        {
            parameters.RemoveAll(x => x.Key == key);
        }
        
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
                var text = manager.Translate(root);
                if (text == currentText) return;
                currentText = text;
                Apply(currentText);
            }
        }
    }
}