using UnityEngine;

namespace Game.Effect
{
    public sealed class ExplosionView : EffectView
    {
        private const float _circlesLimitHeight = 0.5f;
        private const float _xzKoef = 0.65f;
        private const float _yKoef = 1.5f;

        private const float _circleSizeKoef = 1.6f;
        private const float _circleInsidePercentage = 0.9f;
        private const float _debrisShapeRadiusPercentage = 0.33f;
        private const float _explosionLifetimePercentage = 0.8f;

        [SerializeField] private float _radius = 5f;
        [SerializeField] private float _spheresLifetime;
        [SerializeField] private float _explosionLifetime;

        [SerializeField] private ParticleSystem _base;
        [SerializeField] private ParticleSystem[] _spheres;

        [SerializeField] private ParticleSystem _circleInside;
        [SerializeField] private ParticleSystem _circle;

        [SerializeField] private ParticleSystem _debris;
        [SerializeField] private ParticleSystem _debrisGround;

        public float Radius => _radius;
        public float SpheresLifetime => _spheresLifetime;
        public float ExplosionLifetime => _explosionLifetime;

        public float XZKoef => _xzKoef;
        public float YKoef => _yKoef;

        public void SetExplosionEffect(float radius)
        {
            float size = radius * 2f;
            foreach (var effect in _spheres)
            {
                var effectMain = effect.main;
                effectMain.startSize = size;
                effectMain.startLifetime = _spheresLifetime;
            }

            float circleStartSize = size * _circleSizeKoef;

            bool isGrounded = Position.y < _circlesLimitHeight;

            _circle.gameObject.SetActive(isGrounded);
            _circleInside.gameObject.SetActive(isGrounded);

            var circleMain = _circle.main;
            circleMain.startSize = circleStartSize;
            circleMain.startLifetime = _explosionLifetime * _explosionLifetimePercentage;

            var circleInsideMain = _circleInside.main;
            circleInsideMain.startSize = circleStartSize * _circleInsidePercentage;
            circleInsideMain.startLifetime = _explosionLifetime * _explosionLifetimePercentage;

            var debrisShape = _debris.shape;
            debrisShape.radius = radius * _debrisShapeRadiusPercentage;

            _debrisGround.gameObject.SetActive(isGrounded);

            var debrisGroundShape = _debrisGround.shape;
            debrisGroundShape.radius = radius;

            var debrisGroundMain = _debrisGround.main;
            debrisGroundMain.startLifetime = _explosionLifetime;
        }

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
    }
}

