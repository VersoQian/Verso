using System;
using Game.Config;
using Game.Domain;

namespace Game.Managers
{
    public sealed class AudioManager : IDisposable
    {
        public event Action<MusicType> ON_PLAY_MUSIC;
        public event Action<float> ON_MUSIC_VOLUME_CHANGED;
        //public event Action<float> ON_SFX_VOLUME_CHANGED;
        public event Action ON_FOOT_ON_GROUND;

        public float MusicVolume;
        public float SFXVolume;

        public AudioManager(float musicVolume, float sfxVolume)
        {
            MusicVolume = musicVolume;
            SFXVolume = sfxVolume;
        }

        public void Dispose()
        {

        }

        public void FirePlayMusic(MusicType type)
        {
            ON_PLAY_MUSIC?.Invoke(type);
        }

        public void FireMusicVolumeChanged(float value)
        {
            ON_MUSIC_VOLUME_CHANGED?.Invoke(value);
        }

        //public void FireSFXVolumeChanged(float value)
        ////{
        ////    ON_SFX_VOLUME_CHANGED?.Invoke(value);
        ////}

        public void FireFootOnGround()
        {
            ON_FOOT_ON_GROUND?.Invoke();
        }
    }
}

