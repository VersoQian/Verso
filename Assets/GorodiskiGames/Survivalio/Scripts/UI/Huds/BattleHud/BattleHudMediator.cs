using Game.Config;
using Game.Managers;
using Game.States;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class BattleHudMediator : RawCameraHudMediator<BattleHudView>
    {
        private const string _startLevelPriceFormat = "{0}{1}";
        private const int _startLevelPrice = 5;

        [Inject] private HudManager _hudManager;
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private MenuManager _menuManager;
        [Inject] private GameConfig _config;

        protected override void Show()
        {
            // 隐藏3D预览的RawImage，显示2D预览图片
            if (_view.RawImage != null)
                _view.RawImage.gameObject.SetActive(false);

            if (_view.PreviewImage != null)
                _view.PreviewImage.gameObject.SetActive(true);

            // 设置开始价格文本 - 使用简单的闪电图标
            _view.StartPriceText.text = $"⚡ {_startLevelPrice}";

            UpdateLevelDisplay();

            _view.SettingsButton.onClick.AddListener(OnSettingsButtonClick);
            _view.StartButton.onClick.AddListener(OnStartButtonClick);

            if (_view.PreviousLevelButton != null)
                _view.PreviousLevelButton.onClick.AddListener(OnPreviousLevelButtonClick);

            if (_view.NextLevelButton != null)
                _view.NextLevelButton.onClick.AddListener(OnNextLevelButtonClick);
        }

        protected override void Hide()
        {
            _view.SettingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            _view.StartButton.onClick.RemoveListener(OnStartButtonClick);

            if (_view.PreviousLevelButton != null)
                _view.PreviousLevelButton.onClick.RemoveListener(OnPreviousLevelButtonClick);

            if (_view.NextLevelButton != null)
                _view.NextLevelButton.onClick.RemoveListener(OnNextLevelButtonClick);
        }

        private void OnSettingsButtonClick()
        {
            _hudManager.ShowSingle<SettingsHudMediator>();
        }

        private void OnStartButtonClick()
        {
            if(_menuManager.Model.Energy < _startLevelPrice)
                return;

            var energy = _menuManager.Model.Energy;
            energy -= _startLevelPrice;

            _menuManager.Model.Energy = energy;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();

            _gameStateManager.SwitchToState(new GamePlayState());
        }

        private void OnPreviousLevelButtonClick()
        {
            var currentLevel = _menuManager.Model.Level;
            if (currentLevel > 0)
            {
                _menuManager.Model.Level = currentLevel - 1;
                _menuManager.Model.Save();
                _menuManager.Model.SetChanged();
                UpdateLevelDisplay();
            }
        }

        private void OnNextLevelButtonClick()
        {
            var currentLevel = _menuManager.Model.Level;
            var maxLevel = _config.LevelConfigs.Length - 1;
            if (currentLevel < maxLevel)
            {
                _menuManager.Model.Level = currentLevel + 1;
                _menuManager.Model.Save();
                _menuManager.Model.SetChanged();
                UpdateLevelDisplay();
            }
        }

        private void UpdateLevelDisplay()
        {
            var level = _menuManager.Model.Level;
            var levelConfig = _config.LevelConfigs[level];
            var levelLabel = levelConfig.Label;
            var levelDurationMax = _menuManager.Model.LevelDurationMax;

            // 更新2D预览图片
            if (_view.PreviewImage != null)
            {
                // 优先使用配置的PreviewImage
                if (levelConfig.PreviewImage != null)
                {
                    _view.PreviewImage.sprite = levelConfig.PreviewImage;
                }
                else
                {
                    // 如果配置里没有，尝试从 Resources 动态加载
                    var levelIndex = level + 1; // Level 1, Level 2...
                    var spritePath = $"LevelPreviews/Level{levelIndex}";
                    var sprite = Resources.Load<Sprite>(spritePath);

                    if (sprite != null)
                    {
                        _view.PreviewImage.sprite = sprite;
                    }
                    else
                    {
                        // 如果都找不到，显示警告
                        Log.Warning($"LevelConfig '{levelConfig.name}' does not have a PreviewImage assigned, and no image found at '{spritePath}'!");
                    }
                }
            }

            var model = new BattleHudModel(level, levelLabel, levelDurationMax);
            _view.Model = model;

            // 更新按钮可用状态
            if (_view.PreviousLevelButton != null)
                _view.PreviousLevelButton.interactable = level > 0;

            if (_view.NextLevelButton != null)
                _view.NextLevelButton.interactable = level < _config.LevelConfigs.Length - 1;
        }
    }
}

