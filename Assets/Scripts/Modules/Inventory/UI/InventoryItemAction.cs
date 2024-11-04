using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NFHGame.Inventory.UI {

    [CreateAssetMenu(menuName = "Scriptable/Inventory/Item Action")]
    public class InventoryItemAction : ScriptableObject {
        [SerializeField] private string m_Label;
        [SerializeField] private UnityEvent m_OnTrigger;
        [SerializeField] private UnityEvent m_OnEndTrigger;

        public string label => m_Label;
        public UnityEvent onTrigger => m_OnTrigger;
        public UnityEvent onEndTrigger => m_OnEndTrigger;

        public virtual bool IsValid() => true;

        public virtual IEnumerator OnTrigger(ActionContext context) {
            onTrigger?.Invoke();

            yield return new WaitForSeconds(2.0f);

            onEndTrigger?.Invoke();
        }

        public virtual string GetLabel() => label;
    }

    public struct ActionContext {
        public InventoryItem item;
        public Button button;
        public Image itemDisplay;
        public Image itemThumb;
    }
}