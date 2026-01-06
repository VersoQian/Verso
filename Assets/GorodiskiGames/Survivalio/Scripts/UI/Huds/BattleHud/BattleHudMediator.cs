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

        private const float _previewRootHeight = 5000f;
        private const int _previewTextureDepth = 24;

        private GameObject _previewRoot;
        private GameObject _previewInstance;
        private Camera _previewCamera;
        private RenderTexture _previewRenderTexture;

        protected override void Show()
        {
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
            CleanupPreview();

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

            var previewReady = TrySetupLevelCameraPreview(levelConfig);
            if (!previewReady)
                Update2DPreview(level, levelConfig);

            var model = new BattleHudModel(level, levelLabel, levelDurationMax);
            _view.Model = model;

            // 更新按钮可用状态
            if (_view.PreviousLevelButton != null)
                _view.PreviousLevelButton.interactable = level > 0;

            if (_view.NextLevelButton != null)
                _view.NextLevelButton.interactable = level < _config.LevelConfigs.Length - 1;
        }

        private bool TrySetupLevelCameraPreview(LevelConfig levelConfig)
        {
            CleanupPreview();

            if (_view.RawImage == null)
                return false;

            var prefab = levelConfig.Prefab != null ? levelConfig.Prefab : levelConfig.Preview;
            if (prefab == null)
                return false;

            EnsurePreviewRoot();

            _previewInstance = GameObject.Instantiate(prefab, _previewRoot.transform);
            _previewInstance.transform.localPosition = Vector3.zero;
            _previewInstance.transform.localRotation = Quaternion.identity;

            _previewCamera = _previewInstance.GetComponentInChildren<Camera>();
            if (_previewCamera == null)
                _previewCamera = CreateFallbackCamera(_previewInstance);

            if (_previewCamera == null)
            {
                CleanupPreview();
                return false;
            }

            var rawRect = _view.RawImage.rectTransform.rect;
            var width = rawRect.width > 0 ? (int)rawRect.width : Screen.width;
            var height = rawRect.height > 0 ? (int)rawRect.height : Screen.height;
            width = Mathf.Max(256, width);
            height = Mathf.Max(256, height);

            _previewRenderTexture = new RenderTexture(width, height, _previewTextureDepth);
            _previewCamera.targetTexture = _previewRenderTexture;

            _view.RawImage.texture = _previewRenderTexture;
            _view.RawImage.gameObject.SetActive(true);

            if (_view.PreviewImage != null)
                _view.PreviewImage.gameObject.SetActive(false);

            return true;
        }

        private Camera CreateFallbackCamera(GameObject root)
        {
            var bounds = CalculateBounds(root);
            var targetCenter = bounds.center;
            var maxSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            if (maxSize <= 0f)
            {
                targetCenter = root.transform.position;
                maxSize = 10f;
            }

            var distance = Mathf.Max(10f, maxSize * 3f);
            var offset = new Vector3(0f, maxSize * 1.5f, -distance);

            var cameraGo = new GameObject("PreviewCamera");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.transform.position = targetCenter + offset;
            cameraGo.transform.LookAt(targetCenter);

            var camera = cameraGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 5000f;

            return camera;
        }

        private Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(root.transform.position, Vector3.zero);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        private void EnsurePreviewRoot()
        {
            if (_previewRoot != null)
                return;

            _previewRoot = new GameObject("LevelPreviewRoot");
            _previewRoot.transform.position = Vector3.up * _previewRootHeight;
        }

        private void CleanupPreview()
        {
            if (_previewCamera != null)
                _previewCamera.targetTexture = null;

            if (_previewRenderTexture != null)
            {
                _previewRenderTexture.Release();
                Object.Destroy(_previewRenderTexture);
                _previewRenderTexture = null;
            }

            if (_previewInstance != null)
            {
                Object.Destroy(_previewInstance);
                _previewInstance = null;
            }

            if (_previewRoot != null)
            {
                Object.Destroy(_previewRoot);
                _previewRoot = null;
            }

            if (_view.RawImage != null)
            {
                _view.RawImage.texture = null;
                _view.RawImage.gameObject.SetActive(false);
            }

            if (_view.PreviewImage != null)
                _view.PreviewImage.gameObject.SetActive(true);
        }

        private void Update2DPreview(int level, LevelConfig levelConfig)
        {
            if (_view.PreviewImage == null)
                return;

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

            _view.PreviewImage.gameObject.SetActive(true);
        }
    }
}
