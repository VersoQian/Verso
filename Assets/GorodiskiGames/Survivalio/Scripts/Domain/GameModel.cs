using Core;
using Game.Config;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Game.Domain
{
    public sealed class GameModel : Observable
    {
        public static GameModel Load(GameConfig config)
        {
            try
            {
                var data = PlayerPrefs.GetString(GameConstants.GameModelKey);
                if (string.IsNullOrEmpty(data))
                {
                    var model = new GameModel();
                    model.SetDefaults(config);
                    return model;
                }
                var result = JsonConvert.DeserializeObject<GameModel>(data);
                result.Check(config);
                return result;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                var model = new GameModel();
                model.SetDefaults(config);
                return model;
            }
        }

        public int Level;
        public int Energy;
        public int Gems;
        public int Cash;
        public float LevelDurationMax;
        public bool IsVibration;
        public float MusicVolume;
        public float SFXVolume;

        public GameModel()
        {

        }

        private void SetDefaults(GameConfig config)
        {
            Level = config.DefaultLevel;
            Energy = config.DefaultEnergy;
            Gems = config.DefaultGems;
            Cash = config.DefaultCash;
            IsVibration = true;
            MusicVolume = config.MusicVolumeDefault;
            SFXVolume = config.SFXVolumeDefault;

            Save();
        }

        private void Check(GameConfig config)
        {
            if(Level >= config.LevelConfigs.Length)
            {
                Level = 0;
                Save();
            }
        }

        public void Save()
        {
            try
            {
                var data = JsonConvert.SerializeObject(this);
                PlayerPrefs.SetString(GameConstants.GameModelKey, data);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void Remove()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        public void SaveResource(ResourceItemType type, int amount)
        {
            if (type == ResourceItemType.GemsPink)
                Gems += amount;
            else if (type == ResourceItemType.Cash)
                Cash += amount;
            else if (type == ResourceItemType.Energy)
                Energy += amount;

            Save();
            SetChanged();
        }
    }
}
