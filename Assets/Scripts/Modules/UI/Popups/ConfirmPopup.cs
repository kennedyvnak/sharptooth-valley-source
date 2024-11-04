using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.Screens {
    public class ConfirmPopup : Singleton<ConfirmPopup> {
        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private Button m_Confirm;
        [SerializeField] private Button m_Cancel;

        private System.Action<bool> _onClick;

        private void Start() {
            m_Confirm.onClick.AddListener(EVENT_Confirm);
            m_Cancel.onClick.AddListener(EVENT_Cancel);
        }

        public void Popup(string text, string confirmText, string cancelText, float seconds, System.Action<bool> onClick) {
            m_Text.text = text;
            _onClick = onClick;

            m_Confirm.interactable = false;
            m_Cancel.interactable = false;
            m_Confirm.GetComponentInChildren<TextMeshProUGUI>().text = confirmText;
            m_Cancel.GetComponentInChildren<TextMeshProUGUI>().text = cancelText;

            DOVirtual.DelayedCall(seconds, () => {
                m_Confirm.interactable = true;
                m_Cancel.interactable = true;
                EventSystem.current.SetSelectedGameObject(m_Cancel.gameObject);
            });

            m_Group.gameObject.SetActive(true);
            m_Group.ToggleGroupAnimated(true, ScreenManager.instance.screenFadeDuration);
        }

        public Tweener ClosePopup() {
            var t = m_Group.ToggleGroupAnimated(true, ScreenManager.instance.screenFadeDuration);
            t.onComplete += () => m_Group.gameObject.SetActive(false);
            return t;
        }

        private void EVENT_Confirm() {
            _onClick?.Invoke(true);
        }

        private void EVENT_Cancel() {
            _onClick?.Invoke(false);
        }
    }
}
