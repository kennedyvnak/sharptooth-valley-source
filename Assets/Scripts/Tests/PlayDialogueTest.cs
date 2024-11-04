using UnityEngine;
using UnityEngine.InputSystem;
using NFHGame.DialogueSystem;
using Articy.Unity;

namespace NFHGameTests {
    public class PlayDialogueTest : MonoBehaviour {
        [SerializeField] private ArticyRef m_DialogueRef;
        public ArticyRef dialogueRef { get => m_DialogueRef; set => m_DialogueRef = value; }

        private void Update() {
            if (Keyboard.current.f1Key.wasPressedThisFrame) {
                var handler = DialogueManager.instance.CreateHandler();
                handler.onDialogueStartDraw += () => Debug.Log("onDialogueStartDraw");
                handler.onDialogueFinishDraw += () => Debug.Log("onDialogueFinishDraw");
                handler.onDialogueShowBranches += () => Debug.Log("onDialogueShowBranches");
                handler.onDialogueSelectBranch += (branch) => Debug.Log($"onDialogueSelectBranch: {branch}");
                handler.onDialogueProcessGameTrigger += (trigger) => Debug.Log($"onDialogueProcessGameTrigger: {trigger}");
                handler.onDialogueFinished += () => Debug.Log("onDialogueFinished");
                DialogueManager.instance.PlayHandler(m_DialogueRef, handler);
            }
        }
    }
}