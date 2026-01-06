using System;
using Game.Config;
using Game.Core;
using Game.Equipment;
using Game.Weapon.States;
using Injection;

namespace Game.Weapon
{
    public sealed class WeaponModel : EquipmentModel
    {
        public float Range;
        public float BulletMoveSpeed;
        public float BulletLifeTime;
        public BulletControllerType BulletType;
        public int BulletIndex;
        public int Count;
        public float FireRate;
        public float MultipleBulletsDelay;
        public float OrbitRadius;

        public float Attack => 1f / FireRateNominal * Damage;
        public float FireRateNominal => Attributes[AttributeType.FireRate];
        public int Damage => (int)Attributes[AttributeType.Damage];

        public WeaponModel(WeaponConfig config, int serial, int level) : base(config, serial, level)
        {
            Range = config.Range;
            BulletType = config.BulletType;
            BulletIndex = config.BulletIndex;
            BulletMoveSpeed = config.BulletSpeed;
            BulletLifeTime = config.BulletLifeTime;
            MultipleBulletsDelay = config.MultipleBulletsDelay;
            Count = config.BulletsCount;
            OrbitRadius = config.OrbitRadius;

            UpdateStats();
        }

        public override void SetLocalParameters()
        {
            FireRate = Attributes[AttributeType.FireRate];
        }
    }

    public sealed class WeaponController : IDisposable
    {
        public event Action<WeaponController> ON_READY;

        public WeaponModel Model => _model;

        private readonly StateManager<WeaponState> _stateManager;
        private readonly WeaponModel _model;

        public WeaponController(WeaponModel model, Context context)
        {
            _model = model;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            var gameConfig = context.Get<GameConfig>();
            var showLogs = gameConfig.LogEntityMap[EntityType.Weapon];
            _stateManager = new StateManager<WeaponState>();
            _stateManager.IsLogEnabled = showLogs;

            injector.Inject(_stateManager);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        public void Ready()
        {
            _stateManager.SwitchToState(new WeaponReadyState());
        }

        public void Reload(float spawnBulletsTime)
        {
            _stateManager.SwitchToState(new WeaponReloadState(spawnBulletsTime));
        }

        public void FireReady()
        {
            ON_READY?.Invoke(this);
        }
    }
}

