using UnityEngine;

namespace NFHGame.Interaction {
    public class InteractionObjectCollider : MonoBehaviour {
        public InteractionObject interactionObject { get; private set; }

        public void Register(InteractionObject interactionObject) {
            this.interactionObject = interactionObject;
            interactionObject.onInteractionEnabled.AddListener(EVENT_EnableColliders);
            interactionObject.onInteractionDisabled.AddListener(EVENT_DisableColliders);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent<Interactor>(out var interactor)) {
                interactionObject.TRIGGER_InteractorEnter(interactor);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.TryGetComponent<Interactor>(out var interactor)) {
                interactionObject.TRIGGER_InteractorExit(interactor);
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