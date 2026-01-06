using UnityEngine;

namespace Game.Effect
{
    public sealed class ExplosionController
    {
        private float _radius;
        private float _damage;

        private float _timerExplosion;
        private float _timerTotal;

        private float _radiusProgress;
        private float _damageProgress;

        public float Radius => _radius;
        public float RadiusProgress => _radiusProgress;
        public float DamageProgress => _damageProgress;

        private readonly ExplosionView _view;
        public ExplosionView View => _view;

        public ExplosionController(ExplosionView view, Vector3 initPosition, float damage, float radius)
        {
            _view = view;

            _view.Position = initPosition;

            _radius = radius;
            _damage = damage;

            _view.SetExplosionEffect(_radius);
            _view.Play();
        }

        public bool Proceed()
        {
            _timerExplosion += Time.deltaTime / _view.SpheresLifetime;

            _radiusProgress = Mathf.Lerp(0f, _radius, _timerExplosion);
            _damageProgress = Mathf.Lerp(_damage, 0f, _timerExplosion);

            _timerTotal += Time.deltaTime;
            return _timerTotal < _view.ExplosionLifetime;
        }
    }
}

