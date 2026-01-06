using System;
using System.Collections.Generic;
using Game.Config;
using Game.Managers;
using Game.Modules;
using Game.Unit;
using Injection;
using UnityEngine;

namespace Game.Bullet
{
    public sealed class BulletModel
    {
        public BulletControllerType Type;
        public int Index;
        public float MoveSpeed;
        public float LifeTime;
        public float Range;
        public int Damage;
        public int Number, Numbers; //used for RotateAround BulletControllerType to calculate base angle between bullets
        public float OrbitRadius;
        public TeamIDType TeamID;

        public BulletModel(BulletControllerType type, int index, int number, int numbers, float moveSpeed, float lifeTime, float range, float damage, float orbitRadius, TeamIDType teamID)
        {
            Type = type;
            Index = index;
            MoveSpeed = moveSpeed;
            LifeTime = lifeTime;
            Range = range;
            Number = number;
            Numbers = numbers;
            Damage = (int)damage;
            OrbitRadius = orbitRadius;
            TeamID = teamID;
        }
    }

    public abstract class BulletController : IDisposable
    {
        public event Action<BulletController> ON_COLLIDE_ENEMY;
        public event Action<BulletController> ON_LIFETIME_END;

        private const float _trailTimeMin = 2f;
        private const float _trailTimeMax = 4f;
        private const float _sameUnitAttackRate = 0.5f;

        [Inject] protected LevelManager _levelManager;

        protected readonly BulletView _view;
        protected readonly BulletModel _model;
        protected readonly UnitController _unit;
        protected bool _isPause;

        private readonly Dictionary<UnitController, float> _unitsMap;

        public BulletView View => _view;
        public BulletModel Model => _model;

        private Vector3 _initPosition;
        private Vector3 _aimPosition;

        public Vector3 InitPosition => _initPosition;
        public Vector3 AimPosition => _aimPosition;

        public BulletController(BulletView view, BulletModel model, UnitController unit)
        {
            _view = view;
            _model = model;
            _unit = unit;
            _unitsMap = new Dictionary<UnitController, float>();
        }

        public virtual void Dispose()
        {
            ClearTrail();
            _levelManager.ON_PAUSE -= OnPause;
        }

        public void Init(Vector3 initPosition, Vector3 aimPosition)
        {
            _view.Trail.time = 0f;

            _initPosition = initPosition;
            _aimPosition = aimPosition;

            SetPosition();
            Init();
            SetTrailTime();

            _unit.ON_DIED += OnUnitDied;
            _levelManager.ON_PAUSE += OnPause;
        }

        private void SetPosition()
        {
            _view.Position = _initPosition;
        }

        public virtual void SetLookRotation()
        {
            var direction = (_aimPosition - _initPosition).normalized;
            direction.y = 0f;
            var rotation = Quaternion.LookRotation(direction);
            _view.Rotation = rotation;
        }

        private void OnPause(bool isPause)
        {
            _isPause = isPause;

            if(_isPause)
                ClearTrail();
            else
                SetTrailTime();
        }

        public bool CanCollideUnit(UnitController unit, float currentTime)
        {
            if (_unitsMap.TryGetValue(unit, out float time))
            {
                var elapsedTime = currentTime - time;
                if (elapsedTime < _sameUnitAttackRate)
                    return false;
            }

            return true;
        }

        public void AddUnit(UnitController unit, float time)
        {
            _unitsMap[unit] = time;
        }

        public virtual void OnUnitDied()
        {
            _unit.ON_DIED -= OnUnitDied;
        }

        protected abstract void Init();

        public virtual void Proceed()
        {
            if (_isPause)
                return;
        }

        public void SetTrailTime()
        {
            _view.Trail.time = UnityEngine.Random.Range(_trailTimeMin, _trailTimeMax) / _model.MoveSpeed;
        }

        public void ClearTrail()
        {
            _view.Trail.Clear();
        }

        public bool TimeIsUp()
        {
            return _model.LifeTime < 0f;
        }

        public void CalculateLifeTime()
        {
            _model.LifeTime -= Time.deltaTime;
        }

        public virtual void FireCollideEnemy()
        {
            ON_COLLIDE_ENEMY?.Invoke(this);
        }

        public virtual void FireLifetimeEnd()
        {
            ON_LIFETIME_END?.Invoke(this);
        }
    }
}

