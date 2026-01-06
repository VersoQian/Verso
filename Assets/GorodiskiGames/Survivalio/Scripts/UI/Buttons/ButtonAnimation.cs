using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float _toScaleIn = .95f;
        private const float _durationIn = .05f;
        private const float _durationOut = .1f;
        private const float _amplitude = 1f;

        private RectTransform _rectTransform;
        private Button _button;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_rectTransform || !_button.interactable)
                return;

            PlayScaleIn();
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
            _rectTransform.localScale = Vector3.one;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_rectTransform || !_button.interactable)
                return;

            PlayScaleOut();
        }

        public void PlayScaleIn()
        {
            _rectTransform.DOKill(this);
            _rectTransform.DOScale(Vector3.one * _toScaleIn, _durationIn).SetEase(Ease.InOutQuad).SetId(this);
        }

        public void PlayScaleOut()
        {
            _rectTransform.DOKill(this);
            _rectTransform.DOScale(Vector3.one, _durationOut).SetEase(Ease.OutBack, _amplitude).SetId(this);
        }
    }
}