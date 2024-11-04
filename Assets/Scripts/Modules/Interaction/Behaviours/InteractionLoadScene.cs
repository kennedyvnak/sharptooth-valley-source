using NFHGame.SceneManagement;
using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionLoadScene : InteractionBehaviour {
        [SerializeField] private SceneReference m_SceneReference;
        [SerializeField] private string m_AnchorID;
        [SerializeField] private bool m_StopInputWhenStart = true;
        [SerializeField] private bool m_CharactersWalkToLeft = false;

        public System.Func<InteractorPoint, bool> validation;

        private bool _triggered;

        protected override void BindEvents() {
            interactionObject.onInteractorPointClick.AddListener(EVENT_PointClick);
        }

        protected override void UnbindEvents() {
            interactionObject.onInteractorPointClick.RemoveListener(EVENT_PointClick);
        }

        public void LoadScene() {
            var handler = SceneLoader.instance.CreateHandler(m_SceneReference, m_AnchorID);
            handler.charactersToLeftSide = m_CharactersWalkToLeft;
            SceneLoader.instance.LoadScene(handler);
            if (m_StopInputWhenStart)
                handler.StopInput();
        }

        private void EVENT_PointClick(InteractorPoint point) {
            if (_triggered || (validation != null && !validation(point))) return;
            _triggered = true;
            LoadScene();
        }
    }
}
