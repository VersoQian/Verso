using Core;
using UnityEngine;

namespace Game.UI.Hud
{
    /// <summary>
    /// Combines model binding with button navigation.
    /// Keeps the full observer functionality from <see cref="BaseHudWithModel{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">Observable model type.</typeparam>
    public abstract class HudWithSelectablesAndModel<TModel> : HudWithSelectables, IObserver where TModel : IObservable
    {
        private TModel _model;

        /// <summary>
        /// Gets or sets the model bound to this HUD.
        /// Automatically subscribes/unsubscribes from its change events.
        /// </summary>
        public TModel Model
        {
            protected get => _model;
            set
            {
                if (_model != null)
                    _model.RemoveObserver(this);

                OnApplyModel(value);
                _model = value;

                if (_model != null)
                {
                    _model.AddObserver(this);
                    OnModelChanged(_model);
                }
            }
        }

        /// <summary>
        /// Called after the model is set or changed.
        /// </summary>
        protected abstract void OnModelChanged(TModel model);

        /// <summary>
        /// Optional: called right before new model is applied.
        /// </summary>
        protected virtual void OnApplyModel(TModel model) { }

        /// <summary>
        /// Handles observable updates.
        /// </summary>
        public void OnObjectChanged(IObservable observable)
        {
            if (observable is TModel typedModel)
                OnModelChanged(typedModel);
            else if (_model != null)
                OnModelChanged(_model);
        }

        protected override void OnDisable()
        {
            if (_model != null)
            {
                _model.RemoveObserver(this);
                _model = default;
            }
        }
    }
}
