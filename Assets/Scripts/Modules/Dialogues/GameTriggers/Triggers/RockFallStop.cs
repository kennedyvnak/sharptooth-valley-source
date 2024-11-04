using DG.Tweening;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class RockFallStop : GameTrigger {
        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            var tweens = DOTween.TweensById(RockFall.RockFallScreenShakeTweenID);
            tweens?.ForEach(x => x.Kill());
            StartCoroutine(Helpers.DelayForFramesCoroutine(1, handler.onReturnToDialogue.Invoke));
            return true;
        }
    }
}