using UnityEngine;

namespace NFHGame.Interaction {
    public class InteractionProvider : ScriptableSingletons.ScriptableSingleton<InteractionProvider> {
        [SerializeField] private Gradient m_OutlineColorGradient;
        [SerializeField] private Gradient[] m_OutlineColorGradients;
        [SerializeField] private float m_AnimDuration;
        [SerializeField] private float m_HaloAnimDuration;

        public Gradient outlineColorGradient => m_OutlineColorGradient;
        public Gradient[] outlineColorGradients => m_OutlineColorGradients;
        public float animDuration => m_AnimDuration;
        public float haloAnimDuration => m_HaloAnimDuration;
    }
}
