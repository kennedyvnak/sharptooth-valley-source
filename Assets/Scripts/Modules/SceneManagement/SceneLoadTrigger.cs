using NFHGame.Characters;
using NFHGame.Characters.StateMachines;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.SceneManagement {
    public class SceneLoadTrigger : MonoBehaviour {
        [SerializeField] private SceneReference m_SceneReference;
        [SerializeField] private bool m_CharactersWalkToLeftSide;
        [SerializeField] private string m_AnchorID;
        [SerializeField] private UnityEvent m_OnTrigger;

        public SceneReference sceneReference => m_SceneReference;
        public string anchorID => m_AnchorID;
        public UnityEvent onTrigger => m_OnTrigger;

        private BastheetCharacterController _characterEntered;
        private bool _triggered = false;

        private void Update() {
            if (!_triggered && _characterEntered && _characterEntered.stateMachine.currentState is IBastheetInputState) {
                Trigger();
            }
        }

        public void Trigger() {
            onTrigger?.Invoke();
            _triggered = true;
        }

        protected void LoadScene(SceneLoader.SceneLoadingHandler handler) {
            handler.charactersToLeftSide = m_CharactersWalkToLeftSide;
            SceneLoader.instance.LoadScene(handler);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent<BastheetCharacterController>(out var character)) {
                _characterEntered = character;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.TryGetComponent<BastheetCharacterController>(out var character) && _characterEntered == character) {
                _characterEntered = null;
            }
        }
    }
}