using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.HaloManager {
    public class HaloListener : MonoBehaviour {
        [SerializeField] private UnityEvent<bool> m_HaloToggled;
        public UnityEvent<bool> haloToggled => m_HaloToggled;

        [SerializeField] private UnityEvent<bool> m_ForceHaloToggled;
        public UnityEvent<bool> forceHaloToggled => m_ForceHaloToggled;

        private void OnEnable() {
            if (HaloManager.instance) {
                HaloManager.instance.haloToggled.AddListener(EVENT_HaloToggled);
                HaloManager.instance.haloForceToggled.AddListener(EVENT_ForceToggle);
                EVENT_ForceToggle(HaloManager.instance.haloActive);
            }
        }

        private void OnDisable() {
            if (HaloManager.instance) {
                HaloManager.instance.haloToggled.RemoveListener(EVENT_HaloToggled);
                HaloManager.instance.haloForceToggled.RemoveListener(EVENT_ForceToggle);
            }
        }

        protected virtual void EVENT_HaloToggled(bool enabled) {
            haloToggled?.Invoke(enabled);
        }

        protected virtual void EVENT_ForceToggle(bool enabled) {
            m_ForceHaloToggled?.Invoke(enabled);
        }
    }
}