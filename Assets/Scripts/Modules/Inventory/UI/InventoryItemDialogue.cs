using System.Collections;
using Articy.Unity;
using Articy.Unity.Interfaces;
using NFHGame.ArticyImpl;
using NFHGame.DialogueSystem;
using NFHGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.Inventory.UI {
    public class InventoryItemDialogue : MonoBehaviour {
        [SerializeField] private Button m_LabelButtonPrefab;

        [Header("UI")]
        [SerializeField] private CanvasGroup m_CanvasGroup;

        private InventoryItem _item;
        private Image _itemThumb;
        private Selectable _leftSelect;
        private Selectable _upSelect;
        private string _cachedArticy;

        private void OnDestroy() {
            if (_cachedArticy != null) {
                ArticyManager.notifications.RemoveListener(_cachedArticy, RecreateButtons);
                _cachedArticy = null;
            }
        }

        public void SetItem(InventoryItem item, Selectable itemThumb, Selectable upSelect) {
            if (!item) return;

            _item = item;
            _leftSelect = itemThumb;
            _upSelect = upSelect;
            _itemThumb = itemThumb.transform.GetChild(0).GetComponent<Image>();

            CreateButtons();

            if (_cachedArticy != null) {
                ArticyManager.notifications.RemoveListener(_cachedArticy, RecreateButtons);
                _cachedArticy = null;
            }

            if (!string.IsNullOrWhiteSpace(item.conditionToRecreate)) {
                ArticyManager.notifications.AddListener(item.conditionToRecreate, RecreateButtons);
                _cachedArticy = item.conditionToRecreate;
            }
        }

        private void CreateButtons() {
            transform.DestroyChildren();
            Button lastButton = null;

            if (_item.dialogues != null) {
                foreach (var dialogue in _item.dialogues) {
                    if (!dialogue.dialogue.HasReference || !dialogue.dialogue.ValidStart()) continue;
                    var dialogueObj = dialogue.dialogue.GetObject();
                    CreateButton(GetDialogueLabel(dialogueObj), (b) => PlayDialogue(dialogue.dialogue));
                }
            }

            if (_item.actions != null) {
                foreach (var action in _item.actions) {
                    if (!action || !action.IsValid()) continue;

                    Button oldB = lastButton;

                    CreateButton(action.GetLabel(), (b) => TriggerAction(action, b));
                }
            }

            void CreateButton(string label, UnityEngine.Events.UnityAction<Button> onClick) {
                var button = Instantiate(m_LabelButtonPrefab, transform);

                if (lastButton) {
                    lastButton.SetDownNav(button);
                    button.SetUpNav(lastButton);
                } else {
                    button.SetUpNav(_upSelect);
                }

                button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = label;
                button.onClick.AddListener(() => onClick(button));
                button.SetLeftNav(_leftSelect);
                lastButton = button;
            }
        }

        private string GetDialogueLabel(ArticyObject dialogueObj) {
            if (dialogueObj is IObjectWithText locaText) {
                return locaText.Text;
            }
            return "==Invalid Dialogue==";
        }

        private void TriggerAction(InventoryItemAction action, Button b) {
            StartCoroutine(TriggerActionCoroutine(action, b));
        }

        private IEnumerator TriggerActionCoroutine(InventoryItemAction action, Button b) {
            var selectedObject = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
            m_CanvasGroup.interactable = false;
            InventoryManager.instance.ToggleInput(false);

            yield return action.OnTrigger(new ActionContext() { item = _item, itemThumb = _itemThumb, button = b, itemDisplay = InventoryManager.instance.itemDisplayImage });

            m_CanvasGroup.interactable = true;
            InventoryManager.instance.ToggleInput(true);
            yield return null;
            EventSystem.current.SetSelectedGameObject(selectedObject);
        }

        private void PlayDialogue(ArticyRef dialogue) {
            var selectedObject = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
            m_CanvasGroup.interactable = false;
            InventoryManager.instance.ToggleInput(false);
            var handler = DialogueManager.instance.PlayHandledDialogue(dialogue);
            handler.onDialogueFinished += () => {
                m_CanvasGroup.interactable = true;
                InventoryManager.instance.ToggleInput(true);
                StartCoroutine(Helpers.DelayForFramesCoroutine(1, () => EventSystem.current.SetSelectedGameObject(selectedObject)));
            };
        }

        private void RecreateButtons(string n, object o) => CreateButtons();
    }
}