using DG.Tweening;
using NFHGame.AudioManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace NFHGame {
    public class FusionFx : MonoBehaviour {
        [Header("Components")]
        [SerializeField] private ParticleSystem m_FusionParticle;
        [SerializeField] private SpriteRenderer m_CircleRenderer;
        [SerializeField] private Light2D m_Light;
        [SerializeField] private AudioSource m_AudioSource;

        [SerializeField] private float m_CircleSize, m_LightIntensity, m_FadeShakesAnimDuration;

        [Header("Shake")]
        [SerializeField] private float m_ShakeDelay;
        [SerializeField] private float m_ShakeDuration, m_ShakeStrength = 1;
        [SerializeField] private int m_ShakeVibrato = 10;
        [SerializeField] private float m_ShakeRandomness = 90;
        [SerializeField] private bool m_ShakeFadeOut = true;
        [SerializeField] private ShakeRandomnessMode m_ShakeRandomnessMode = ShakeRandomnessMode.Full;

        [Header("Sound")]
        [SerializeField] private AudioObject m_ChargeStartSound;
        [SerializeField] private AudioObject m_ChargeLoopSound;

        private Tween _circleTween;
        private double _cannonStart;

        public void StartAnim() {
            m_FusionParticle.Play();
            DOVirtual.DelayedCall(m_ShakeDelay, StartShake);
            _cannonStart = AudioSettings.dspTime;
            m_ChargeLoopSound.CloneToSource(m_AudioSource);
            m_AudioSource.Stop();
            m_AudioSource.PlayOneShot(m_ChargeStartSound.clip);
            m_AudioSource.PlayScheduled(AudioSettings.dspTime + m_ChargeStartSound.clip.length);
        }

        public void ShotAnim(System.Action onShot = null) {
            StartCoroutine(SoundControl(onShot));
        }

        public void EndAnim() {
            m_FusionParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _circleTween?.Kill();
            _circleTween = m_CircleRenderer.transform.DOScale(0.0f, m_FadeShakesAnimDuration).OnUpdate(SetLightIntensity);

            m_AudioSource.DOFade(0.0f, m_FadeShakesAnimDuration).OnComplete(() => {
                m_AudioSource.Stop();
            });
        }

        private void StartShake() {
            _circleTween?.Kill();
            _circleTween = m_CircleRenderer.transform.DOScale(m_CircleSize, m_FadeShakesAnimDuration).OnComplete(() => {
                _circleTween = m_CircleRenderer.transform.DOShakeScale(m_ShakeDuration, m_ShakeStrength, m_ShakeVibrato, m_ShakeRandomness, m_ShakeFadeOut, m_ShakeRandomnessMode).SetLoops(-1).OnUpdate(SetLightIntensity);
            }).OnUpdate(SetLightIntensity);
        }

        private void SetLightIntensity() {
            float lerp = Mathf.InverseLerp(0.0f, m_CircleSize, m_CircleRenderer.transform.localScale.x);
            m_Light.intensity = lerp * m_LightIntensity;
        }

        private IEnumerator SoundControl(System.Action onShot) {
            float startLenght = m_ChargeStartSound.clip.length;

            while (m_AudioSource.isPlaying) {
                double startTime = AudioSettings.dspTime - _cannonStart;
                bool start = startTime < startLenght;
                bool startInEnd = start && startTime + 0.1f > startLenght;
                bool loopEnd = !start && m_AudioSource.time + 0.1f > m_ChargeLoopSound.clip.length;

                if (startInEnd || loopEnd)
                    break;

                yield return null;
            }

            m_AudioSource.Stop();
            onShot?.Invoke();

            EndAnim();
        }
    }
}
