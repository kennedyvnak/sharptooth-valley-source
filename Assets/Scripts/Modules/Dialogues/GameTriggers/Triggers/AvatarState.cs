using DG.Tweening;
using NFHGame.Characters;
using NFHGame.RangedValues;
using NFHGame.Serialization;
using System.Collections;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class AvatarState : GameTrigger {
        [SerializeField] private float m_RockFallStartDelay;
        [SerializeField] private float m_ReturnDialogueDelay;
        [SerializeField] private RangedFloat m_DinnerFlipDelay;
        [SerializeField, TextArea] private string m_TryToExitWithPowersGameOverLabel;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.avatarState.EnterAvatarState(0, 0, false, false, true);

            var dinner = GameCharactersManager.instance.dinner;
            dinner.shitDinner = true;
            dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.shit);
            
            GameManager.instance.forceOverrideGameOverLabel = m_TryToExitWithPowersGameOverLabel;
            DataManager.instance.Save();

            DOVirtual.DelayedCall(m_RockFallStartDelay, () => {
                StartCoroutine(DinnerFlip());
                (GameTriggerProcessor.instance.GetTrigger("rockFallAlert") as RockFall).StartRockFall();
            });

            DOVirtual.DelayedCall(m_ReturnDialogueDelay, handler.onReturnToDialogue.Invoke);
            return true;
        }

        private IEnumerator DinnerFlip() {
            var dinner = GameCharactersManager.instance.dinner;
            while (true) {
                float delay = Random.Range(m_DinnerFlipDelay.min, m_DinnerFlipDelay.max);
                yield return new WaitForSeconds(delay);
                dinner.SetFacingDirection(dinner.facingDirection * -1);
            }
        }
    }
}
