using System;
using UnityEngine;

namespace Game.ThirdHub.Equipment
{
    /// <summary>
    /// GameObject 装备槽位配置
    /// 用于配置一个装备部位下的所有装备选项
    /// </summary>
    [Serializable]
    public class GameObjectEquipmentSlot
    {
        /// <summary>装备部位类型</summary>
        [SerializeField] private GameObjectEquipmentType _equipmentType;

        /// <summary>该部位的父节点（例如：Parts/Bag、Parts/Hair）</summary>
        [SerializeField] private Transform _slotParent;

        /// <summary>该部位下的所有装备选项</summary>
        [SerializeField] private GameObject[] _equipmentOptions;

        /// <summary>当前激活的装备索引（-1 表示不穿戴）</summary>
        private int _currentEquipmentIndex = -1;

        public GameObjectEquipmentType EquipmentType => _equipmentType;
        public Transform SlotParent => _slotParent;
        public GameObject[] EquipmentOptions => _equipmentOptions;
        public int CurrentEquipmentIndex => _currentEquipmentIndex;

        public GameObjectEquipmentSlot(GameObjectEquipmentType type, Transform parent)
        {
            _equipmentType = type;
            _slotParent = parent;
        }

        /// <summary>
        /// 初始化：收集该槽位下的所有装备选项
        /// </summary>
        public void Initialize()
        {
            if (_slotParent == null)
            {
                Debug.LogWarning($"装备槽位 {_equipmentType} 的父节点为空");
                return;
            }

            // 收集所有子对象作为装备选项
            int childCount = _slotParent.childCount;
            _equipmentOptions = new GameObject[childCount];

            for (int i = 0; i < childCount; i++)
            {
                _equipmentOptions[i] = _slotParent.GetChild(i).gameObject;
            }

            // 默认关闭所有装备
            DeactivateAll();
        }

        /// <summary>
        /// 装备指定索引的装备
        /// </summary>
        /// <param name="index">装备索引（-1 表示脱下装备）</param>
        public void EquipByIndex(int index)
        {
            if (_equipmentOptions == null || _equipmentOptions.Length == 0)
            {
                Debug.LogWarning($"装备槽位 {_equipmentType} 没有可用的装备选项");
                return;
            }

            // 先关闭所有装备
            DeactivateAll();

            // 如果索引有效，激活指定装备
            if (index >= 0 && index < _equipmentOptions.Length)
            {
                if (_equipmentOptions[index] != null)
                {
                    _equipmentOptions[index].SetActive(true);
                    _currentEquipmentIndex = index;
                }
                else
                {
                    Debug.LogWarning($"装备槽位 {_equipmentType} 的索引 {index} 对应的装备为空");
                }
            }
            else
            {
                // 索引为-1或超出范围，表示脱下装备
                _currentEquipmentIndex = -1;
            }
        }

        /// <summary>
        /// 通过装备名称装备
        /// </summary>
        /// <param name="equipmentName">装备GameObject的名称</param>
        public void EquipByName(string equipmentName)
        {
            if (_equipmentOptions == null || _equipmentOptions.Length == 0)
                return;

            for (int i = 0; i < _equipmentOptions.Length; i++)
            {
                if (_equipmentOptions[i] != null && _equipmentOptions[i].name == equipmentName)
                {
                    EquipByIndex(i);
                    return;
                }
            }

            Debug.LogWarning($"装备槽位 {_equipmentType} 找不到名为 '{equipmentName}' 的装备");
        }

        /// <summary>
        /// 关闭所有装备
        /// </summary>
        public void DeactivateAll()
        {
            if (_equipmentOptions == null)
                return;

            foreach (var equipment in _equipmentOptions)
            {
                if (equipment != null)
                {
                    equipment.SetActive(false);
                }
            }

            _currentEquipmentIndex = -1;
        }
    }
}
