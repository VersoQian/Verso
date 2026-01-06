using Game.Managers;
using Injection;
using Game.UI.Hud;
using UnityEngine;
using Game.Core;
using Game.Player;
using Game.Config;
using Game.Level;
using Game.Modules;

namespace Game.States
{
    public sealed class GamePlayState : GameModulesState
	{
		private const float _rotationY = 180f;
		private const float _showHudDelay = 2f;

		[Inject] private Context _context;
		[Inject] private HudManager _hudManager;
		[Inject] private GameConfig _config;
		[Inject] private Timer _timer;
		[Inject] private AudioManager _audioManager;

		private GameManager _gameManager;
		private LevelManager _levelManager;
		private LevelView _levelView;

		private float _showHudTime;
		private bool _isWin;

		public override void Initialize()
        {
			_audioManager.FirePlayMusic(MusicType.GamePlay);

			_gameManager = new GameManager(_config);
			
			var level = _gameManager.Model.Level;
			var levelConfig = _config.LevelConfigs[level];
			var durationMax = _gameManager.Model.LevelDurationMax;
            _levelManager = new LevelManager(levelConfig, level, durationMax);

			var prefab = _levelManager.Model.Prefab;
			_levelView = GameObject.Instantiate(prefab).GetComponent<LevelView>();

			_context.Install(_gameManager);
			_context.Install(_levelManager);
			_context.Install(_levelView);
			_context.ApplyInstall();

			var model = new PlayerModel(_config);
			var player = new PlayerController(_gameView.PlayerView, model, _context);
			player.View.Position = Vector3.zero;
			player.View.Rotation = Quaternion.Euler(0f, _rotationY, 0f);
			player.Idle();
			_gameManager.Player = player;

			_levelManager.Initialize();

			_gameView.Camera.SetPosition(_gameManager.Player.View.Position);
			_gameView.Camera.SetTarget(_gameManager.Player.View.RotateNode);
			_gameView.Camera.SetEnable(true);

			InitLevelModules();

			_hudManager.ShowAdditional<GamePlayHudMediator>();
			_hudManager.ShowAdditional<DamageBorderHudMediator>();

			_gameView.Joystick.gameObject.SetActive(true);
			_gameView.GameInput.gameObject.SetActive(true);

			_gameView.MenuLight.SetActive(false);
			_gameView.LevelLight.SetActive(true);

			_levelManager.ON_LEVEL_END += OnLevelEnd;
			_gameView.Joystick.ON_INPUT += OnInput;
			_gameManager.Player.View.ON_FOOT_ON_GROUND += OnFootOnGraund;
		}

		public override void Dispose()
        {
			_gameManager.Player.View.ON_FOOT_ON_GROUND -= OnFootOnGraund;
			_levelManager.ON_LEVEL_END -= OnLevelEnd;
			_gameView.Joystick.ON_INPUT -= OnInput;

			_hudManager.HideAdditional<GamePlayHudMediator>();
			_hudManager.HideAdditional<DamageBorderHudMediator>();

			_gameView.Joystick.gameObject.SetActive(false);
			_gameView.GameInput.gameObject.SetActive(false);

			_gameView.Camera.SetEnable(false);
			_gameView.Camera.SetTarget(null);

			_context.Uninstall(_gameManager);
			_context.Uninstall(_levelManager);
			_context.Uninstall(_levelView);

			DisposeModules();

			_gameManager.Dispose();
			_levelManager.Dispose();

			GameObject.Destroy(_levelView.gameObject);
		}

        private void InitLevelModules()
		{
			AddModule<GroundsModule>();
            AddModule<WeaponsModule, WeaponsModuleView>(_gameView);
			AddModule<AbilitiesModule>();
			AddModule<EnemiesModule, EnemiesModuleView>(_levelView);
			AddModule<EffectsModule, EffectsModuleView>(_gameView);
			AddModule<CollectiblesModule, CollectiblesModuleView>(_gameView);
            AddModule<SkillsModule>();
			AddModule<UINotificationModule, UINotificationModuleView>(_gameView);
		}

		private void OnLevelEnd(bool isWin)
		{
			_levelManager.ON_LEVEL_END -= OnLevelEnd;

			_isWin = isWin;
			if (_isWin)
				_gameManager.Player.Win();

			var elapsed = _levelManager.Model.Elapsed;
			var durationMax = _levelManager.Model.DurationMax;
			if (elapsed > durationMax)
			{
				_levelManager.Model.IsNewRecord = true;
				_levelManager.Model.DurationMax = elapsed;

				_gameManager.Model.LevelDurationMax = elapsed;
				_gameManager.Model.Save();
			}

			_showHudTime = _timer.Time + _showHudDelay;
			_timer.TICK += OnTickDelayShowHud;
		}

		private void OnTickDelayShowHud()
		{
			if(_timer.Time < _showHudTime)
				return;

			_timer.TICK -= OnTickDelayShowHud;

			if (_isWin)
				_hudManager.ShowSingle<VictoryHudMediator>();
			else
				_hudManager.ShowSingle<DefeatHudMediator>();
		}

		private void OnInput()
		{
			_gameView.Joystick.ON_INPUT -= OnInput;
			_gameView.BarsHolder.gameObject.SetActive(true);
        }

		private void OnFootOnGraund()
		{
			_audioManager.FireFootOnGround();
		}
	}
}

