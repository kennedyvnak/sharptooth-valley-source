using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.LevelAssets.Level4e1 {
    public class SacrificeSubtitleManager : Singleton<SacrificeSubtitleManager> {
        [SerializeField] private TextMeshProUGUI m_Subtitle;
        [SerializeField] private Image m_Background;
        [SerializeField] private float m_BackgroundAlpha;

        private TweenerCore<Color, Color, ColorOptions> _fadeInTween;
        private TweenerCore<Color, Color, ColorOptions> _fadeOutTween;

        private SacrificeCutsceneSubtitle _currentSubtitle;
        private Action _onFinish;
        private int _subtitleIndex;

        public void StartSubtitle(SacrificeCutsceneSubtitle cutsceneSubtitle, System.Action onFinish) {
            _currentSubtitle = cutsceneSubtitle;
            _onFinish = onFinish;

            _fadeInTween = m_Subtitle.DOFade(1.0f, _currentSubtitle.fadeTime).OnComplete(() => {
                var subtitle = _currentSubtitle.subtitles[_subtitleIndex];
                _fadeOutTween.Restart(true, subtitle.duration);
            }).SetAutoKill(false).SetUpdate(UpdateType.Normal, true).Pause();
            _fadeOutTween = m_Subtitle.DOFade(0.0f, _currentSubtitle.fadeTime).SetUpdate(UpdateType.Normal, true).OnComplete(TWEEN_FadeOut).SetAutoKill(false).Pause();

            m_Subtitle.gameObject.SetActive(true);

            PlaySubtitle(0);
        }

        public void StartSubtitle(SacrificeCutsceneSubtitle cutsceneSubtitle, SacrificeCutsceneSubtitle.Duration[] d, System.Action onFinish) {
            _currentSubtitle = cutsceneSubtitle;
            _onFinish = onFinish;

            _fadeInTween = m_Subtitle.DOFade(1.0f, _currentSubtitle.fadeTime).OnComplete(() => {
                var subtitle = _currentSubtitle.subtitles[_subtitleIndex];
                _fadeOutTween.Restart(true, subtitle.duration);
            }).SetAutoKill(false).SetUpdate(UpdateType.Fixed, true).Pause();
            _fadeOutTween = m_Subtitle.DOFade(0.0f, _currentSubtitle.fadeTime).SetUpdate(UpdateType.Fixed, true).OnComplete(TWEEN_FadeOut).SetAutoKill(false).Pause();

            m_Subtitle.gameObject.SetActive(true);

            PlaySubtitle(0);
        }

        public void ToggleBackground(bool enabled) {
            m_Background.DOFade(enabled ? m_BackgroundAlpha : 0.0f, _currentSubtitle.fadeTime).OnComplete(() => {
                m_Background.enabled = enabled;
            });
        }

        private void PlaySubtitle(int index) {
            _subtitleIndex = index;
            var subtitle = _currentSubtitle.subtitles[index];
            m_Subtitle.text = subtitle.subtitle;
            m_Subtitle.ForceMeshUpdate();
            _fadeInTween.Restart(true, subtitle.delay);
        }

        private void TWEEN_FadeOut() {
            _subtitleIndex++;
            if (_currentSubtitle.subtitles.Length == _subtitleIndex) {
                _fadeInTween.Kill();
                _fadeOutTween.Kill();
                m_Subtitle.text = string.Empty;
                m_Subtitle.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                m_Subtitle.gameObject.SetActive(false);
                _onFinish?.Invoke();
            } else {
                PlaySubtitle(_subtitleIndex);
            }
        }
    }
}
