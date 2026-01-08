using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ThirdHub.Equipment
{
    /// <summary>
    /// GameObject 装备管理器
    /// 管理基于 GameObject 激活/禁用的换装系统
    /// </summary>
    public class GameObjectEquipmentManager : MonoBehaviour
    {
        [Header("自动扫描设置")]
        [Tooltip("装备部位父节点。留空则在当前对象下查找名为 'Parts' 的子节点。")]
        [SerializeField] private Transform _partsRoot;

        /// <summary>所有装备槽位</summary>
        [SerializeField] private List<GameObjectEquipmentSlot> _equipmentSlots = new List<GameObjectEquipmentSlot>();

        /// <summary>装备槽位字典（类型 -> 槽位）</summary>
        private Dictionary<GameObjectEquipmentType, GameObjectEquipmentSlot> _slotDictionary;

        /// <summary>装备变化事件</summary>
        public event Action<GameObjectEquipmentType, int> OnEquipmentChanged;

        private void Awake()
        {
            TryAutoSetupSlots();
            InitializeSlots();
        }

        /// <summary>
        /// 初始化所有装备槽位
        /// </summary>
        private void InitializeSlots()
        {
            _slotDictionary = new Dictionary<GameObjectEquipmentType, GameObjectEquipmentSlot>();

            foreach (var slot in _equipmentSlots)
            {
                if (slot == null || slot.SlotParent == null)
                    continue;

                slot.Initialize();
                _slotDictionary[slot.EquipmentType] = slot;
            }
        }

        /// <summary>
        /// 在运行时自动生成槽位配置（当未手动配置时）
        /// </summary>
        private void TryAutoSetupSlots()
        {
            var hasConfiguredSlots = _equipmentSlots != null && _equipmentSlots.Exists(s => s != null && s.SlotParent != null);
            if (hasConfiguredSlots)
                return;

            if (_partsRoot == null)
            {
                // 默认在自身下查找名为 "Parts" 的节点
                _partsRoot = transform.Find("Parts");
            }

            if (_partsRoot == null)
            {
                Debug.LogWarning($"{name} 缺少 Parts 根节点，无法自动生成装备槽位");
                return;
            }

            _equipmentSlots ??= new List<GameObjectEquipmentSlot>();
            _equipmentSlots.Clear();

            foreach (GameObjectEquipmentType equipmentType in Enum.GetValues(typeof(GameObjectEquipmentType)))
            {
                if (equipmentType == GameObjectEquipmentType.None)
                    continue;

                var slotParent = _partsRoot.Find(equipmentType.ToString());
                if (slotParent != null && slotParent.childCount > 0)
                {
                    _equipmentSlots.Add(new GameObjectEquipmentSlot(equipmentType, slotParent));
                }
            }
        }

        /// <summary>
        /// 装备指定部位的指定装备
        /// </summary>
        /// <param name="equipmentType">装备部位</param>
        /// <param name="equipmentIndex">装备索引（-1 表示脱下）</param>
        public void Equip(GameObjectEquipmentType equipmentType, int equipmentIndex)
        {
            if (!_slotDictionary.TryGetValue(equipmentType, out var slot))
            {
                Debug.LogWarning($"找不到装备槽位：{equipmentType}");
                return;
            }

            slot.EquipByIndex(equipmentIndex);
            OnEquipmentChanged?.Invoke(equipmentType, equipmentIndex);
        }

        /// <summary>
        /// 通过名称装备
        /// </summary>
        /// <param name="equipmentType">装备部位</param>
        /// <param name="equipmentName">装备名称</param>
        public void EquipByName(GameObjectEquipmentType equipmentType, string equipmentName)
        {
            if (!_slotDictionary.TryGetValue(equipmentType, out var slot))
            {
                Debug.LogWarning($"找不到装备槽位：{equipmentType}");
                return;
            }

            slot.EquipByName(equipmentName);
            OnEquipmentChanged?.Invoke(equipmentType, slot.CurrentEquipmentIndex);
        }

        /// <summary>
        /// 脱下指定部位的装备
        /// </summary>
        public void Unequip(GameObjectEquipmentType equipmentType)
        {
            Equip(equipmentType, -1);
        }

        /// <summary>
        /// 脱下所有装备
        /// </summary>
        public void UnequipAll()
        {
            foreach (var slot in _equipmentSlots)
            {
                slot.DeactivateAll();
            }
        }

        /// <summary>
        /// 获取指定部位当前装备的索引
        /// </summary>
        public int GetCurrentEquipmentIndex(GameObjectEquipmentType equipmentType)
        {
            if (_slotDictionary.TryGetValue(equipmentType, out var slot))
            {
                return slot.CurrentEquipmentIndex;
            }
            return -1;
        }

        /// <summary>
        /// 批量装备
        /// </summary>
        /// <param name="equipmentSetup">装备配置（部位 -> 索引）</param>
        public void EquipMultiple(Dictionary<GameObjectEquipmentType, int> equipmentSetup)
        {
            if (equipmentSetup == null)
                return;

            foreach (var kvp in equipmentSetup)
            {
                Equip(kvp.Key, kvp.Value);
            }
        }

        #region 编辑器辅助方法

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器辅助：自动设置装备槽位
        /// </summary>
        /// <param name="partsRoot">装备根节点（例如：Character_114/Parts）</param>
        public void EditorSetupSlots(Transform partsRoot)
        {
            if (partsRoot == null)
            {
                Debug.LogError("Parts 根节点为空");
                return;
            }

            _equipmentSlots.Clear();

            // 查找所有装备部位
            foreach (GameObjectEquipmentType equipmentType in Enum.GetValues(typeof(GameObjectEquipmentType)))
            {
                if (equipmentType == GameObjectEquipmentType.None)
                    continue;

                // 尝试找到对应的子节点
                string nodeName = equipmentType.ToString();
                Transform slotParent = partsRoot.Find(nodeName);

                if (slotParent != null && slotParent.childCount > 0)
                {
                    var slot = new GameObjectEquipmentSlot(equipmentType, slotParent);
                    _equipmentSlots.Add(slot);
                    Debug.Log($"找到装备槽位：{equipmentType}，包含 {slotParent.childCount} 个装备选项");
                }
            }

            Debug.Log($"装备槽位设置完成，共 {_equipmentSlots.Count} 个槽位");
        }
#endif

        #endregion
    }
}
