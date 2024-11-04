using NFHGame.SceneManagement;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.SceneManagement.SceneState;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class SpammyDigging1e1 : GameTrigger {
        [SerializeField] private SceneLoadHandler m_LoadHandler;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => {
                GameKeysManager.instance.ToggleGameKey(Level1e1StateController.PassageOpenKey, true);
                m_LoadHandler.LoadScene();
            };
            handler.onReturnToDialogue?.Invoke();
            return false;
        }
    }
}