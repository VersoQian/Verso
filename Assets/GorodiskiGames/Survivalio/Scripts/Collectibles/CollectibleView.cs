using UnityEngine;

namespace Game.Collectible
{
    public sealed class CollectibleView : MonoBehaviour
    {
        [SerializeField] private float _scaleMin = 1f;
        [SerializeField] private float _scaleMax = 1f;

        [SerializeField] private float _localHeightMin = 0f;
        [SerializeField] private float _localHeightMax = 0f;

        [SerializeField] private Transform _localTransform;
        [SerializeField] private float _radius = 0.5f;

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

        public Vector3 LocalScale
        {
            get { return _localTransform.localScale; }
            set { _localTransform.localScale = value; }
        }

        public Vector3 LocalPosition
        {
            get { return _localTransform.localPosition; }
            set { _localTransform.localPosition = value; }
        }

        public void Initialize(Vector3 position)
        {
            Position = position;
            LocalScale = Vector3.one * Random.Range(_scaleMin, _scaleMax);
            LocalPosition = Vector3.up * Random.Range(_localHeightMin, _localHeightMax);
            Rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
    }
}

