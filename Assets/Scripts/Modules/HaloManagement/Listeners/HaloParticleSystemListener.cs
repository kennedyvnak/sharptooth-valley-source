using UnityEngine;

namespace NFHGame.HaloManager {
    public class HaloParticleSystemListener : HaloListener {
        [SerializeField] private ParticleSystem m_Particles;
        [SerializeField] private bool m_ActiveWhenEnabled = true;

        protected override void EVENT_HaloToggled(bool enabled) {
            base.EVENT_HaloToggled(enabled);

            if (m_ActiveWhenEnabled && enabled) {
                m_Particles.Play(false);
            } else {
                m_Particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        protected override void EVENT_ForceToggle(bool enabled) {
            base.EVENT_ForceToggle(enabled);

            if (m_ActiveWhenEnabled && enabled) {
                m_Particles.Play(true);
            } else {
                m_Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}