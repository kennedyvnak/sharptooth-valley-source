using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class GetAmnesiaRock : GameTrigger {
        [SerializeField] private Transform m_Object;
        [SerializeField] private Vector2 m_ObjectOffset;

        private GameTriggerProcessor.GameTriggerHandler _handler;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            _handler = handler;
            var bast = GameCharactersManager.instance.bastheet;
            bast.PickObject(m_Object, m_ObjectOffset, EVENT_PickedItem);
            return true;
        }

        private void EVENT_PickedItem() {
            DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += EVENT_DialogueFinished;
            _handler.onReturnToDialogue.Invoke();
        }

        private void EVENT_DialogueFinished() {
            var bast = GameCharactersManager.instance.bastheet;
            bast.stateMachine.pickState.DestroyPickObject();
        }
    }
}
