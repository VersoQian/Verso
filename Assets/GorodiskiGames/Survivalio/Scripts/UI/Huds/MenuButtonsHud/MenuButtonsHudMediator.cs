using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class MenuButtonsHudMediator : Mediator<MenuButtonsHudView>
    {
        private const float _activeButtonSize = 1.6f;

        [Inject] private GameView _gameView;
        [Inject] private GameConfig _config;
        [Inject] private MenuManager _menuManager;

        private int _currentHudID;

        private readonly Dictionary<MenuHudType, MenuHudModel> _menuMap; 

        public MenuButtonsHudMediator()
        {
            _menuMap = new Dictionary<MenuHudType, MenuHudModel>();
        }

        protected override void Show()
        {
            _currentHudID = int.MinValue;

            var level = 0;

            foreach (var button in _view.Buttons)
            {
                var index = _view.Buttons.ToList().IndexOf(button);
                var config = _config.MenuHudConfigs[index];
                var model = new MenuHudModel(config, level);
                button.Model = model;

                _menuMap[model.Type] = model;

                button.ON_CLICK += OnMenuButtonClick;
            }

            var defaultHud = _config.DefaultMenuHud;
            OnMenuButtonClick(defaultHud);

            _menuManager.ON_MENU_BUTTON_CLICK += OnMenuButtonClick;
        }

        protected override void Hide()
        {
            _menuManager.ON_MENU_BUTTON_CLICK -= OnMenuButtonClick;

            foreach (var button in _view.Buttons)
            {
                button.ON_CLICK -= OnMenuButtonClick;
                button.Model = null;
            }

            _menuMap.Clear();
        }

        private void OnMenuButtonClick(MenuHudType clickedHudType)
        {
            var clickedHudID = (int)clickedHudType;

            var type = (MenuHudType)clickedHudID;
            var model = _menuMap[type];

            if (_currentHudID == clickedHudID || model.IsLocked)
                return;

            _currentHudID = clickedHudID;

            SetWindowPositions(_currentHudID);
            SetButtonsPositions(_currentHudID);
        }

        private void SetWindowPositions(int clickedHudID)
        {
            for (int i = 0; i < _view.Huds.Length; i++)
            {
                var xPosition = Screen.width * (i - clickedHudID) / _gameView.Canvas.scaleFactor;
                _view.Huds[i].anchoredPosition = new Vector2(xPosition, 0f);
            }
        }

        private void SetButtonsPositions(int clickedHudID)
        {
            var buttonsCount = _view.Buttons.Length;
            var buttonSizeBig = (1f / buttonsCount) * _activeButtonSize;
            var buttonSize = (1f - buttonSizeBig) / (buttonsCount - 1);
            float xMin, xMax = 0f;

            for (int i = 0; i < buttonsCount; i++)
            {
                var isClicked = i == clickedHudID;
                if (isClicked)
                {
                    if (i == 0)
                        xMin = 0f;
                    else
                        xMin = xMax;
                    xMax += buttonSizeBig;
                }
                else
                {
                    xMin = xMax;
                    xMax += buttonSize;
                }

                var type = (MenuHudType)i;
                var model = _menuMap[type];

                model.IsClicked = isClicked;
                model.SetChanged();

                var size = new Vector2(xMin, xMax);
                _view.Buttons[i].SetSize(size);
            }
        }
    }
}

