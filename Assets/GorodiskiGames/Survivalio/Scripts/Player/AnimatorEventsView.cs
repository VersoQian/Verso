using UnityEngine;

namespace Game.Player
{
    public sealed class AnimatorEventsView : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerView;

        public void FireFootOnGround()
        {
            if(_playerView == null)
                return;

            _playerView.FireFootOnGround();
        }
    }
}

