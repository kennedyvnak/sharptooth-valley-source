using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.Interaction {
    [RequireComponent(typeof(Collider2D))]
    public class Interactor : MonoBehaviour {
        [SerializeField] private UnityEvent<InteractionObject> m_WalkToInteractionObjectTrigger;
        [SerializeField] private UnityEvent<InteractionObject> m_SetupInteractionObjectTrigger;

        public UnityEvent<InteractionObject> walkToInteractionObjectTrigger => m_WalkToInteractionObjectTrigger;
        public UnityEvent<InteractionObject>  setupInteractionObjectTrigger => m_SetupInteractionObjectTrigger;

        private InteractorPoint _point;

        public InteractorPoint point => _point;

        private void Awake() {
            _point = GameObject.FindGameObjectWithTag("InteractorPoint").GetComponent<InteractorPoint>();
            _point.SetInteractor(this);
        }
    }
}
