using Cinemachine;
using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class SmallRockFall : GameTrigger {
        [SerializeField] private CinemachineImpulseSource m_DefaultSource;
        [SerializeField] private AudioObject m_QuakeSound;
        [SerializeField] private string m_CorridorShake;

        public override bool Match(string id) {
            return base.Match(id) || id == m_CorridorShake;
        }

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            bool corridorShake = handler.articyTrigger.Template.GameTrigger.TriggerCode == m_CorridorShake;
            var bast = GameCharactersManager.instance.bastheet;
            bool exit = false;

            m_DefaultSource.GenerateImpulse();
            var quakeSource = AudioPool.instance.PlaySound(m_QuakeSound);

            if (corridorShake && bast.stateMachine.currentState == bast.stateMachine.moveState) {
                bast.stateMachine.animState.Animate(BastheetCharacterController.IdleAnimationHashes.right);
                exit = true;
            }

            DOVirtual.DelayedCall(m_DefaultSource.m_ImpulseDefinition.m_ImpulseDuration, () => {
                quakeSource.source.DOFade(0.0f, 2.0f).SetEase(Helpers.CameraOutEase).OnComplete(() => quakeSource.source.Stop());
                if (exit)
                    bast.stateMachine.EnterState(bast.stateMachine.moveState);
                handler.onReturnToDialogue.Invoke();
            });
            return true;
        }
    }
}
