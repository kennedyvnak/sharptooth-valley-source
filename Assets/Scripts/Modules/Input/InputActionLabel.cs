using NFHGame.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NFHGame {
    public class InputActionLabel : MonoBehaviour {
        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private InputActionReference m_StartActionReference;
        [SerializeField] private InputBinding.DisplayStringOptions m_DisplayStringOptions;
        [SerializeField] private UnityEvent<Sprite> m_OnUpdate;

        [SerializeField] private bool m_IgnoreFitting;
        [SerializeField] private Image m_Icon;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private float m_TextSpacing;

        public UnityEvent<Sprite> onUpdateDisplay => m_OnUpdate;

        private InputActionReference _actionReference;
        public InputActionReference actionReference {
            get => _actionReference;
            set {
                _actionReference = value;
                UpdateDisplay();
            }
        }

        private void Start() {
            if (m_Text)
                m_Text.text = name;
            actionReference = m_StartActionReference;
        }

        public void UpdateDisplay() {
            InputAction action = actionReference.asset.FindAction(actionReference.action.id);
            int bindingIndex = action.GetBindingIndex(InputBinding.MaskByGroup(InputManager.instance.playerInput.currentControlScheme));

            action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath, m_DisplayStringOptions);

            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
                return;

            onUpdateDisplay?.Invoke(InputManager.instance.GetBindingIcon(controlPath));

            if (!m_IgnoreFitting) {
                var preferredTextSize = m_Text.GetPreferredValues();
                Vector2 iconSize = m_Icon.rectTransform.sizeDelta;

                var pos = m_Text.rectTransform.anchoredPosition;
                pos.x = iconSize.x + m_TextSpacing;
                m_Text.rectTransform.anchoredPosition = pos;

                var rectTransform = (RectTransform)transform;
                float x = iconSize.x + m_TextSpacing + preferredTextSize.x;
                float y = Mathf.Max(iconSize.y, preferredTextSize.y);
                rectTransform.sizeDelta = new Vector2(x, y);
            }
        }
    }
}
