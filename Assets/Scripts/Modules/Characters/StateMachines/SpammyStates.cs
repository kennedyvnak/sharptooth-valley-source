using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using UnityEngine;

namespace NFHGame.Characters.StateMachines {
    public abstract class SpammyStateBase : FollowerStateBase {
        protected SpammyStateBase(FollowerStateMachine stateMachine) : base(stateMachine) { }
    }

    public class SpammyArrowBattleState : SpammyStateBase {
        private Sprite[] _bowTensionSprites;

        public SpammyArrowBattleState(SpammyStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(FollowerStateBase previousState) {
            follower.anim.enabled = false;
            ArticyManager.notifications.AddListener("trustPoints.spamPoints", SpamPointsChanged);
            SpamPointsChanged(null, ArticyVariables.globalVariables.trustPoints.spamPoints);
        }

        public override void Exit() {
            ArticyManager.notifications.RemoveListener("trustPoints.spamPoints", SpamPointsChanged);
            follower.anim.enabled = true;
        }

        private void SpamPointsChanged(string variable, object value) {
            int val = (int)value;
            follower.spriteRenderer.sprite = _bowTensionSprites[Mathf.Clamp(val - 1, 0, _bowTensionSprites.Length - 1)];
        }

        public void Setup(Sprite[] bowTensionSprites) {
            _bowTensionSprites = bowTensionSprites;
        }
    }
}