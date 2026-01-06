using System;
using Game.Config;
using Game.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{
    public sealed class PlayerView : UnitView
    {
        public event Action ON_FOOT_ON_GROUND;

        [SerializeField] private Image _health;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private SkinnedMeshRenderer _full;
        [SerializeField] private SkinnedMeshRenderer _head;

        // 新装备系统 - 10个部位
        [SerializeField] private SkinnedMeshRenderer _top;        // 上衣
        [SerializeField] private SkinnedMeshRenderer _bottom;     // 裤子
        [SerializeField] private SkinnedMeshRenderer _headgear;   // 头部装备
        [SerializeField] private SkinnedMeshRenderer _shoes;      // 鞋子
        [SerializeField] private SkinnedMeshRenderer _gloves;     // 手套
        [SerializeField] private SkinnedMeshRenderer _eyewear;    // 眼镜
        [SerializeField] private SkinnedMeshRenderer _body;       // 身体部件
        [SerializeField] private SkinnedMeshRenderer _bag;        // 背包
        [SerializeField] private SkinnedMeshRenderer _face;       // 脸部变体
        [SerializeField] private SkinnedMeshRenderer _hair;       // 发型

        protected override void OnModelChanged(UnitModel model)
        {
            var playerModel = model as PlayerModel;

            var health = playerModel.GetAttribute(UnitAttributeType.Health);
            _health.fillAmount = (float)health / playerModel.HealthNominal;
            _healthText.text = health.ToString();

            bool hasAllClothMeshes = playerModel.HasAllClothMeshes;
            if(!hasAllClothMeshes)
            {
                _full.sharedMesh = playerModel.FullSkinnedMesh;
                _head.sharedMesh = null;
                return;
            }

            // 更新所有装备部位的Mesh（添加空检查避免崩溃）
            if (_top != null)
                _top.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Top)
                    ? playerModel.ClothMeshMap[ClothElementType.Top] : null;

            if (_bottom != null)
                _bottom.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Bottom)
                    ? playerModel.ClothMeshMap[ClothElementType.Bottom] : null;

            if (_headgear != null)
                _headgear.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Headgear)
                    ? playerModel.ClothMeshMap[ClothElementType.Headgear] : null;

            if (_shoes != null)
                _shoes.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Shoes)
                    ? playerModel.ClothMeshMap[ClothElementType.Shoes] : null;

            if (_gloves != null)
                _gloves.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Gloves)
                    ? playerModel.ClothMeshMap[ClothElementType.Gloves] : null;

            if (_eyewear != null)
                _eyewear.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Eyewear)
                    ? playerModel.ClothMeshMap[ClothElementType.Eyewear] : null;

            if (_body != null)
                _body.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Body)
                    ? playerModel.ClothMeshMap[ClothElementType.Body] : null;

            if (_bag != null)
                _bag.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Bag)
                    ? playerModel.ClothMeshMap[ClothElementType.Bag] : null;

            if (_face != null)
                _face.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Face)
                    ? playerModel.ClothMeshMap[ClothElementType.Face] : null;

            if (_hair != null)
                _hair.sharedMesh = playerModel.ClothMeshMap.ContainsKey(ClothElementType.Hair)
                    ? playerModel.ClothMeshMap[ClothElementType.Hair] : null;
        }

        public void FireFootOnGround()
        {
            ON_FOOT_ON_GROUND?.Invoke();
        }
    }
}

