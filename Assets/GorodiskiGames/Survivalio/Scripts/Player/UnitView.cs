using System.Collections;
using Core;
using Game.Player;
using UnityEngine;

namespace Game.Unit
{
    public enum AnimatorStateType
    {
        Walk,
        Jump,
        Die,
        Idle
    }

    public enum AnimatorParameterType
    {
        Speed
    }

    public abstract class UnitView : BehaviourWithModel<UnitModel>
    {
        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _rotateNode;
        [SerializeField] private Transform _bulletNode;
        [SerializeField] private Transform _aimNode;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private float _radius = 0.5f;

        public Transform RotateNode => _rotateNode;
        public Transform BulletNode => _bulletNode;
        public float Radius => _radius;

        private Material[] _materials;

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Vector3 AimPosition
        {
            get { return _aimNode.position; }
            set { _aimNode.position = value; }
        }

        public Quaternion Rotation
        {
            get { return _rotateNode.rotation; }
            set { _rotateNode.rotation = value; }
        }

        private void Awake()
        {
            _materials = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _materials[i] = _renderers[i].material;
            }

            _collider.radius = _radius;
        }

        public void SetCollider(bool value)
        {
            _collider.enabled = value;
        }

        public float GetCurrentStateLength => _animator.GetCurrentAnimatorStateInfo(0).length;

        public void Idle()
        {
            PlayAnimation(AnimatorStateType.Idle, Random.Range(0, 1f));
        }

        public void Walk()
        {
            PlayAnimation(AnimatorStateType.Walk, float.NegativeInfinity);
        }

        public void Jump()
        {
            PlayAnimation(AnimatorStateType.Jump, float.NegativeInfinity);
        }

        public void Die()
        {
            PlayAnimation(AnimatorStateType.Die, float.NegativeInfinity);
        }

        private void PlayAnimation(AnimatorStateType animationState, float timeValue)
        {
            var nameHash = Animator.StringToHash(animationState.ToString());
            _animator.PlayInFixedTime(nameHash, 0, timeValue);

            _animator.Update(0);
        }

        public void Damage(float blinkDuration)
        {
            StartCoroutine(BlinkCoroutine(blinkDuration));
        }

        private IEnumerator BlinkCoroutine(float blinkDuration)
        {
            float halfDuration = blinkDuration * 0.5f;
            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float value = Mathf.Clamp01(t / halfDuration);
                SetBlinkAmount(value);
                yield return null;
            }

            SetBlinkAmount(1f);
            yield return null;

            for (float t = 0; t < halfDuration; t += Time.deltaTime)
            {
                float value = Mathf.Clamp01(1f - (t / halfDuration));
                SetBlinkAmount(value);
                yield return null;
            }

            SetBlinkAmount(0f);
        }

        private void SetBlinkAmount(float value)
        {
            foreach (var mat in _materials)
            {
                mat.SetFloat("_BlinkAmount", value);
            }
        }
    }
}

