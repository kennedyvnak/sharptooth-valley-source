using NFHGame.Interaction.Behaviours;
using UnityEngine;

namespace NFHGame.Interaction {
    public class InteactionPointIcon : InteractionBehaviour {
        [SerializeField] private InteractionPointIconManager.Icon m_Icon;

        protected override void BindEvents() {
            interactionObject.onInteractorPointEnter.AddListener(EVENT_PointerEnter);
            interactionObject.onInteractorPointExit.AddListener(EVENT_PointerExit);
        }

        protected override void UnbindEvents() {
            interactionObject.onInteractorPointEnter.RemoveListener(EVENT_PointerEnter);
            interactionObject.onInteractorPointExit.RemoveListener(EVENT_PointerExit);
        }

        private void EVENT_PointerEnter(InteractorPoint point) {
            InteractionPointIconManager.instance.SetIcon(m_Icon);
            InteractionPointIconManager.instance.InsertBlock(GetInstanceID());
        }

        private void EVENT_PointerExit(InteractorPoint point) {
            if (InteractionPointIconManager.instance)
                InteractionPointIconManager.instance.RemoveBlock(GetInstanceID());
        }
    }
}
