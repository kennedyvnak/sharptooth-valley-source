using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class TripleCharactersBox : DialogueBox {
        [SerializeField] private RectTransform m_SpeechBox;
        [SerializeField] private TextMeshProUGUI m_SpeechText;
        [SerializeField] private PortraitDisplay m_APortrait, m_BPortrait, m_CPortrait;
        [SerializeField] private TextMeshProUGUI m_LeftName, m_RightName;
        [SerializeField] private Button m_StepButton;
        [SerializeField] private float m_SpeechBoxLineSize, m_SpeechBoxSpacing, m_SpeechBoxLineOffset, m_SpeechBoxMinSize, m_BoxRightBOffset;

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

        public virtual void SetPortraits(Portraits.Portrait a, Portraits.Portrait b, Portraits.Portrait c) {
            m_APortrait.SetPortrait(a, a.facingRight ? 1 : -1);
            m_BPortrait.SetPortrait(b, b.facingRight ? -1 : 1);
            m_CPortrait.SetPortrait(c, c.facingRight ? -1 : 1);

            m_SpeechBox.offsetMin = new Vector2(m_APortrait.rectTransform.sizeDelta.x + a.boxRightOffset, m_SpeechBox.offsetMin.y);
            m_SpeechBox.offsetMax = new Vector2(-(m_CPortrait.rectTransform.sizeDelta.x + c.boxRightOffset + m_BoxRightBOffset), m_SpeechBox.offsetMax.y);
        }

        public virtual void SetNames(string a, string b, string c) {
            m_LeftName.text = a;
            m_RightName.text = $"{c} & {b}";
        }

        public override void ToggleBox(bool enabled) {
            base.ToggleBox(enabled);
            m_LeftName.gameObject.SetActive(enabled);
            m_RightName.gameObject.SetActive(enabled);
        }
    }
}
