using UnityEngine;
using System;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/MultipleAudioClipConfig")]
    public class MultipleAudioClipConfig : AudioClipConfig
    {
        [Tooltip("In seconds")]
        public float[] StartTimes;
    }
}



