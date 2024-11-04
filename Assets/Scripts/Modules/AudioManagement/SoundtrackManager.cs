using UnityEngine;
using DG.Tweening;

namespace NFHGame.AudioManagement {
    using VolumeTweener = DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>;

    public class SoundtrackManager : SingletonPersistent<SoundtrackManager> {
        [SerializeField] private AudioSource[] m_SoundtrackSources;

        [Header("Fade")]
        [SerializeField] private float m_FadeDuration;
        [SerializeField] private Ease m_EnableEase = Ease.InCubic;
        [SerializeField] private Ease m_DisableEase = Ease.OutCubic;

        private VolumeTweener[] _tweeners;

        private AudioMusicObject _currentSoundtrack;
        private int _playingAtIdx = -1;

        public AudioMusicObject currentSoundtrack => _currentSoundtrack;
        public AudioSource[] soundtrackSources => m_SoundtrackSources;

        protected override void Awake() {
            base.Awake();
            _tweeners = new VolumeTweener[m_SoundtrackSources.Length];
        }

        public void SetSoundtrack(AudioMusicObject soundtrack) {
            if (soundtrack == _currentSoundtrack) return;

            var toDisable = GetCurrentSource();
            var toEnable = GetNextSource(out int prevIdx);

            if (prevIdx != -1) _tweeners[prevIdx].Kill();
            _tweeners[_playingAtIdx].Kill();

            if (prevIdx != -1) _tweeners[prevIdx] = toDisable.DOFade(0.0f, m_FadeDuration).SetEase(m_DisableEase).OnComplete(() => toDisable.Stop());
            _tweeners[_playingAtIdx] = toEnable.DOFade(1.0f, m_FadeDuration).SetEase(m_EnableEase);
            PlayMusic(soundtrack, toEnable);

            _currentSoundtrack = soundtrack;
        }

        public void StopSoundtrack() {
            if (_playingAtIdx == -1) return;

            var toDisable = GetCurrentSource();
            _tweeners[_playingAtIdx].Kill();
            _tweeners[_playingAtIdx] = toDisable.DOFade(0.0f, m_FadeDuration).SetEase(m_DisableEase).OnComplete(() => toDisable.Stop());
            _currentSoundtrack = null;
            _playingAtIdx = -1;
        }

        private void PlayMusic(AudioMusicObject obj, AudioSource source) {
            obj.CloneToSource(source);
            source.Stop();
            if (obj.musicStart) {
                source.PlayOneShot(obj.musicStart);
                source.PlayScheduled(AudioSettings.dspTime + obj.musicStart.length);
            } else {
                source.Play();
            }
        }

        public AudioSource GetCurrentSource() => _playingAtIdx == -1 ? null : m_SoundtrackSources[_playingAtIdx];

        public AudioSource GetNextSource(out int prevIdx) {
            prevIdx = _playingAtIdx;
            _playingAtIdx++;
            if (_playingAtIdx < 0 || _playingAtIdx + 1 >= m_SoundtrackSources.Length)
                _playingAtIdx = 0;
            return m_SoundtrackSources[_playingAtIdx];
        }
    }
}