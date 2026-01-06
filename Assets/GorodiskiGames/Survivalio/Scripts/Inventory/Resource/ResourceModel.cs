using System.Collections.Generic;
using Core;
using Game.Config;
using Game.Inventory;
using Game.Skill;
using UnityEngine;

namespace Game.Skill
{
    public abstract class SkillModel : Observable
    {

    }
}

namespace Game.Inventory
{
    public abstract class InventoryModel : SkillModel
    {
        public InventoryCategory Category;
        public string Label;
        public Sprite Icon;

        public InventoryModel(InventoryConfig config)
        {
            Category = config.Category;
            Label = config.Label;
            Icon = config.Icon;
        }
    }
}

namespace Game.Equipment
{
    public abstract class EquipmentModel : InventoryModel
    {
        public int Index;
        public int Serial;
        public int Level;
        public int Price;
        public EquipmentLevelConfig[] Levels;
        public readonly Dictionary<AttributeType, float> Attributes;
        public readonly Dictionary<AttributeType, float> AttributesNext;

        public bool IsEquipped { get; internal set; }
        public int LevelsCount => Levels.Length;
        public bool IsUpgradedMax => Level >= Levels.Length - 1;
        public abstract void SetLocalParameters();

        public EquipmentModel(EquipmentConfig config, int serial, int level) : base(config)
        {
            Category = config.Category;
            Index = config.Index;
            Serial = serial;
            Label = config.Label;
            Icon = config.Icon;
            Level = level;
            Levels = config.Levels;

            Attributes = new Dictionary<AttributeType, float>();
            AttributesNext = new Dictionary<AttributeType, float>();
        }

        public void UpdateStats()
        {
            if (Level >= LevelsCount)
                Level = Levels.Length - 1;

            var levelConfig = Levels[Level];
            foreach (var info in levelConfig.Attributes)
            {
                Attributes[info.Type] = info.Value;
                AttributesNext[info.Type] = info.Value;
            }

            SetLocalParameters();

            if (IsUpgradedMax)
                return;

            var levelNext = Level + 1;
            var levelConfigNext = Levels[levelNext];
            foreach (var info in levelConfigNext.Attributes)
            {
                AttributesNext[info.Type] = info.Value;
            }
            Price = levelConfigNext.Price;
        }
    }
}

namespace Game.Resource
{
    public class ResourceModel : InventoryModel
    {
        public ResourceItemType ResourceType;
        public int Amount;

        public ResourceModel(ResourceConfig config, int amount) : base(config)
        {
            ResourceType = config.ResourceType;
            Amount = amount;
        }
    }
}

