using Game.Core;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.Player.States
{
    public abstract class PlayerState : State
    {
        private const float _shiftY = -1.25f;

        [Inject] protected PlayerController _player;
        [Inject] protected GameView _gameView;

        protected void HandleBarsPosition()
        {
            var playerPosition = _player.View.Position + Vector3.up * _shiftY;
            var screenPosition = _gameView.Camera.Camera.WorldToScreenPoint(playerPosition);
            screenPosition.z = 0f;
            _gameView.BarsHolder.position = screenPosition;
        }
    }
}