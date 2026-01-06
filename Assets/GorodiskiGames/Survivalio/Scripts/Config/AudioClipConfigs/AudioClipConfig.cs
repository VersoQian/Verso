using System;
using UnityEngine;

namespace Game.Config
{
    public enum MusicType
    {
        None,
        GamePlay,
        Menu,
        SFX
    }

    public abstract class AudioClipConfig : ScriptableObject
    {
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
    }
}

