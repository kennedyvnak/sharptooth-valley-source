using DG.Tweening;
using NFHGame.AudioManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.LevelAssets.Level4e1 {
    public class SacrificeCutsceneSubtitle : MonoBehaviour {
        [System.Serializable]
        public class Subtitle {
            [TextArea] public string subtitle;
            public float duration;
            public float delay;
        }

        [System.Serializable]
        public struct Duration {
            public float start, end;
        }

        [SerializeField] private float m_ScreenFadeDuration;
        [SerializeField] private float m_FadeTime;
        [SerializeField] private Subtitle[] m_Subtitles;
        [SerializeField] private Duration[] m_Durations;
        [SerializeField] private string m_AnimationName;
        [SerializeField] private float m_AnimDuration;
        [SerializeField] private RawImage m_MapImage;
        [SerializeField] private AudioMusicObject m_SacrificeMusic;

        public Animator anim { get; private set; }

        public Subtitle[] subtitles => m_Subtitles;
        public Duration[] durations => m_Durations;
        public float fadeTime => m_FadeTime;

        public void StartAnimation(System.Action onEnd, System.Action beforeFade = null) {
            Setup();

            var group = GetComponent<CanvasGroup>();
            group.alpha = 0.0f;
            if (m_MapImage) m_MapImage.texture = Resources.Load<Texture2D>("MAP_Iridia_GAME_Big");

            AudioMusicObject cachedMusic = SoundtrackManager.instance.currentSoundtrack;
            if (m_SacrificeMusic) SoundtrackManager.instance.SetSoundtrack(m_SacrificeMusic);
            else SoundtrackManager.instance.StopSoundtrack();

            group.DOFade(1.0f, m_ScreenFadeDuration).SetEase(Helpers.CameraInEase).OnComplete(() => {
                anim.Play(m_AnimationName);
                if (m_Durations.Length > 0) SacrificeSubtitleManager.instance.StartSubtitle(this, m_Durations, null);
                DOVirtual.DelayedCall(m_AnimDuration, () => {
                    beforeFade?.Invoke();
                    SoundtrackManager.instance.SetSoundtrack(cachedMusic);
                    group.DOFade(0.0f, m_ScreenFadeDuration).SetEase(Helpers.CameraOutEase).OnComplete(() => {
                        gameObject.SetActive(false);
                        if (m_MapImage) {
                            var mapTex = m_MapImage.texture;
                            m_MapImage.texture = null;
                            Resources.UnloadAsset(mapTex);
                        }
                        onEnd?.Invoke();
                    });
                }).SetUpdate(UpdateType.Fixed);
            });
            gameObject.SetActive(true);
        }

        [ContextMenu("See Animation")]
        public void SeeAnimation() => DOVirtual.DelayedCall(1.0f, () => StartAnimation(null));

        [ContextMenu("Setup")]
        private void Setup() {
            anim = GetComponent<Animator>();
            m_Durations = new Duration[m_Subtitles.Length];
            float duration = 0.0f;
            for (int i = 0; i < m_Durations.Length; i++) {
                m_Durations[i].start = duration + m_Subtitles[i].delay;
                m_Durations[i].end = m_Durations[i].start + m_Subtitles[i].duration + 2 * fadeTime; 
                duration = m_Durations[i].end;
            }
        }
    }
}