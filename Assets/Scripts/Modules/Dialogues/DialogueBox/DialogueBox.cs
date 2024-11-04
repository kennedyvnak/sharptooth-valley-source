using UnityEngine;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class DialogueBox : MonoBehaviour {
        [SerializeField] protected CanvasGroup m_Group;

        public event System.Action OnStepDialogue;
        public virtual void ToggleBox(bool enabled) {
            m_Group.ToggleGroup(enabled);
        }

        public virtual void ClearCache() { }

        protected void StepDialogue() {
            OnStepDialogue?.Invoke();
        }
    }
}
