using DG.Tweening;
using System;
using UnityEngine;

namespace NFHGame.Interaction {
    public class InteractionObjectPointHoverHighlight : MonoBehaviour {
        [SerializeField] private float m_IntensityHaloOn;
        [SerializeField] private float m_IntensityHaloOff;
        [SerializeField] private UnityEngine.Rendering.Universal.Light2D m_Target;
        [SerializeField] private SpriteRenderer m_OutlineSprite;
        [SerializeField, Range(0, 31)] private int m_TargetGradient;

        public InteractionObject interactionObject { get; private set; }

        public float animDuration => InteractionProvider.instance.animDuration;
        public Gradient outlineColorGradient => InteractionProvider.instance.outlineColorGradients[m_TargetGradient];

        public float intensityHaloOn { get => m_IntensityHaloOn; set => m_IntensityHaloOn = value; }
        public float intensityHaloOff { get => m_IntensityHaloOff; set => m_IntensityHaloOff = value; }
        public int targetGradient { get => m_TargetGradient; set => m_TargetGradient = value; }

#if UNITY_EDITOR
        public new UnityEngine.Rendering.Universal.Light2D light => m_Target;
#else
        public UnityEngine.Rendering.Universal.Light2D light => m_Target;
#endif

        private Tweener _intensityTween;
        private Tweener _outlineTween;
        private bool _enabled;
        private float _outlineFactor;

        private void Start() {
            if (m_Target) {
                m_Target.intensity = 0.0f;
                _intensityTween = DOVirtual.Float(0.0f, 1.0f, animDuration, (x) => m_Target.intensity = x).SetAutoKill(false).Pause();
            }

            if (m_OutlineSprite) {
                _outlineTween = DOVirtual.Float(0.0f, 1.0f, animDuration, (x) => {
                    m_OutlineSprite.color = outlineColorGradient.Evaluate(x);
                    _outlineFactor = x;
                }).SetAutoKill(false).Pause();
                m_OutlineSprite.color = outlineColorGradient.Evaluate(0.0f);
            }
        }

        private void OnDestroy() {
            _intensityTween?.Kill();
            _outlineTween?.Kill();
            Unregister();
        }

        public void Register(InteractionObject interactionObject) {
            this.interactionObject = interactionObject;
            interactionObject.onInteractorPointEnter.AddListener(EVENT_PointEnter);
            interactionObject.onInteractorPointExit.AddListener(EVENT_PointExit);

            HaloManager.HaloManager.instance.haloToggled.AddListener(EVENT_HaloToggled);
        }

        public void Unregister() {
            interactionObject.onInteractorPointEnter.RemoveListener(EVENT_PointEnter);
            interactionObject.onInteractorPointExit.RemoveListener(EVENT_PointExit);
            if (HaloManager.HaloManager.instance)
                HaloManager.HaloManager.instance.haloToggled.RemoveListener(EVENT_HaloToggled);
        }

        public bool EnableHighlight() {
            if (_enabled) return false;

            if (_intensityTween != null)
                ScaleTo(HaloManager.HaloManager.instance.haloActive ? m_IntensityHaloOn : m_IntensityHaloOff, m_Target.intensity, ref _intensityTween);

            if (_outlineTween != null)
                ScaleTo(1.0f, _outlineFactor, ref _outlineTween);

            _enabled = true;
            return true;
        }

        public bool DisableHighlight() {
            if (!_enabled) return false;

            if (_intensityTween != null)
                ScaleTo(0.0f, m_Target.intensity, ref _intensityTween);

            if (_outlineTween != null)
                ScaleTo(0.0f, _outlineFactor, ref _outlineTween);

            _enabled = false;
            return true;
        }

        private void EVENT_PointEnter(InteractorPoint interactorPoint) {
            EnableHighlight();
        }

        private void EVENT_PointExit(InteractorPoint interactorPoint) {
            DisableHighlight();
        }

        private void ScaleTo(float intensity, float startValue, ref Tweener lastTween) {
            if (lastTween.IsPlaying()) {
                lastTween.ChangeValues(startValue, intensity);
            } else {
                lastTween.ChangeValues(startValue, intensity, animDuration);
                lastTween.Play();
            }
        }

        private void EVENT_HaloToggled(bool x) {
            if (_enabled && _intensityTween != null)
                ScaleTo(x ? m_IntensityHaloOn : m_IntensityHaloOff, m_Target.intensity, ref _intensityTween);
        }
    }
}
