using DG.Tweening;
using NFHGame.Interaction;
using UnityEngine;

namespace NFHGame.HaloManager {
    public class HaloSpriteRendererColorListener : HaloListener {
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        [SerializeField] private Color m_ColorWhenEnabled, m_ColorWhenDisabled;

        private Tweener _tweener;

        protected override void EVENT_HaloToggled(bool enabled) {
            base.EVENT_HaloToggled(enabled);
            _tweener?.Kill();
            _tweener = DOVirtual.Color(m_SpriteRenderer.color, enabled ? m_ColorWhenEnabled : m_ColorWhenDisabled, InteractionProvider.instance.haloAnimDuration, x => m_SpriteRenderer.color = x);
        }

        protected override void EVENT_ForceToggle(bool enabled) {
            base.EVENT_ForceToggle(enabled);
            m_SpriteRenderer.color = enabled ? m_ColorWhenEnabled : m_ColorWhenDisabled;
        }
    }
}
