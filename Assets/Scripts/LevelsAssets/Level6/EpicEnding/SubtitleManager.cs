using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.LevelAssets.Level6.EpicEnding {
    public class SubtitleManager : Singleton<SubtitleManager> {
        [Serializable]
        public class Subtitle {
            [Serializable]
            public struct SubtitleInfo {
                [TextArea] public string text;
                public float time;
            }

            public SubtitleInfo[] info;
        }

        [SerializeField] private TextMeshProUGUI m_Subtitle;
        [SerializeField] private Image m_Background;
        [SerializeField] private float m_BackgroundAlpha;
        [SerializeField] private float m_FadeTime, m_Delay, m_CharTime, m_MinTime;
        [SerializeField] private TextMeshProUGUI m_ForgiveYourselfText;

        private TweenerCore<Color, Color, ColorOptions> _fadeInTween;
        private TweenerCore<Color, Color, ColorOptions> _fadeOutTween;

        private Subtitle _currentSubtitle;
        private Action _onFinish;
        private int _subtitleIndex;

        public TextMeshProUGUI forgiveYourselfText => m_ForgiveYourselfText; 

        public void StartSubtitle(Subtitle subtitle, System.Action onFinish) {
            _currentSubtitle = subtitle;
            _onFinish = onFinish;

            _fadeInTween = m_Subtitle.DOFade(1.0f, m_FadeTime).SetDelay(m_Delay).OnComplete(() => _fadeOutTween.Restart()).SetAutoKill(false).Pause();
            _fadeOutTween = m_Subtitle.DOFade(0.0f, m_FadeTime).OnComplete(TWEEN_FadeOut).SetAutoKill(false).Pause();

            m_Subtitle.gameObject.SetActive(true);

            PlaySubtitle(0);
        }

        public void ToggleBackground(bool enabled) {
            m_Background.DOFade(enabled ? m_BackgroundAlpha : 0.0f, m_FadeTime).OnComplete(() => {
                m_Background.enabled = enabled;
            });
        }

        private void PlaySubtitle(int index) {
            _subtitleIndex = index;
            var info = _currentSubtitle.info[index];
            m_Subtitle.text = info.text;
            m_Subtitle.ForceMeshUpdate();
            _fadeOutTween.SetDelay(info.time <= 0.0f ? Mathf.Max(m_CharTime * m_Subtitle.textInfo.characterCount, m_MinTime) : info.time);
            _fadeInTween.Restart();
        }

        private void TWEEN_FadeOut() {
            _subtitleIndex++;
            if (_currentSubtitle.info.Length == _subtitleIndex) {
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
