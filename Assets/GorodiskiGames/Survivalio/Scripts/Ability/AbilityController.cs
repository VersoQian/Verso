using System;
using Game.Config;
using Game.Managers;
using Injection;
using UnityEngine;
using Game.Skill;

namespace Game.Ability
{
    public sealed class AbilityModel : SkillModel
    {
        public AbilityType Type;
        public int Count;
        public string Label;
        public Sprite Icon;
        public UnitAttributeInfo AttributeInfo;

        public AbilityModel(AbilityConfig config)
        {
            Type = config.Type;
            Count = config.Count;
            Label = config.Label;
            Icon = config.Icon;
            AttributeInfo = config.AttributeInfo;
        }
    }

    public class AbilityController : IDisposable
    {
        private const int _deltaCount = 1;

        [Inject] protected LevelManager _levelManager;
        [Inject] protected GameManager _gameManager;

        public AbilityModel Model => _model;

        private readonly AbilityModel _model;
        protected bool _isPause;

        public AbilityController(AbilityModel model)
        {
            _model = model;
        }

        public void Init()
        {
            _levelManager.ON_PAUSE += OnPause;

            var value = _model.AttributeInfo.Value;
            var addValue = _model.Count * value;

            UpdatePlayerAttributes(addValue);
        }

        public void Dispose()
        {
            _levelManager.ON_PAUSE -= OnPause;
        }

        public virtual void Proceed()
        {
            if (_isPause)
                return;
        }

        public void Update()
        {
            var сount = _model.Count;
            сount = Mathf.Min(сount + _deltaCount, GameConstants.BulletsMax);
            _model.Count = сount;

            var value = _model.AttributeInfo.Value;
            var addValue = _deltaCount * value;

            UpdatePlayerAttributes(addValue);
        }

        private void UpdatePlayerAttributes(float addValue)
        {
            var type = _model.AttributeInfo.Type;
            var value = _gameManager.Player.Model.GetAttribute(type);

            value += addValue;
            _gameManager.Player.Model.SetAttribute(type, value);

            _levelManager.FireAttributeUpdated(type);
        }

        private void OnPause(bool isPause)
        {
            _isPause = isPause;
        }
    }
}

