using System;
using Core;
using Game.Config;
using Game.Core;
using Game.Effect;
using Game.Enemy.States;
using Game.Managers;
using Game.Modules;
using Game.Unit;
using Injection;
using UnityEngine;

namespace Game.Unit
{
    public abstract class UnitController
    {
        public event Action ON_DIED; //used in the BulletController

        public abstract TeamIDType TeamID { get; }

        private readonly UnitView _view;
        public UnitView View => _view;

        public UnitController(UnitView view)
        {
            _view = view;
        }

        public void FireDied()
        {
            ON_DIED?.Invoke();
        }
    }
}

namespace Game.Enemy
{
    public sealed class EnemyModel : Observable
    {
        public Sprite Icon;
        public float Speed;
        public int Damage;
        public int Health;
        public int HealthNominal;
        public ResourceInfo[] Reward;
        public UINotificationColorType UINotificationColor;

        public EnemyModel(EnemyConfig config)
        {
            Speed = config.WalkSpeed;
            Damage = config.Damage;
            Reward = config.Reward;
            Health = config.Health;
            HealthNominal = config.Health;
            UINotificationColor = config.UINotificationColor;
        }
    }

    public sealed class EnemyController : UnitController, IDisposable
    {
        private const string _damageFormat = "-{0}";

        public EnemyModel Model => _model;
        public int Index { get; internal set; }
        public override TeamIDType TeamID => TeamIDType.Enemy;
        public bool IsBoss => _isBoss;

        private readonly EnemyModel _model;
        private readonly EnemyView _view;  
        private readonly bool _isBoss;
        private readonly StateManager<EnemyState> _stateManager;
        private readonly GameManager _gameManager;

        public EnemyController(EnemyView view, Context context, bool isBoss) : base(view)
        {
            _model = new EnemyModel(view.Config);
            _view = view;
            _isBoss = isBoss;
            _gameManager = context.Get<GameManager>();

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<EnemyState>();
            _stateManager.IsLogEnabled = false;

            injector.Inject(_stateManager);

            _view.PlayEffect(true);
            _view.SetCollider(true);
        }
        
        public void Dispose()
        {
            _stateManager.Dispose();
            _view.PlayEffect(false);
        }

        public void FollowPlayer()
        {
            // 暂时使用改进的射线避障(性能更好)
            // TODO: 等A*优化后再切换回去
            _stateManager.SwitchToState(new EnemyImprovedFollowState());

            /* A*版本(性能问题暂时禁用)
            if (_isBoss)
            {
                _stateManager.SwitchToState(new BossSmartFollowState());
            }
            else
            {
                _stateManager.SwitchToState(new EnemySmartFollowState());
            }
            */
        }

        public void Die()
        {
            _stateManager.SwitchToState(new EnemyDieState());
        }

        public void Damage()
        {
            _stateManager.SwitchToState(new EnemyDamageState());
        }

        public void Idle()
        {
            _stateManager.SwitchToState(new EnemyIdleState());
        }

        public void TryToDamage(int damage, Vector3 direction)
        {
            damage = Mathf.Min(damage, _model.Health);

            if (damage <= 0)
                return;
            
            _model.Health -= damage;
            _model.SetChanged();

            var colorType = _model.UINotificationColor;
            if (_model.Health <= 0)
            {
                colorType = UINotificationColorType.Red;
                Die();
            }
            else
            {
                Damage();
            }

            var position = _view.AimPosition;
            _gameManager.FireSpawnEffect(EffectType.Blood, position, direction);

            var info = string.Format(_damageFormat, damage);
            _gameManager.FireSpawnNotificationPopUp(info, position, colorType);
        }
    }
}

