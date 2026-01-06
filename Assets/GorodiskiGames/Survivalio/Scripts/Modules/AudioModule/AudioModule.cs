using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Audio;
using Game.Config;
using Game.Core;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.Modules
{
    public sealed class AudioModule : Module<AudioModuleView>
    {
        private const float _changeTypeFadeDuration = 1.5f;
        private const float _crossFadeDuration = 5f;
        private const float _volumeChangeFade = 0.5f;
        private const float _snippetDuration = 0.2f;

        [Inject] private AudioManager _audioManager;
        [Inject] private Timer _timer;

        private AudioSourceController _audioSource;

        private readonly Dictionary<AudioSourceController, float> _expirationSourcesMap;
        private readonly Dictionary<AudioSourceController, Coroutine> _coroutinesMap;
        private readonly Dictionary<MusicType, int> _musicIndexesMap;

        public AudioModule(AudioModuleView view) : base(view)
        {
            _expirationSourcesMap = new Dictionary<AudioSourceController, float>();
            _coroutinesMap = new Dictionary<AudioSourceController, Coroutine>();
            _musicIndexesMap = new Dictionary<MusicType, int>();
        }

        public override void Initialize()
        {
            var view = _view.AudioSourcesPool.Get<AudioSourceView>();
            _audioSource = new AudioSourceController(view, MusicType.None);

            _musicIndexesMap[MusicType.GamePlay] = 0;
            _musicIndexesMap[MusicType.Menu] = 0;

            _audioManager.ON_PLAY_MUSIC += TryPlayMusic;
            _audioManager.ON_MUSIC_VOLUME_CHANGED += OnMusicVolumeChanged;
            _audioManager.ON_FOOT_ON_GROUND += OnFootOnGround;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _audioManager.ON_PLAY_MUSIC -= TryPlayMusic;
            _audioManager.ON_MUSIC_VOLUME_CHANGED -= OnMusicVolumeChanged;
            _audioManager.ON_FOOT_ON_GROUND -= OnFootOnGround;
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            foreach (var audioSource in _expirationSourcesMap.Keys.ToList())
            {
                var duration = _expirationSourcesMap[audioSource];
                duration -= Time.deltaTime;
                _expirationSourcesMap[audioSource] = duration;

                if (duration > 0f)
                    continue;

                _expirationSourcesMap.Remove(audioSource);
                _view.AudioSourcesPool.Release(audioSource.View);
            }

            if(_audioSource.Type == MusicType.None)
                return;

            var time = _audioSource.View.Source.time;
            var fadeStartTime = _audioSource.FadeStartTime;
            if (time < fadeStartTime)
                return;

            PlayNext(_crossFadeDuration);
        }

        private void TryPlayMusic(MusicType type)
        {
            if(_audioSource.Type == type)
                return;

            FadeToTargetVolume(_audioSource, 0f, _changeTypeFadeDuration, true);
            PlayMusic(type, _musicIndexesMap[type]);
            FadeToTargetVolume(_audioSource, _audioManager.MusicVolume, _changeTypeFadeDuration, false);
        }

        public void PlayNext(float fadeDuration)
        {
            var type = _audioSource.Type;
            var configs = _view.MusicMap[type];
            var index = _musicIndexesMap[type] + 1;

            if (index >= configs.Length)
                index = 0;

            _musicIndexesMap[type] = index;

            FadeToTargetVolume(_audioSource, 0f, fadeDuration, true);
            PlayMusic(type, index);
            FadeToTargetVolume(_audioSource, _audioManager.MusicVolume, fadeDuration, false);
        }

        private void OnMusicVolumeChanged(float newVolume)
        {
            FadeToTargetVolume(_audioSource, newVolume, _volumeChangeFade, false);
        }

        private void FadeToTargetVolume(AudioSourceController audioSource, float targetVolume, float duration, bool isRelease)
        {
            var startVolume = audioSource.View.Source.volume;
            var clipVolume = audioSource.ClipVolume;
            var volumeResult = targetVolume * clipVolume;

            if (_coroutinesMap.TryGetValue(audioSource, out Coroutine existed))
                _view.StopCoroutine(existed);

            var coroutine = _view.StartCoroutine(FadeToVolume(audioSource, startVolume, volumeResult, duration, isRelease));
            _coroutinesMap[audioSource] = coroutine;
        }

        public void PlayMusic(MusicType type, int index)
        {
            var configs = _view.MusicMap[type];
            var config = configs[index];
            var startTime = config.StartTime;
            var stopTime = (config.StopTime > 0) ? config.StopTime : config.Clip.length;
            var fadeStartTime = stopTime - _crossFadeDuration;
            var clipVolume = config.Volume;
            var clip = config.Clip;
            var volumeResult = 0f;
            var view = _view.AudioSourcesPool.Get<AudioSourceView>();
            var audioSource = new AudioSourceController(view, type, clipVolume, fadeStartTime, clip, startTime, volumeResult);
            audioSource.Play();

            _audioSource = audioSource;
        }

        private IEnumerator FadeToVolume(AudioSourceController audioSource, float startVolume, float targetVolume, float duration, bool isRelease)
        {
            var time = 0f;
            var source = audioSource.View.Source;

            while (time < duration)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Clamp01(Mathf.Lerp(startVolume, targetVolume, time / duration));
                yield return null;
            }

            source.volume = Mathf.Clamp01(targetVolume);
            _coroutinesMap.Remove(audioSource);

            if (!isRelease)
                yield break;

            _view.AudioSourcesPool.Release(audioSource.View);
        }

        private void OnFootOnGround()
        {
            var view = _view.AudioSourcesPool.Get<AudioSourceView>();

            var type = MusicType.SFX;
            var config = _view.FootstepConfig;
            var clip = config.Clip;
            var startTime = config.StartTimes[Random.Range(0, config.StartTimes.Length)];
            var clipVolume = config.Volume;
            var sfxVolume = _audioManager.SFXVolume;
            var volumeResult = sfxVolume * clipVolume;

            var audioSource = new AudioSourceController(view, type, clipVolume, 0f, clip, startTime, volumeResult);
            audioSource.Play();

            _expirationSourcesMap[audioSource] = _snippetDuration;
        }
    }
}
