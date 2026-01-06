using Game.Config;
using Game.Player;
using Game.Unit;
using UnityEngine;

namespace Game.Enemy
{
    public sealed class EnemyView : UnitView
    {
        [SerializeField] private EnemyConfig _config;
        [SerializeField] private ParticleSystem _effect;

        public EnemyConfig Config => _config;

        protected override void OnModelChanged(UnitModel model)
        {

        }

        public void PlayEffect(bool isPlay)
        {
            if(_effect == null)
                return;

            if(isPlay)
                _effect.Play();
            else
                _effect.Stop();
        }
    }
}

