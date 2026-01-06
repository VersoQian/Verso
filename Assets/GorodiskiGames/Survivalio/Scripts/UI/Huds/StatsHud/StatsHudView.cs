using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class StatsHudView : BaseHud
    {
        [SerializeField] private PlayerStatsHudView _playerStatsHudView;
        [SerializeField] private GameStatsHudView _gameStatsHudView;

        public PlayerStatsHudView PlayerStatsHudView => _playerStatsHudView;
        public GameStatsHudView GameStatsHudView => _gameStatsHudView;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}
