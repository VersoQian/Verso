using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Modules
{
    public enum UINotificationColorType
    {
        White,
        Yellow,
        Red
    }

    public sealed class UINotificationPopupView : MonoBehaviour
    {
        private const float _scaleStart = 0.75f;
        private const float _scaleEnd = 1f;
        private const float _introDeltaPosition = 50f;
        private const float _introDuration = 0.3f;
        private const float _slightDeltaPosition = 20f;
        private const float _slightMoveDuration = 0.5f;
        private const float _stayDuration = 0.3f;
        private const float _fadeOutDuration = 0.3f;

        public event Action<UINotificationPopupView> ON_END;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _infoText;

        private Vector3 _introPosition;
        private Vector3 _endPosition;

        public void Initialize(string info, Color color, Vector3 position)
        {
            _infoText.text = info;
            _infoText.color = color;

            transform.position = position;
            transform.localScale = Vector3.one * _scaleStart;

            _canvasGroup.alpha = 0f;
            _introPosition = position + Vector3.up * _introDeltaPosition;
            _endPosition = _introPosition + Vector3.up * _slightDeltaPosition;

            PlayAnimation();
        }

        private void OnDisable()
        {
            KillTween();
        }

        private void PlayAnimation()
        {
            Sequence sequence = DOTween.Sequence().SetId(this);

            sequence.Append(transform.DOMoveY(_introPosition.y, _introDuration).SetEase(Ease.OutQuad));
            sequence.Join(transform.DOScale(Vector3.one * _scaleEnd, _introDuration).SetEase(Ease.OutBack));
            sequence.Join(_canvasGroup.DOFade(1f, _introDuration));

            sequence.Append(transform.DOMoveY(_endPosition.y, _slightMoveDuration).SetEase(Ease.Linear));
            sequence.AppendInterval(_stayDuration);
            sequence.Append(_canvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad));

            sequence.OnComplete(() =>
            {
                KillTween();
                FireEnd();
            });
        }

        private void FireEnd()
        {
            ON_END?.Invoke(this);
        }

        private void KillTween()
        {
            DOTween.Kill(this);
        }
    }
}

