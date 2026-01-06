using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using Core;
using Game.Config;

namespace Game.UI.Hud
{
    public sealed class MenuHudModel : Observable
    {
        public MenuHudType Type;
        public int UnlockAtLevel;
        public string Label;
        public Sprite Icon;

        public bool IsClicked;
        public bool IsLocked;

        public MenuHudModel(MenuHudConfig config, int level)
        {
            Type = config.Type;
            UnlockAtLevel = config.UnlockAtLevel;
            Label = config.Label;
            Icon = config.Icon;
            IsLocked = config.UnlockAtLevel > level;
            IsClicked = false;
        }
    }

    public sealed class ButtonMenuView : BaseHudWithModel<MenuHudModel>
    {
        public event Action<MenuHudType> ON_CLICK; 

        private const float _changeSizeDuration = 0.16f;
        private const float _scaleMin = 0.5f;
        private const float _height = 45f;
        private const float _amplitude = 2f;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _clickedImage;
        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _rectTransformIcon;
        [SerializeField] private GameObject _lockedIcon;
        [SerializeField] private TMP_Text _labelText;

        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);

            DOTween.Kill(this);
        }

        private void OnButtonClick()
        {
            ON_CLICK?.Invoke(Model.Type);
        }

        private float SizeLeft
        {
            get
            {
                return _rectTransform.anchorMin.x;
            }
            set
            {
                _rectTransform.anchorMin = new Vector2(value, _rectTransform.anchorMin.y);
            }
        }

        private float SizeRight
        {
            get
            {
                return _rectTransform.anchorMax.x;
            }
            set
            {
                _rectTransform.anchorMax = new Vector2(value, _rectTransform.anchorMax.y);
            }
        }

        protected override void OnModelChanged(MenuHudModel model)
        {
            _icon.sprite = model.Icon;
            _labelText.text = model.Label;

            var scale = _scaleMin;
            var height = 0f;

            var isClicked = model.IsClicked;
            var isLocked = model.IsLocked;

            if (isClicked)
            {
                scale = 1f;
                height = _height;
            }

            _clickedImage.SetActive(isClicked && !isLocked);
            _icon.gameObject.SetActive(!isLocked);
            _lockedIcon.SetActive(isLocked);
            _labelText.gameObject.SetActive(isClicked && !isLocked);

            AnimateIcon(scale, height);
        }

        private void AnimateIcon(float scale, float height)
        {
            _rectTransformIcon.DOScale(Vector3.one * scale, _changeSizeDuration).SetEase(Ease.OutBack).SetId(this);
            _rectTransformIcon.DOAnchorPosY(height, _changeSizeDuration).SetEase(Ease.OutBack).SetId(this);
        }

        public void SetSize(Vector2 newValue)
        {
            DOTween.To(() => SizeLeft, x => SizeLeft = (float)x, newValue.x, _changeSizeDuration).SetEase(Ease.OutBack, _amplitude).SetId(this);
            DOTween.To(() => SizeRight, x => SizeRight = (float)x, newValue.y, _changeSizeDuration).SetEase(Ease.OutBack, _amplitude).SetId(this);
        }
    }
}

