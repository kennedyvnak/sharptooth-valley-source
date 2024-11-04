namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class MapOfTheWorld : GameTrigger {
        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            MapController.instance.ShowMap(() => {
                handler.onReturnToDialogue.Invoke();
            });
            return true;
        }
    }
}