using UnityEngine;

namespace NFHGame.Interaction {
    public class InteractionObjectPointCollider : MonoBehaviour {
        public InteractionObject interactionObject { get; private set; }

        public void Register(InteractionObject interactionObject) {
            this.interactionObject = interactionObject;
            interactionObject.onInteractionEnabled.AddListener(EVENT_EnableColliders);
            interactionObject.onInteractionDisabled.AddListener(EVENT_DisableColliders);
        }

        public void PointClick(InteractorPoint point) {
            interactionObject.TRIGGER_PointClick(point);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent<InteractorPoint>(out var interactorPoint)) {
                interactionObject.TRIGGER_InteractorPointEnter(interactorPoint);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.TryGetComponent<InteractorPoint>(out var interactorPoint)) {
                interactionObject.TRIGGER_InteractorPointExit(interactorPoint);
            }
        }

        private void EVENT_EnableColliders() {
            gameObject.SetActive(true);
        }

        private void EVENT_DisableColliders() {
            gameObject.SetActive(false);
        }
    }
}