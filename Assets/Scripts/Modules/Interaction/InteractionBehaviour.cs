using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    [RequireComponent(typeof(InteractionObject))]
    public abstract class InteractionBehaviour : MonoBehaviour {
        public InteractionObject interactionObject { get; private set; }

        protected virtual void Awake() {
            interactionObject = GetComponent<InteractionObject>();
        }

        public virtual void Init(InteractionObject interactionObject) {
            this.interactionObject = interactionObject;
            BindEvents();
        }

        private void OnDestroy() {
            UnbindEvents();
        }

        protected abstract void BindEvents();
        protected abstract void UnbindEvents();
    }
}
