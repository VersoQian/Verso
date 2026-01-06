using UnityEngine;
using Game.UI.Pool;
using Game.Config;
using System.Collections.Generic;

namespace Game.Modules
{
    public class AudioModuleView : MonoBehaviour
    {
        [Header("Audio Sources")]
        public ComponentPoolFactory AudioSourcesPool;

        [Header("Sound Clips")]
        [SerializeField] private SingleAudioClipConfig[] _gamePlayMusicConfigs;
        [SerializeField] private SingleAudioClipConfig[] _menuMusicConfigs;
        public MultipleAudioClipConfig FootstepConfig;

        public Dictionary<MusicType, SingleAudioClipConfig[]> MusicMap;

        private void Awake()
        {
            MusicMap = new Dictionary<MusicType, SingleAudioClipConfig[]>();
            MusicMap[MusicType.GamePlay] = _gamePlayMusicConfigs;
            MusicMap[MusicType.Menu] = _menuMusicConfigs;
        }
    }
}

