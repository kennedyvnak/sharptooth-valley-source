using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class NarratorBox : DialogueBox {
        [SerializeField] private TextMeshProUGUI m_SpeechText;
        [SerializeField] private RectTransform m_SpeechBox;
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
    }
}
