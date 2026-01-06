using System;
using System.Linq;
using Game.Core.UI;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.UI.Hud
{
    public abstract class MediatorWithSelectables<T> : Mediator<T> where T : HudWithSelectables
    {
        private const float _navigateDelay = 0.2f; // delay between navigation steps

        [Inject] protected GameView _gameView;

        protected UISelectableView[] _selectables;
        protected int _selectedIndex;
        protected bool _isMobilePlatform;

        private float _lastNavigateTime;
        private Direction _lastDirection = Direction.None;

        private enum Direction { None, Up, Down, Left, Right }

        protected override void Show()
        {
            _selectables = _view.Selectables
                .Where(s => s.gameObject.activeInHierarchy)
                .ToArray();

            if (_selectables.Length == 0)
            {
                Log.Warning($"[{GetType().Name}] No visible selectables.");
                return;
            }

            _selectedIndex = 0;
            _isMobilePlatform = PlatformUtils.IsMobile;

            RegisterListeners();
            HighlightSelectable(_selectedIndex);
            SubscribeNavigation();
        }

        protected override void Hide()
        {
            UnsubscribeNavigation();
            UnregisterListeners();
            _selectables = null;
        }

        private void RegisterListeners()
        {
            foreach (var selectable in _selectables)
            {
                if (selectable is ButtonSelectableView buttonView)
                {
                    buttonView.Button.onClick.AddListener(() => OnSelectableClick(selectable));
                }
            }
        }

        private void UnregisterListeners()
        {
            foreach (var selectable in _selectables)
            {
                if (selectable is ButtonSelectableView buttonView)
                    buttonView.Button.onClick.RemoveAllListeners();
            }
        }

        protected virtual void OnSelectableClick(UISelectableView selectable)
        {
            _selectedIndex = Array.IndexOf(_selectables, selectable);
            HighlightSelectable(_selectedIndex);
            HandleSelectableClick(_selectedIndex);
        }

        protected abstract void HandleSelectableClick(int index);

        protected virtual void SubscribeNavigation()
        {
            _gameView.GameInput.ON_NAVIGATE_UP += OnNavigateUp;
            _gameView.GameInput.ON_NAVIGATE_DOWN += OnNavigateDown;
            _gameView.GameInput.ON_NAVIGATE_LEFT += OnNavigateLeft;
            _gameView.GameInput.ON_NAVIGATE_RIGHT += OnNavigateRight;
            _gameView.GameInput.ON_SELECTION += OnSelection;
            _gameView.GameInput.ON_ESCAPE += OnEscape;

            _gameView.GameInput.ON_NAVIGATION_RELEASED += ResetNavigateDelay;
        }

        protected virtual void UnsubscribeNavigation()
        {
            _gameView.GameInput.ON_NAVIGATE_UP -= OnNavigateUp;
            _gameView.GameInput.ON_NAVIGATE_DOWN -= OnNavigateDown;
            _gameView.GameInput.ON_NAVIGATE_LEFT -= OnNavigateLeft;
            _gameView.GameInput.ON_NAVIGATE_RIGHT -= OnNavigateRight;
            _gameView.GameInput.ON_SELECTION -= OnSelection;
            _gameView.GameInput.ON_ESCAPE -= OnEscape;

            _gameView.GameInput.ON_NAVIGATION_RELEASED -= ResetNavigateDelay;
        }

        private void OnNavigateUp() => Navigate(Direction.Up);
        private void OnNavigateDown() => Navigate(Direction.Down);
        private void OnNavigateLeft() => Navigate(Direction.Left);
        private void OnNavigateRight() => Navigate(Direction.Right);

        private void Navigate(Direction direction)
        {
            float currentTime = Time.unscaledTime;

            if (direction != _lastDirection)
                _lastNavigateTime = 0;

            if (currentTime - _lastNavigateTime < _navigateDelay)
                return;

            _lastDirection = direction;
            _lastNavigateTime = currentTime;

            switch (direction)
            {
                case Direction.Up:
                    HandleUp();
                    break;
                case Direction.Down:
                    HandleDown();
                    break;
                case Direction.Left:
                    HandleLeft();
                    break;
                case Direction.Right:
                    HandleRight();
                    break;
            }

            HighlightSelectable(_selectedIndex);
        }

        public virtual void HandleUp() { }
        public virtual void HandleDown() { }
        public virtual void HandleLeft() { }
        public virtual void HandleRight() { }

        public void SelectPrevious()
        {
            _selectedIndex = (_selectedIndex - 1 + _selectables.Length) % _selectables.Length;
        }

        public void SelectNext()
        {
            _selectedIndex = (_selectedIndex + 1) % _selectables.Length;
        }

        private void ResetNavigateDelay()
        {
            _lastDirection = Direction.None;
            _lastNavigateTime = 0;
        }

        protected virtual void OnSelection()
        {
            if (_selectables[_selectedIndex] is ButtonSelectableView button)
                button.Button.onClick.Invoke();
        }

        protected virtual void OnEscape() { }

        protected void HighlightSelectable(int index)
        {
            for (int i = 0; i < _selectables.Length; i++)
            {
                var marker = _selectables[i].Marker;

                var visibility = i == index;
                if(_isMobilePlatform)
                    visibility = false;

                marker.SetActive(visibility);
            }
        }
    }
}

