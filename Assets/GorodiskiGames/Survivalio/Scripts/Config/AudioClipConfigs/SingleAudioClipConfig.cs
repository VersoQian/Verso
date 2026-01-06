using UnityEngine;
using System;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/SingleAudioClipConfig")]
    public class SingleAudioClipConfig : AudioClipConfig
    {
        public float StartTime;
        public float StopTime;
    }
}

