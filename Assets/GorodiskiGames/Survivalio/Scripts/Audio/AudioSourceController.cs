using Game.Config;
using UnityEngine;

namespace Game.Audio
{
    public sealed class AudioSourceController
    {
        private readonly AudioSourceView _view;
        private MusicType _type;
        private float _clipVolume;
        private float _fadeStartTime;

        public AudioSourceView View => _view;
        public float ClipVolume => _clipVolume;
        public float FadeStartTime => _fadeStartTime;
        public MusicType Type => _type;

        public AudioSourceController(AudioSourceView view, MusicType type)
        {
            _view = view;
            _type = type;
        }

        public AudioSourceController(AudioSourceView view, MusicType type, float clipVolume, float fadeStartTime, AudioClip clip, float startTime, float volumeResult)
        {
            _view = view;
            _type = type;
            _clipVolume = clipVolume;
            _fadeStartTime = fadeStartTime;

            var source = _view.Source;
            source.playOnAwake = false;
            source.clip = clip;
            source.time = startTime;
            source.loop = false;
            source.volume = volumeResult;
        }

        public void PlayScheduled(double dspStartTime)
        {
            _view.Source.PlayScheduled(dspStartTime);
        }

        public void SetScheduledEndTime(double endTime)
        {
            _view.Source.SetScheduledEndTime(endTime);
        }

        public void Play()
        {
            _view.Source.Play();
        }
    }
}

