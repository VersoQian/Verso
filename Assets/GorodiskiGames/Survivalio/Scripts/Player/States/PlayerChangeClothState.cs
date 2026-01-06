using DG.Tweening;
using UnityEngine;

namespace Game.Player.States
{
    public sealed class PlayerChangeClothState : PlayerState
    {
        [SerializeField] private float _duration = 0.25f;

        public override void Initialize()
        {
            DoJump();
        }

        public override void Dispose()
        {
            KillTween();
        }

        private void KillTween()
        {
            DOTween.Kill(this);
        }

        private void DoJump()
        {
            var direction = _player.SpinDirection;
            var sequence = DOTween.Sequence();
            sequence.Join(_player.View.RotateNode.DORotate(new Vector3(0, 360f * direction, 0), _duration, RotateMode.WorldAxisAdd).SetEase(Ease.OutCubic));
            sequence.OnComplete(OnJumpComplete).SetId(this);
        }

        private void OnJumpComplete()
        {
            _player.IdleMenu();
        }    
    }
}

