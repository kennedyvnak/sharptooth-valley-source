using NFHGame.DialogueSystem.Portraits;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class DialogueBoxWithActor : DialogueBox {
        [SerializeField] private TextMeshProUGUI m_SpeechText;
        [SerializeField] private RectTransform m_SpeechBox;
        [SerializeField] private PortraitDisplay m_RightPortraitDisplay;
        [SerializeField] private PortraitDisplay m_LeftPortraitDisplay;
        [SerializeField] private Button m_StepButton;

        [SerializeField] private Image m_BoxImage;
        [SerializeField] private Sprite m_RightBoxSprite, m_LeftBoxSprite, m_RightThinkingBoxSprite, m_LeftThinkingBoxSprite;
        [SerializeField] private float m_ArrowOffset, m_BoxOffset;

        [SerializeField] private float m_SpeechBoxMinWidth, m_SpeechBoxMaxWidth;
        [SerializeField] private float m_SpeechBoxLineSize, m_SpeechBoxBorderPadding, m_SpeechBoxSpacing, m_SpeechBoxLineOffset, m_SpeechBoxMinSize;

        [Header("Name")]
        [SerializeField] private TextMeshProUGUI m_NameText;
        [SerializeField] private float m_NameBorderXOffset;

        private TextVertexAnimator _textVertexAnimator;
        private Coroutine _typeRoutine;
        private bool _right;

        private void Awake() {
            _textVertexAnimator = new TextVertexAnimator(m_SpeechText);
            m_StepButton.onClick.AddListener(StepDialogue);
        }

        public virtual bool SetSpeech(string speech, bool thinking, System.Action finishDraw, out TextVertexAnimator textVertexAnimator, out Coroutine typeRoutine) {
            textVertexAnimator = _textVertexAnimator;

            m_BoxImage.sprite = thinking ? (_right ? m_LeftThinkingBoxSprite : m_RightThinkingBoxSprite) : (_right ? m_LeftBoxSprite : m_RightBoxSprite);

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

                m_SpeechBox.sizeDelta = new Vector2(m_SpeechBoxMaxWidth, 41.0f);

                m_SpeechText.text = processedMessage;
                m_SpeechText.ForceMeshUpdate();

                int lineCount = m_SpeechText.textInfo.lineCount;
                Vector2 size;
                size.y = Mathf.Max(m_SpeechBoxMinSize, m_SpeechBoxLineSize * lineCount + m_SpeechBoxSpacing * (lineCount - 1) + m_SpeechBoxLineOffset);
                size.x = Mathf.Max(m_SpeechBoxMinWidth, m_SpeechText.GetRenderedValues().x + m_ArrowOffset + m_BoxOffset);
                m_SpeechBox.sizeDelta = size;

                _typeRoutine = typeRoutine = StartCoroutine(_textVertexAnimator.AnimateTextIn(commands, processedMessage, finishDraw));
            }

            return !empty;
        }

        public void Setup(string name, Portrait portrait, bool rightSide, TMP_FontAsset dialogueFont) {
            m_NameText.text = name;
            m_SpeechText.font = dialogueFont;

            var portraitDisplay = rightSide ? m_RightPortraitDisplay : m_LeftPortraitDisplay;
            (!rightSide ? m_RightPortraitDisplay : m_LeftPortraitDisplay).gameObject.SetActive(false);
            bool flip = rightSide == portrait.facingRight;
            portraitDisplay.SetPortrait(portrait, flip ? -1 : 1);
            portraitDisplay.gameObject.SetActive(true);

            _right = rightSide;
            var anchor = rightSide ? Vector2.right : Vector2.zero;
            {
                m_SpeechBox.anchorMin = anchor;
                m_SpeechBox.anchorMax = anchor;
                m_SpeechBox.pivot = anchor;

                var leftPos = rightSide ? m_BoxOffset : m_ArrowOffset;
                var rightPos = rightSide ? m_ArrowOffset : m_BoxOffset;
                m_SpeechText.rectTransform.offsetMin = new Vector2(leftPos, m_SpeechText.rectTransform.offsetMin.y);
                m_SpeechText.rectTransform.offsetMax = new Vector2(-rightPos, m_SpeechText.rectTransform.offsetMax.y);

                float portraitSizeX = portraitDisplay.rectTransform.sizeDelta.x;
                float borderOffset = (rightSide ? -portrait.boxRightOffset : portrait.boxRightOffset);
                m_SpeechBox.sizeDelta = new Vector2((((RectTransform)transform.parent).rect.width + borderOffset - m_SpeechBoxBorderPadding - portraitSizeX), m_SpeechBox.sizeDelta.y);
                m_SpeechBox.anchoredPosition = new Vector2((rightSide ? -portraitSizeX : portraitSizeX) + borderOffset, m_SpeechBox.anchoredPosition.y);
            }
            {
                m_NameText.rectTransform.anchorMin = anchor;
                m_NameText.rectTransform.anchorMax = anchor;
                m_NameText.rectTransform.pivot = anchor;
                m_NameText.rectTransform.anchoredPosition = new Vector2(m_NameBorderXOffset * (rightSide ? -1.0f : 1.0f), m_NameText.rectTransform.anchoredPosition.y);
                m_NameText.alignment = rightSide ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            }
        }

        public override void ToggleBox(bool enabled) {
            m_NameText.gameObject.SetActive(enabled);
            base.ToggleBox(enabled);
        }
    }
}
