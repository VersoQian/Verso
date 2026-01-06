using System;
using System.Collections.Generic;
using Core;
using Game.Cloth;
using Game.Config;
using Game.Core;
using Game.Effect;
using Game.Managers;
using Game.Modules;
using Game.Player.States;
using Game.Unit;
using Game.Weapon;
using Injection;
using UnityEngine;

namespace Game.Player
{
    public enum UnitAttributeType
    {
        Attack,
        Health,
        CollectDistance,
        HealthRecovery
    }

    public abstract class UnitModel : Observable
    {
        public float HealthNominal => _healthNominal;

        private float _healthNominal;

        protected Dictionary<UnitAttributeType, float> _attributes;

        public float GetAttribute(UnitAttributeType type)
        {
            return _attributes.TryGetValue(type, out float value) ? value : 0f;
        }

        public void SetAttribute(UnitAttributeType type, float value)
        {
            _attributes[type] = value;
        }

        public void UpdateNominalHealth(float value)
        {
            _healthNominal = value;
        }
    }

    public sealed class PlayerModel : UnitModel
    {
        public Sprite Icon;
        public string Label;
        public float WalkSpeed;
        public float RotateSpeed;
        public bool HasAllClothMeshes;
        public Mesh FullSkinnedMesh;
        public Mesh HelmetMesh;

        private readonly PlayerData _data;
        public Dictionary<ClothElementType, Mesh> ClothMeshMap;

        public int EquippedWeapon
        {
            get { return _data.EquippedWeapon; }
            set { _data.EquippedWeapon = value; }
        }
        public Dictionary<int, int> StoredWeapons => _data.StoredWeapons;
        public Dictionary<int, int> WeaponLevels => _data.WeaponLevels;

        public List<int> EquippedCloth => _data.EquippedCloth;
        public Dictionary<int, int> StoredCloth => _data.StoredCloth;
        public Dictionary<int, int> ClothLevels => _data.ClothLevels;

        public PlayerModel(GameConfig gameConfig)
        {
            _data = PlayerData.Load(gameConfig);
            _attributes = new Dictionary<UnitAttributeType, float>();

            var config = gameConfig.PlayerConfig;

            Label = config.Label;
            HasAllClothMeshes = config.HasAllClothMeshes;
            Icon = config.Icon;
            WalkSpeed = config.WalkSpeed;
            RotateSpeed = config.RotateSpeed;
            FullSkinnedMesh = config.FullSkinnedMesh;

            var attributeInfos = config.AttributeInfos;
            foreach (var attributeInfo in attributeInfos)
            {
                var type = attributeInfo.Type;
                var value = attributeInfo.Value;
                SetAttribute(type, value);
            }

            //attack
            if(StoredWeapons.ContainsKey(EquippedWeapon))
            {
                var index = StoredWeapons[EquippedWeapon];
                var weaponConfig = gameConfig.WeaponMap[index];
                var level = WeaponLevels[EquippedWeapon];
                var weaponModel = new WeaponModel(weaponConfig, EquippedWeapon, level);
                var value = weaponModel.Attack;
                SetAttribute(UnitAttributeType.Attack, value);
            }

            //health
            ClothMeshMap = new Dictionary<ClothElementType, Mesh>();
            var health = config.Health;
            foreach (var serial in EquippedCloth)
            {
                var configIndex = StoredCloth[serial];
                var clothConfig = gameConfig.ClothMap[configIndex];
                var level = ClothLevels[serial];
                var clothModel = new ClothModel(clothConfig, serial, level);
                ClothMeshMap[clothModel.ClothType] = clothModel.Mesh;
                health += (int)clothModel.Armor;
            }

            SetAttribute(UnitAttributeType.Health, health);
            UpdateNominalHealth(health);
        }

        public void Save()
        {
            _data.Save();
        }
    }

    public sealed class PlayerController : UnitController, IDisposable
    {
        public event Action ON_DAMAGE; //used in the DamageBorderHudMediator

        private const string _damageFormat = "-{0}";
        private const float _distance = 1.5f;
        private const float _speed = 15f;

        private readonly PlayerView _view;
        private readonly PlayerModel _model;

        public PlayerModel Model => _model;
        public new PlayerView View => _view;
        public override TeamIDType TeamID => TeamIDType.Player;

        private readonly StateManager<PlayerState> _stateManager;

        private float _spinDirection = 1f;
        public float SpinDirection => _spinDirection *= -1f;

        public PlayerController(PlayerView view, PlayerModel model, Context context) : base(view)
        {
            _view = view;
            _model = model;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            var gameConfig = context.Get<GameConfig>();
            var showLogs = gameConfig.LogEntityMap[EntityType.Player];
            _stateManager = new StateManager<PlayerState>();
            _stateManager.IsLogEnabled = showLogs;

            injector.Inject(_stateManager);

            _view.Model = model;
            Visibility(true);
            _view.SetCollider(true);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
            Visibility(false);
        }

        private void Visibility(bool value)
        {
            _view.gameObject.SetActive(value);
        }

        public void Idle()
        {
            _stateManager.SwitchToState(new PlayerIdleState());
        }

        public void IdleMenu()
        {
            _stateManager.SwitchToState(new PlayerIdleMenuState());
        }

        public void Walk()
        {
            _stateManager.SwitchToState(new PlayerWalkState());
        }

        private void Die()
        {
            _stateManager.SwitchToState(new PlayerDieState());
        }

        public void ChangeCloth()
        {
            _stateManager.SwitchToState(new PlayerChangeClothState());
        }

        public void Win()
        {
            _stateManager.SwitchToState(new PlayerWinState());
        }

        public void TryToDamage(int damage, Vector3 direction, GameManager gameManager)
        {
            var type = UnitAttributeType.Health;
            var health = (int) _model.GetAttribute(type);

            damage = Mathf.Min(damage, health);
            if (damage <= 0)
                return;

            health -= damage;
            _model.SetAttribute(type, health);
            _model.SetChanged();

            var blinkDuration = _distance / _speed * 0.5f;
            _view.Damage(blinkDuration);

            var colorType = UINotificationColorType.White;
            if (health <= 0)
            {
                colorType = UINotificationColorType.Red;
                Die();
            }

            var position = _view.AimPosition;
            gameManager.FireSpawnEffect(EffectType.Blood, position, direction);

            var info = string.Format(_damageFormat, damage);
            gameManager.FireSpawnNotificationPopUp(info, position, colorType);

            ON_DAMAGE?.Invoke();
        }
    }
}