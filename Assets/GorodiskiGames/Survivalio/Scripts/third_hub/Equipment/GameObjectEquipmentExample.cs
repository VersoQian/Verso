using UnityEngine;

namespace Game.ThirdHub.Equipment
{
    /// <summary>
    /// GameObject 装备系统使用示例
    /// 演示如何使用 GameObjectEquipmentManager 进行换装
    /// </summary>
    public class GameObjectEquipmentExample : MonoBehaviour
    {
        [Header("装备管理器引用")]
        [SerializeField] private GameObjectEquipmentManager _equipmentManager;

        [Header("测试参数")]
        [SerializeField] private GameObjectEquipmentType _testEquipmentType = GameObjectEquipmentType.Bag;
        [SerializeField] private int _testEquipmentIndex = 0;

        private void Start()
        {
            if (_equipmentManager == null)
            {
                _equipmentManager = GetComponent<GameObjectEquipmentManager>();
            }

            // 订阅装备变化事件
            if (_equipmentManager != null)
            {
                _equipmentManager.OnEquipmentChanged += OnEquipmentChanged;
            }
        }

        private void OnDestroy()
        {
            if (_equipmentManager != null)
            {
                _equipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
            }
        }

        private void OnEquipmentChanged(GameObjectEquipmentType equipmentType, int index)
        {
            Debug.Log($"装备变化：{equipmentType} -> 索引 {index}");
        }

        /// <summary>
        /// 测试：装备指定装备
        /// </summary>
        [ContextMenu("测试：装备")]
        public void TestEquip()
        {
            if (_equipmentManager != null)
            {
                _equipmentManager.Equip(_testEquipmentType, _testEquipmentIndex);
            }
        }

        /// <summary>
        /// 测试：脱下装备
        /// </summary>
        [ContextMenu("测试：脱下")]
        public void TestUnequip()
        {
            if (_equipmentManager != null)
            {
                _equipmentManager.Unequip(_testEquipmentType);
            }
        }

        /// <summary>
        /// 测试：脱下所有装备
        /// </summary>
        [ContextMenu("测试：脱下所有")]
        public void TestUnequipAll()
        {
            if (_equipmentManager != null)
            {
                _equipmentManager.UnequipAll();
            }
        }

        /// <summary>
        /// 示例：设置一套完整装备
        /// </summary>
        [ContextMenu("示例：穿上一套装备")]
        public void ExampleEquipFullSet()
        {
            if (_equipmentManager == null)
                return;

            // 装备一套完整装备
            _equipmentManager.Equip(GameObjectEquipmentType.Bag, 0);        // 背包1
            _equipmentManager.Equip(GameObjectEquipmentType.Top, 0);        // 上衣1
            _equipmentManager.Equip(GameObjectEquipmentType.Bottom, 0);     // 裤子1
            _equipmentManager.Equip(GameObjectEquipmentType.Shoes, 0);      // 鞋子1
            _equipmentManager.Equip(GameObjectEquipmentType.Hair, 0);       // 头发1
            _equipmentManager.Equip(GameObjectEquipmentType.Headgear, 0);   // 头盔1

            Debug.Log("已装备一套完整装备");
        }

        /// <summary>
        /// 示例：随机换装
        /// </summary>
        [ContextMenu("示例：随机换装")]
        public void ExampleRandomEquip()
        {
            if (_equipmentManager == null)
                return;

            // 对每个装备部位随机装备
            var equipmentTypes = System.Enum.GetValues(typeof(GameObjectEquipmentType));

            foreach (GameObjectEquipmentType type in equipmentTypes)
            {
                if (type == GameObjectEquipmentType.None)
                    continue;

                // 随机索引（0-4，假设每个部位至少有5个装备）
                int randomIndex = Random.Range(0, 5);
                _equipmentManager.Equip(type, randomIndex);
            }

            Debug.Log("随机换装完成");
        }
    }
}
