using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionClick : InteractionBehaviour {
        [SerializeField] private bool m_WalkToThisOnClick = true;
        [SerializeField] private bool m_NeedsInteractor = true;

        public bool walkToThisOnClick { get => m_WalkToThisOnClick; set => m_WalkToThisOnClick = value; }
        public bool needsInteractor { get => m_NeedsInteractor; set => m_NeedsInteractor = value; }

        protected override void BindEvents() {
            interactionObject.onInteractorPointClick.AddListener(EVENT_PointClick);
        }

        protected override void UnbindEvents() {
            interactionObject.onInteractorPointClick.RemoveListener(EVENT_PointClick);
        }

        private void EVENT_PointClick(InteractorPoint point) {
            if (!m_NeedsInteractor || HaveInteractor()) {
                point.interactor.setupInteractionObjectTrigger?.Invoke(interactionObject);
                interactionObject.Interact(point.interactor);
            } else if (m_WalkToThisOnClick) {
                point.interactor.walkToInteractionObjectTrigger?.Invoke(interactionObject);
            }

            bool HaveInteractor() {
                bool interactorEnter = (interactionObject.currentInteractor && point.interactor == interactionObject.currentInteractor);
                bool areaEnter = !interactorEnter && TryGetComponent<InteractionArea>(out var area) && area.BastheetIn();
                return interactorEnter || areaEnter;
            }
        }
    }
}
