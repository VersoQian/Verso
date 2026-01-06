using System;
using UnityEngine;

namespace Game.Effect
{
    public enum EffectType
    {
        Blood,
        Explosion
    }

    public class EffectView : MonoBehaviour
    {
        public event Action<EffectView> ON_REMOVE;

        private float _duration;
        private float _durationNominal;
        private float _emissionRateNominal;
        private EffectType _type;

        public float Timer { get; internal set; }
        public float FadingTime { get; internal set; }

        public float EmissionRateNominal => _emissionRateNominal;
        public EffectType Type => _type;

        private ParticleSystem _effect;

        private void Awake()
        {
            _effect = GetComponent<ParticleSystem>();
            _emissionRateNominal = _effect.emission.rateOverTime.constant;

            var partMain = _effect.main;
            _durationNominal = partMain.duration;
        }

        public void Init(EffectType type, Vector3 position, Vector3 direction)
        {
            _duration = 0f;
            _type = type;

            var rotation = Quaternion.LookRotation(direction);
            transform.SetPositionAndRotation(position, rotation);
        }

        public void Proceed()
        {
            _duration += Time.deltaTime;

            if (_duration < _durationNominal)
                return;

            _effect.Stop();

            ON_REMOVE?.Invoke(this);
        }

        public float EmissionRate
        {
            get { return _effect.emission.rateOverTime.constant; }
            set
            {
                var rateOverTime = _effect.emission.rateOverTime;
                rateOverTime.constant = value;
            }
        }

        public void Play()
        {
            if (_effect.isPlaying)
                return;

            _effect.Play();
        }

        public void Stop()
        {
            _effect.Stop();
        }
    }
}

