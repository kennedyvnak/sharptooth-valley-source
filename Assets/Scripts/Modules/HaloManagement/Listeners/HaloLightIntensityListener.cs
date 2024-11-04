using DG.Tweening;
using NFHGame.Interaction;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame.HaloManager {
    public class HaloLightIntensityListener : HaloListener {
        [SerializeField] private Light2D m_Light;
        [SerializeField] private float m_IntensityWhenEnabled, m_IntensityWhenDisabled;

        private Tweener _tweener;

        public float intensityWhenEnabled { get => m_IntensityWhenEnabled; set => m_IntensityWhenEnabled = value; }
        public float intensityWhenDisabled { get => m_IntensityWhenDisabled; set => m_IntensityWhenDisabled = value; }

        protected override void EVENT_HaloToggled(bool enabled) {
            base.EVENT_HaloToggled(enabled);
            _tweener?.Kill();
            _tweener = DOVirtual.Float(m_Light.intensity, enabled ? m_IntensityWhenEnabled : m_IntensityWhenDisabled, InteractionProvider.instance.haloAnimDuration, x => m_Light.intensity = x);
        }

        protected override void EVENT_ForceToggle(bool enabled) {
            base.EVENT_ForceToggle(enabled);
            m_Light.intensity = enabled ? m_IntensityWhenEnabled : m_IntensityWhenDisabled;
        }
    }
}