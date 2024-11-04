using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class DualCharactersBox : DialogueBox {
        [SerializeField] private RectTransform m_SpeechBox;
        [SerializeField] private TextMeshProUGUI m_SpeechText;
        [SerializeField] private PortraitDisplay m_LeftPortrait;
        [SerializeField] private PortraitDisplay m_RightPortrait;
        [SerializeField] private TextMeshProUGUI m_LeftName;
        [SerializeField] private TextMeshProUGUI m_RightName;
        [SerializeField] private Button m_StepButton;
        [SerializeField] private float m_SpeechBoxLineSize, m_SpeechBoxSpacing, m_SpeechBoxLineOffset, m_SpeechBoxMinSize;

        private TextVertexAnimator _textVertexAnimator;
        private Coroutine _typeRoutine;

        private void Awake() {
            _textVertexAnimator = new TextVertexAnimator(m_SpeechText);
            m_StepButton.onClick.AddListener(StepDialogue);
        }

        public virtual bool SetSpeech(string speech, System.Action finishDraw, out TextVertexAnimator textVertexAnimator, out Coroutine typeRoutine) {
            textVertexAnimator = _textVertexAnimator;

            bool empty = string.IsNullOrWhiteSpace(speech);
            if (empty) {
                m_SpeechText.text = string.Empty;
                finishDraw?.Invoke();
                _typeRoutine = typeRoutine = null;
            } else {
                this.EnsureCoroutineStopped(ref _typeRoutine);
                _textVertexAnimator.textAnimating = false;
                var commands = DialogueUtility.ProcessInputString(speech, out var processedMessage);
                textVertexAnimator = _textVertexAnimator;
                _typeRoutine = typeRoutine = StartCoroutine(_textVertexAnimator.AnimateTextIn(commands, processedMessage, finishDraw));
            }

            int lineCount = m_SpeechText.textInfo.lineCount;
            var size = m_SpeechBox.sizeDelta;
            size.y = Mathf.Max(m_SpeechBoxMinSize, m_SpeechBoxLineSize * lineCount + m_SpeechBoxSpacing * (lineCount - 1) + m_SpeechBoxLineOffset);

            m_SpeechBox.sizeDelta = size;

            return !empty;
        }

        public virtual void SetPortraits(Portraits.Portrait left, Portraits.Portrait right) {
            m_LeftPortrait.SetPortrait(left, left.facingRight ? 1 : -1);
            m_RightPortrait.SetPortrait(right, right.facingRight ? -1 : 1);

            m_SpeechBox.offsetMin = new Vector2(m_LeftPortrait.rectTransform.sizeDelta.x + left.boxRightOffset, m_SpeechBox.offsetMin.y);
            m_SpeechBox.offsetMax = new Vector2(-(m_RightPortrait.rectTransform.sizeDelta.x + right.boxRightOffset), m_SpeechBox.offsetMax.y);
        }

        public virtual void SetNames(string left, string right) {
            m_LeftName.text = left;
            m_RightName.text = right;
        }

        public override void ToggleBox(bool enabled) {
            base.ToggleBox(enabled);
            m_LeftName.gameObject.SetActive(enabled);
            m_RightName.gameObject.SetActive(enabled);
        }
    }
}
