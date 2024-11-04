using NFHGame.Characters;
using NFHGame.Characters.StateMachines;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class CrystalBox : DialogueBox {
        [SerializeField] private TextMeshProUGUI m_SpeechText;
        [SerializeField] private RectTransform m_SpeechBox;
        [SerializeField] private Button m_StepButton;
        [SerializeField] private float m_SpeechBoxLineSize, m_SpeechBoxSpacing, m_SpeechBoxLineOffset, m_SpeechBoxMinSize;

        private TextVertexAnimator _textVertexAnimator;
        private Coroutine _typeRoutine;

        private int _cachedBastAnimHash;
        private BastheetStateBase _cachedBastState;

        [System.NonSerialized] public bool hurtFlag;

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

        public void SetHurt(bool hurt) {
            if (hurt && _cachedBastAnimHash == 0) {
                var bastheet = GameCharactersManager.instance.bastheet;
                _cachedBastState = bastheet.stateMachine.currentState;
                _cachedBastAnimHash = bastheet.anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
                bastheet.stateMachine.animState.Animate(BastheetCharacterController.HeadacheAnimationHashes.GetAnimation(bastheet.facingDirection));
            } else if (!hurt && _cachedBastAnimHash != 0) {
                var bastheet = GameCharactersManager.instance.bastheet;
                if (_cachedBastState is BastheetAnimState animState) {
                    animState.Animate(_cachedBastAnimHash);
                } else {
                    bastheet.stateMachine.animState.Animate(_cachedBastAnimHash);
                    bastheet.stateMachine.EnterState(_cachedBastState);
                }
                _cachedBastAnimHash = 0;
                _cachedBastState = null;
            }
        }

        public override void ToggleBox(bool enabled) {
            if (!enabled)
                SetHurt(false);
            base.ToggleBox(enabled);
        }
    }
}
