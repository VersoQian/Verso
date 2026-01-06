using UnityEngine;

namespace Game.Bullet
{
    public sealed class BulletView : MonoBehaviour
    {
        [SerializeField] private Transform _localTransform;
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private float _radius = 0.5f;

        public Transform LocalTransform => _localTransform;
        public TrailRenderer Trail => _trail;
        public float Radius => _radius;

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }
    }
}

