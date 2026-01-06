using System.Collections.Generic;
using Game.Config;
using Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class EquipmentHudView : BaseHudWithModel<PlayerModel>
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GridLayoutGroup _layoutGroup;
        [SerializeField] private RectTransform _weaponCell;
        [SerializeField] private RectTransform _clothHelmetCell;
        [SerializeField] private RectTransform _clothVestCell;
        [SerializeField] private RectTransform _clothGlovesCell;
        [SerializeField] private RectTransform _clothUniformCell;
        [SerializeField] private RectTransform _clothShoesCell;
        [SerializeField] private AttributeSlotView _attackAttribute;
        [SerializeField] private AttributeSlotView _healthAttribute;

        public RawImage RawImage => _rawImage;
        public RectTransform Content => _content;
        public GridLayoutGroup LayoutGroup => _layoutGroup;
        public RectTransform WeaponCell => _weaponCell;
        public Dictionary<ClothElementType, RectTransform> ClothCellsMap;

        public EquipmentHudView()
        {
            ClothCellsMap = new Dictionary<ClothElementType, RectTransform>();
        }

        protected override void OnEnable()
        {
            // 映射到新的装备类型
            ClothCellsMap[ClothElementType.Headgear] = _clothHelmetCell;  // Helmet → Headgear
            ClothCellsMap[ClothElementType.Top] = _clothVestCell;         // Vest → Top
            ClothCellsMap[ClothElementType.Gloves] = _clothGlovesCell;    // Gloves (不变)
            ClothCellsMap[ClothElementType.Bottom] = _clothUniformCell;   // Uniform → Bottom
            ClothCellsMap[ClothElementType.Shoes] = _clothShoesCell;      // Shoes (不变)
        }

        protected override void OnDisable()
        {
            ClothCellsMap.Clear();
        }

        protected override void OnModelChanged(PlayerModel model)
        {
            _attackAttribute.SetValue(model.GetAttribute(UnitAttributeType.Attack));
            _healthAttribute.SetValue(model.GetAttribute(UnitAttributeType.Health));
        }
    }
}

