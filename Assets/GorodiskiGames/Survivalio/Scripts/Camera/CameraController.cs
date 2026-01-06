using UnityEngine;
using DG.Tweening;

namespace Game
{
    public sealed class CameraController : MonoBehaviour
    {
        private const int _shakeVibrato = 20;
        private const float _shakeDuration = 0.3f;
        private const float _shakeStrength = 0.1f;

        [SerializeField] private Camera _camera;
        [SerializeField] private float _distance;
        [SerializeField] private float _sensitivity = 10f;

        public Camera Camera => _camera;

        private Transform _target;
        private bool _isShaking;

        private void OnDisable()
        {
            DOTween.Kill(this);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = (Vector2)position;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetEnable(bool value)
        {
            enabled = value;
        }

        private void Update()
        {
            if(_target == null)
                return;

            transform.position = Vector3.Lerp(transform.position, _target.position + _target.forward * _distance, Time.deltaTime * _sensitivity);
        }

        public void Shake()
        {
            if (_isShaking)
                return;

            _isShaking = true;

            var strengthVector = new Vector3(_shakeStrength, _shakeStrength, _shakeStrength);
            _camera.DOShakePosition(_shakeDuration, strengthVector, _shakeVibrato).OnComplete(OnComplete).SetId(this);
        }

        private void OnComplete()
        {
            DOTween.Kill(this);
            _isShaking = false;
        }
    }
}

