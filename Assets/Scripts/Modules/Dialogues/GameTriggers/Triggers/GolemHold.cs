using NFHGame.Characters;
using NFHGame.Inventory.UI;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class GolemHold : GameTrigger {
        [SerializeField] private Transform m_GolemHead;
        [SerializeField] private Vector2 m_GolemHeadOffset;

        private BastheetCharacterController _bastheet;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            _bastheet = GameCharactersManager.instance.bastheet;
            _bastheet.PickObject(m_GolemHead, m_GolemHeadOffset, handler.onReturnToDialogue.Invoke);
            return true;
        }

        public void SetGolemHead(bool b) {
            if (_bastheet && b) {
                _bastheet.stateMachine.pickState.DestroyPickObject();
                _bastheet = null;
            } else {
                m_GolemHead.gameObject.SetActive(!b);
            }
        }
    }
}