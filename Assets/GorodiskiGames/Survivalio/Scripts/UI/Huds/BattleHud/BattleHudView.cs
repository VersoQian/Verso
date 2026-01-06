using System;
using Core;
using Game.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class BattleHudModel : Observable
    {
        public int Level;
        public string LevelLabel;
        public float DurationMax;

        public BattleHudModel(int level, string levelLabel, float durationMax)
        {
            Level = level;
            LevelLabel = levelLabel;
            DurationMax = durationMax;
        }
    }

    public sealed class BattleHudView : BaseHudWithModel<BattleHudModel>
    {
        private const string _levelLabelFormat = "{0}. {1}";
        private const string _durationMaxFormat = "BEST TIME: {0}";

        [SerializeField] private RawImage _rawImage;
        [SerializeField] private Image _previewImage;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _levelLabelText;
        [SerializeField] private TMP_Text _durationMaxText;
        [SerializeField] private Button _startButton;
        [SerializeField] private TMP_Text _startButtonText;
        [SerializeField] private TMP_Text _startPriceText;
        [SerializeField] private Button _previousLevelButton;
        [SerializeField] private Button _nextLevelButton;

        public RawImage RawImage => _rawImage;
        public Image PreviewImage => _previewImage;
        public Button SettingsButton => _settingsButton;
        public Button StartButton => _startButton;
        public TMP_Text StartPriceText => _startPriceText;
        public Button PreviousLevelButton => _previousLevelButton;
        public Button NextLevelButton => _nextLevelButton;

        protected override void OnEnable()
        {
            // 自动查找并绑定组件（如果Inspector里没有手动配置）
            AutoBindComponents();
        }

        private void AutoBindComponents()
        {
            // 如果PreviewImage没有在Inspector配置，尝试自动查找
            if (_previewImage == null)
            {
                _previewImage = transform.Find("PreviewImage")?.GetComponent<Image>();
                if (_previewImage == null)
                {
                    _previewImage = transform.Find("Preview")?.GetComponent<Image>();
                }
                if (_previewImage == null)
                {
                    // 查找所有Image组件，找到最大的那个作为预览图
                    var images = GetComponentsInChildren<Image>(true);
                    Image largestImage = null;
                    float maxSize = 0;
                    foreach (var img in images)
                    {
                        if (img == this.GetComponent<Image>()) continue; // 跳过自己
                        var rect = img.rectTransform.rect;
                        float size = rect.width * rect.height;
                        if (size > maxSize)
                        {
                            maxSize = size;
                            largestImage = img;
                        }
                    }
                    _previewImage = largestImage;
                }
            }

            // 设置PreviewImage位置（往下移动）
            if (_previewImage != null)
            {
                var rectTransform = _previewImage.rectTransform;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(0, -50); // 往下移动到 -50
            }

            // 如果Previous按钮没有配置，尝试自动查找
            if (_previousLevelButton == null)
            {
                _previousLevelButton = transform.Find("PreviousLevelButton")?.GetComponent<Button>();
                if (_previousLevelButton == null)
                {
                    _previousLevelButton = transform.Find("PreviousButton")?.GetComponent<Button>();
                }
                if (_previousLevelButton == null)
                {
                    _previousLevelButton = transform.Find("PrevButton")?.GetComponent<Button>();
                }
            }

            // 设置Previous按钮位置
            if (_previousLevelButton != null)
            {
                // 设置位置和大小
                var rectTransform = _previousLevelButton.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(-900, 50); // 改成 -900
                rectTransform.sizeDelta = new Vector2(350, 140);
            }

            // 如果Next按钮没有配置，尝试自动查找
            if (_nextLevelButton == null)
            {
                _nextLevelButton = transform.Find("NextLevelButton")?.GetComponent<Button>();
                if (_nextLevelButton == null)
                {
                    _nextLevelButton = transform.Find("NextButton")?.GetComponent<Button>();
                }
            }

            // 设置Next按钮位置
            if (_nextLevelButton != null)
            {
                // 设置位置和大小
                var rectTransform = _nextLevelButton.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(900, 50); // 改成 900
                rectTransform.sizeDelta = new Vector2(350, 140);
            }
        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(BattleHudModel model)
        {
            var levelNice = model.Level + 1; 
            _levelLabelText.text = string.Format(_levelLabelFormat, levelNice, model.LevelLabel);

            var durationMax = TimeSpan.FromSeconds(model.DurationMax).DateToMMSS();
            _durationMaxText.text = string.Format(_durationMaxFormat, durationMax);
        }
    }
}

