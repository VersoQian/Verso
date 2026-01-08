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
            // 初始化材质数组,增加空指针保护
            if (_renderers != null && _renderers.Length > 0)
            {
                _materials = new Material[_renderers.Length];
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                    {
                        _materials[i] = _renderers[i].material;
                    }
                }
            }
            else
            {
                _materials = new Material[0];
                Debug.LogWarning($"[{gameObject.name}] UnitView: _renderers数组为空或未配置!");
            }

            if (_collider != null)
            {
                _collider.radius = _radius;
            }
        }

        public void SetCollider(bool value)
        {
            _collider.enabled = value;
        }

        public float GetCurrentStateLength
        {
            get
            {
                if (_animator == null)
                {
                    Debug.LogWarning($"[{gameObject.name}] UnitView: _animator未配置");
                    return 0f;
                }
                return _animator.GetCurrentAnimatorStateInfo(0).length;
            }
        }

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
            if (_animator == null)
            {
                Debug.LogWarning($"[{gameObject.name}] UnitView: _animator未配置,无法播放动画 {animationState}");
                return;
            }

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
            if (_materials == null || _materials.Length == 0)
                return;

            foreach (var mat in _materials)
            {
                if (mat != null)
                {
                    mat.SetFloat("_BlinkAmount", value);
                }
            }
        }
    }
}

