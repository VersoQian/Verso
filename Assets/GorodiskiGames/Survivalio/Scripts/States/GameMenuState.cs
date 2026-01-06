using Injection;
using Game.Managers;
using Game.UI.Hud;
using Game.Config;
using Game.UI;
using Game.Player;

namespace Game.States
{
    public sealed class GameMenuState : GameState
    {
        [Inject] private HudManager _hudManager;
        [Inject] private GameConfig _config;
        [Inject] private Context _context;
        [Inject] private GameView _gameView;
        [Inject] private AudioManager _audioManager;

        private MenuManager _menuManager;

        public override void Initialize()
        {
            _audioManager.FirePlayMusic(MusicType.Menu);

            _hudManager.ShowAdditional<SplashScreenHudMediator>(true);

            _menuManager = new MenuManager(_config);
            var model = new PlayerModel(_config);
            _menuManager.Player = new PlayerController(_gameView.PlayerView, model, _context);

            _context.Install(_menuManager);
            _context.ApplyInstall();

            _hudManager.ShowAdditional<MenuButtonsHudMediator>();
            _hudManager.ShowAdditional<StatsHudMediator>();
            _hudManager.ShowAdditional<ShopHudMediator>();
            _hudManager.ShowAdditional<EquipmentHudMediator>();
            _hudManager.ShowAdditional<BattleHudMediator>();
            _hudManager.ShowAdditional<PurchaseHudMediator>();

            _gameView.MenuLight.SetActive(true);
            _gameView.LevelLight.SetActive(false);

            _gameView.BarsHolder.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            _hudManager.HideAdditional<MenuButtonsHudMediator>();
            _hudManager.HideAdditional<StatsHudMediator>();
            _hudManager.HideAdditional<ShopHudMediator>();
            _hudManager.HideAdditional<EquipmentHudMediator>();
            _hudManager.HideAdditional<BattleHudMediator>();
            _hudManager.HideAdditional<PurchaseHudMediator>();

            _menuManager.Dispose();

            _context.Uninstall(_menuManager);
        }
    }
}

