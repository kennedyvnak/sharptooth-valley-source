using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class GolemShocked : GameTrigger {
        [SerializeField] private float m_ShockDuration;
        [SerializeField] private GameObject m_ShockEffect;
        [SerializeField] private Vector3 m_ShockEffectOffset;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            var bastheet = GameCharactersManager.instance.bastheet;
            var curState = bastheet.stateMachine.currentState;
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.ShockStateAnimationHash);
            AudioPool.instance.PlayResourcedAudio("Audio/SFXgolenElectricATTACK");
            m_ShockEffect.transform.position = bastheet.transform.position + m_ShockEffectOffset;
            m_ShockEffect.SetActive(true);
            DOVirtual.DelayedCall(m_ShockDuration, () => {
                bastheet.stateMachine.EnterState(curState);
                m_ShockEffect.SetActive(false);
                handler.onReturnToDialogue.Invoke();
            });

            return true;
        }
    }
}