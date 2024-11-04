using UnityEngine;
using UnityEngine.EventSystems;

namespace NFHGame {
    public class SaveSlotButtons : MonoBehaviour, ISelectHandler, IDeselectHandler {
        [SerializeField] private SaveSlot m_Parent;

        public void OnSelect(BaseEventData eventData) {
            m_Parent.Push();
        }

        public void OnDeselect(BaseEventData eventData) {
            m_Parent.Pop();
        }
    }
}
