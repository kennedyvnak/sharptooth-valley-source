using Articy.Unity;
using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Battle;
using NFHGame.DialogueSystem.Actors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class RoboticBox : DialogueBox {
        [System.Serializable]
        public struct MachineDialogue {
            public AudioProviderObject sound;
            public float stopInputTime;
            public string animation;
        }

        [SerializeField] private TextMeshProUGUI m_SpeechText;
        [SerializeField] private RectTransform m_SpeechBox;
        [SerializeField] private Button m_StepButton;
        [SerializeField] private float m_SpeechBoxLineSize, m_SpeechBoxSpacing, m_SpeechBoxLineOffset, m_SpeechBoxMinSize;

        [Header("Dragon Battle")]
        [SerializeField] private MachineDialogue[] m_Dialogues;
        [SerializeField] private AudioSource m_DialogueSource;

        private TextVertexAnimator _textVertexAnimator;
        private Coroutine _typeRoutine;
        private int _dialogueCount;

        public int state { get; set; } = 0; // 1: dragon;

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

        public void PlaySound(DialogueActor actor) {
            if (state == 1) {
                var anim = m_Dialogues[_dialogueCount];

                anim.sound.CloneToSource(m_DialogueSource);
                m_DialogueSource.Play();
                if (!string.IsNullOrEmpty(anim.animation)) {
                    BattleManager.instance.shipCanon.SetMood(Animator.StringToHash(anim.animation));
                }

                var executionEngine = DialogueManager.instance.executionEngine;
                executionEngine.OnCanStepChanged += ExecutionEngine_OnCanStepChanged;

                DialogueManager.instance.executionEngine.SetCanStep(false);
                DOVirtual.DelayedCall(anim.stopInputTime, () => {
                    executionEngine.OnCanStepChanged -= ExecutionEngine_OnCanStepChanged;
                    DialogueManager.instance.executionEngine.SetCanStep(true);
                });

                _dialogueCount++;
            }
        }

        private void ExecutionEngine_OnCanStepChanged(bool canStep) {
            if (canStep)
                DialogueManager.instance.executionEngine.SetCanStep(false);
        }
    }
}
