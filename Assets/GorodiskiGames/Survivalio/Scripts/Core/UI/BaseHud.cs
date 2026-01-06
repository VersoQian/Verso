using Core;
using UnityEngine;

namespace Game.UI.Hud
{
    public interface IHud
    {
        bool IsActive
        {
            set;
        }
    }

    public abstract class BaseHud : MonoBehaviour, IHud
    {
        public bool IsActive
        {
            set
            {
                gameObject.SetActive(value);
            }
        }
        protected abstract void OnDisable();
        protected abstract void OnEnable();
    }

    public abstract class BaseHudWithModel<T> : BaseHud, IObserver where T : IObservable
    {
        private T _model;

        public T Model
        {
            protected get
            {
                return _model;
            }
            set
            {
                if (null != _model)
                {
                    _model.RemoveObserver(this);
                }

                OnApplyModel(value);

                _model = value;

                if (null != _model)
                {
                    _model.AddObserver(this);
                    OnModelChanged(_model);
                }
            }
        }

        protected BaseHudWithModel()
        {
        }

        protected abstract void OnModelChanged(T model);

        protected virtual void OnApplyModel(T model)
        {
        }

        #region Observer implementation
        public void OnObjectChanged(IObservable observable)
        {
            if (observable is T)
            {
                OnModelChanged((T)observable);
            }
            else
            {
                OnModelChanged(Model);
            }
        }
        #endregion
    }
}