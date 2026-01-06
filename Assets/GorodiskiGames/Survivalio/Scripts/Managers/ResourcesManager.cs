using System;
using UnityEngine;

namespace Game.Managers
{
    public sealed class ResourcesManager
    {
        private const string _subLevelSlotPath = "Prefabs/SubLevelSlot";
        private const string _levelSlotPath = "Prefabs/LevelSlot";
        private const string _rawCameraPrefabPath = "Prefabs/RawCameraPrefab";
        private const string _inventorySlotPath = "Prefabs/InventorySlot";

        public GameObject LoadSubLevelSlot()
        {
            return Resources.Load<GameObject>(_subLevelSlotPath);
        }

        public GameObject LoadLevelSlot()
        {
            return Resources.Load<GameObject>(_levelSlotPath);
        }

        public GameObject LoadRawCameraPrefab()
        {
            return Resources.Load<GameObject>(_rawCameraPrefabPath);
        }

        public GameObject LoadInventorySlot()
        {
            return Resources.Load<GameObject>(_inventorySlotPath);
        }

        public void UnloadResources(GameObject gameObject)
        {
            Resources.UnloadAsset(gameObject);
        }

        public void Dispose()
        {

        }
    }
}