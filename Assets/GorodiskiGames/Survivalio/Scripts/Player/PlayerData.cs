using System;
using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerData
    {
        private const int _defaultConfigIndex = 0;
        private const int _defaultLevel = 0;

        public int EquippedWeapon; //serial
        public Dictionary<int, int> StoredWeapons; //serial, configIndex
        public Dictionary<int, int> WeaponLevels;  //serial, level

        public List<int> EquippedCloth; //serials
        public Dictionary<int, int> StoredCloth;  //serial, configIndex
        public Dictionary<int, int> ClothLevels;  //serial, level

        public static PlayerData Load(GameConfig config)
        {
            try
            {
                var key = GameConstants.PlayerDataKey;
                var dataLoaded = PlayerPrefs.GetString(key);

                PlayerData result = null;
                if (string.IsNullOrEmpty(dataLoaded))
                {
                    result = new PlayerData();
                    result.SetDefaults(config);
                    return result;
                }
                result = JsonConvert.DeserializeObject<PlayerData>(dataLoaded);
                return result;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                var result = new PlayerData();
                result.SetDefaults(config);
                return result;
            }
        }

        public PlayerData()
        {
            StoredWeapons = new Dictionary<int, int>();
            WeaponLevels = new Dictionary<int, int>();

            EquippedCloth = new List<int>();
            StoredCloth = new Dictionary<int, int>();
            ClothLevels = new Dictionary<int, int>();
        }

        public void SetDefaults(GameConfig gameConfig)
        {
            var weaponConfig = gameConfig.WeaponMap.Values.ElementAtOrDefault(_defaultConfigIndex);
            if(weaponConfig == null)
                Log.Error("No WeaponConfig added to the GameConfig");
            else
            {
                var weaponSerial = GameConstants.GetSerial();

                EquippedWeapon = weaponSerial;
                StoredWeapons[weaponSerial] = _defaultConfigIndex;
                WeaponLevels[weaponSerial] = _defaultLevel;
            }

            foreach (ClothElementType clothType in Enum.GetValues(typeof(ClothElementType)))
            {
                var clothConfig = gameConfig.ClothMap.Values.ToList().Find(config => config.ClothType == clothType);
                if(clothConfig == null)
                {
                    Log.Error("No ClothConfig for type: " + clothType + " added to the GameConfig");
                    continue;
                }

                var configIndex = clothConfig.Index;
                var clothSerial = GameConstants.GetSerial();

                EquippedCloth.Add(clothSerial);

                StoredCloth[clothSerial] = configIndex;
                ClothLevels[clothSerial] = _defaultLevel;
            }

            if (!gameConfig.AddAllEquipment)
                return;

            //add Weapons
            foreach (var index in gameConfig.WeaponMap.Keys)
            {
                if (StoredWeapons.ContainsValue(index))
                    continue;

                var weaponSerial = GameConstants.GetSerial();
                StoredWeapons[weaponSerial] = index;
                WeaponLevels[weaponSerial] = _defaultLevel;
            }

            //add Cloth
            foreach (var index in gameConfig.ClothMap.Keys)
            {
                if(StoredCloth.ContainsValue(index))
                    continue;

                var clothSerial = GameConstants.GetSerial();
                StoredCloth[clothSerial] = index;
                ClothLevels[clothSerial] = _defaultLevel;
            }

            Save();
        }

        public void Save()
        {
            var dataSerialized = JsonConvert.SerializeObject(this);
            var key = GameConstants.PlayerDataKey;
            PlayerPrefs.SetString(key, dataSerialized);
            PlayerPrefs.Save();
        }
    }
}

