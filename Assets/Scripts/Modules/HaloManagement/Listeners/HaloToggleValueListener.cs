using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.HaloManager {
    public class HaloToggleValueListener : HaloListener {
        [SerializeField] private UnityEvent m_OnHaloEnabled;
        [SerializeField] private UnityEvent m_OnHaloDisabled;

        [SerializeField] private UnityEvent<bool> m_OnToggleHalo;

        [SerializeField] private bool m_ActiveWhenEnabled = true;
        public bool activeWhenEnabled => m_ActiveWhenEnabled;

        protected override void EVENT_HaloToggled(bool enabled) {
            base.EVENT_HaloToggled(enabled);

            bool shouldBeActive = ShouldBeActive(enabled);
            (shouldBeActive ? m_OnHaloEnabled : m_OnHaloDisabled)?.Invoke();
            m_OnToggleHalo?.Invoke(shouldBeActive);
        }

        protected override void EVENT_ForceToggle(bool enabled) {
            base.EVENT_ForceToggle(enabled);

            bool shouldBeActive = ShouldBeActive(enabled);
            (shouldBeActive ? m_OnHaloEnabled : m_OnHaloDisabled)?.Invoke();
            m_OnToggleHalo?.Invoke(shouldBeActive);
        }

        public bool ShouldBeActive(bool enabled) => (enabled && m_ActiveWhenEnabled) || (!enabled && !m_ActiveWhenEnabled);
    }
}