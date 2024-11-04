using System.Collections.Generic;
using Articy.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NFHGame.UI.Input;
using NFHGame.DialogueSystem.Actors;
using NFHGame.DialogueSystem.Portraits;
using UnityEngine.Pool;
using NFHGame.Input;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.DialogueSystem.DialogueBoxes;
using Articy.SharptoothValley;

namespace NFHGame.DialogueSystem {
    [RequireComponent(typeof(ArticyFlowPlayer))]
    public class DialogueManager : Singleton<DialogueManager>, IArticyFlowPlayerCallbacks, IScriptMethodProvider {
        private static readonly int k_UiBlockKey = "Dialogue".GetHashCode();

        [Header("UI")]
        [SerializeField] private GameObject m_BastheetHalo;
        [SerializeField] private CanvasGroup m_MainGroup;
        [SerializeField] private CanvasGroup m_StepGroup;
        [SerializeField] private CanvasGroup m_BranchingGroup;
        [SerializeField] private float m_StepGroupAlpha;

        [Header("Boxes")]
        [SerializeField] private DialogueBoxWithActor m_DialogueBoxWithActor;
        [SerializeField] private NarratorBox m_NarratorBox;
        [SerializeField] private CrystalBox m_CrystalBox;
        [SerializeField] private RoboticBox m_RoboticBox;
        [SerializeField] private LynssJournalBox m_JournalBox;
        [SerializeField] private DialogueBranchBox m_DialogueBranchBox;
        [SerializeField] private DualCharactersBox m_DualCharactersBox;
        [SerializeField] private TripleCharactersBox m_TripleCharactersBox;

        [Header("Branches")]
        [SerializeField] private CanvasGroup m_BranchButtonsGroup;
        [SerializeField] private float m_BranchingInputDelay = 1.0f / 3.0f;

        [SerializeField] private float m_SkipDialogueTime = 0.3f;

        public DialogueExecutionEngine executionEngine { get; private set; }

        public event System.Action<bool> DialogueToggled;

        private DialogueBox _lastBox;
        private TextVertexAnimator _currentVertexAnimator;
        private float _startedTextTime;
        private Coroutine _typeRoutine;

        private ObjectPool<BranchButton> _branchButtonsPool;
        private List<BranchButton> _activeButtons;

        private bool _uiActive;

        public float branchingInputDelay { get => m_BranchingInputDelay; set => m_BranchingInputDelay = value; }
        public bool IsCalledInForecast { get; set; }

        protected override void Awake() {
            base.Awake();

            var flowPlayer = GetComponent<ArticyFlowPlayer>();
            var branchButtonScrollRectTransform = m_BranchButtonsGroup.GetComponent<RectTransform>();
            var branchButtonScrollRect = branchButtonScrollRectTransform.GetComponent<ScrollRect>();
            _activeButtons = new List<BranchButton>();
            _branchButtonsPool = new ObjectPool<BranchButton>(
                () => {
                    var button = Instantiate(m_DialogueBranchBox.branchButtonPrefab, m_DialogueBranchBox.branchButtonsParent);
                    button.OnMouseEnter += Branch_OnMouseEnter;
                    return button;
                },
                (button) => button.gameObject.SetActive(true),
                (button) => button.gameObject.SetActive(false));

            executionEngine = new DialogueExecutionEngine(this, flowPlayer, branchButtonScrollRectTransform, branchButtonScrollRect);
            executionEngine.OnUpdateActorSpeech += ENGINE_OnUpdateActorSpeech;
            executionEngine.OnUpdateNarrator += ENGINE_OnUpdateNarrator;
            executionEngine.OnUpdateCrystal += ENGINE_OnUpdateCrystal;
            executionEngine.OnUpdateRobotic += ENGINE_OnUpdateRobotic;
            executionEngine.OnUpdateJournal += ENGINE_OnUpdateJournal;
            executionEngine.OnUpdateDualCharacters += ENGINE_OnUpdateDualCharacters;
            executionEngine.OnUpdateTripleCharacters += ENGINE_OnUpdateTripleCharacters;
            executionEngine.OnBeforeBranching += ENGINE_BeforeBranching;
            executionEngine.OnCreateBranch += ENGINE_CreateBranch;
            executionEngine.OnSelectBranch += ENGINE_SelectBranch;
            executionEngine.OnCreateBranching += ENGINE_CreateBranching;
            executionEngine.OnProcessGameTrigger += ENGINE_ProcessGameTrigger;
            executionEngine.OnFinish += ENGINE_Finish;
            executionEngine.OnCanStepChanged += ENGINE_CanStepChanged;
        }

        private void OnEnable() {
            Input.InputReader.instance.OnStepDialogue += INPUT_StepDialogue;
            Input.InputReader.instance.OnDialogueNavigate += INPUT_DialogueNavigate;
        }

        private void OnDisable() {
            Input.InputReader.instance.OnStepDialogue -= INPUT_StepDialogue;
            Input.InputReader.instance.OnDialogueNavigate -= INPUT_DialogueNavigate;
        }

        private void Start() {
            ClearBranches();
            m_DialogueBoxWithActor.OnStepDialogue += INPUT_StepDialogue;
            m_DualCharactersBox.OnStepDialogue += INPUT_StepDialogue;
            m_TripleCharactersBox.OnStepDialogue += INPUT_StepDialogue;
            m_NarratorBox.OnStepDialogue += INPUT_StepDialogue;
            m_JournalBox.OnStepDialogue += INPUT_StepDialogue;
            m_RoboticBox.OnStepDialogue += INPUT_StepDialogue;
            m_CrystalBox.OnStepDialogue += INPUT_StepDialogue;
            m_DialogueBranchBox.OnStepDialogue += INPUT_StepDialogue;
        }

        public void ClearCache() {
            if (!executionEngine.ClearCache()) return;

            _activeButtons.Clear();
            m_DialogueBoxWithActor.ClearCache();
            m_DialogueBranchBox.ClearCache();
            m_NarratorBox.ClearCache();
            m_CrystalBox.ClearCache();
            m_DualCharactersBox.ClearCache();
            this.EnsureCoroutineStopped(ref _typeRoutine);
        }

        public DialogueHandler CreateHandler() => new DialogueHandler();

        public void PlayHandler(ArticyRef aRef, DialogueHandler handler) => PlayDialogueInternal(aRef, handler);

        public DialogueHandler PlayHandledDialogue(ArticyRef aRef) {
            PlayHandler(aRef, CreateHandler());
            return executionEngine.currentHandler;
        }

        public void PlayDialogue(ArticyRef aRef) {
            PlayDialogueInternal(aRef, null);
        }

        public void OnFlowPlayerPaused(IFlowObject aObject) => executionEngine.OnFlowPlayerPaused(aObject);

        public void OnBranchesUpdated(IList<Branch> aBranches) => executionEngine.OnBranchesUpdated(aBranches);

        public void SetRoboticState(int state) {
            m_RoboticBox.state = state;
        }

        private void PlayDialogueInternal(ArticyRef aRef, DialogueHandler handler) {
            OpenDialogue();
            executionEngine.Setup(aRef, handler);
        }

        private void OpenDialogue() {
            if (_uiActive) return;

            EventSystem.current.sendNavigationEvents = false;
            Input.InputReader.instance.PushMap(InputReader.InputMap.Dialogue | InputReader.InputMap.UI);
            if (UserInterfaceInput.instance) UserInterfaceInput.instance.SetInteractable(k_UiBlockKey, false);
            m_MainGroup.ToggleGroup(true);
            transform.GetChild(0).gameObject.SetActive(true);
            _uiActive = true;
            DialogueToggled?.Invoke(true);
        }

        private void ClearBranches() {
            for (int i = _activeButtons.Count - 1; i >= 0; i--) {
                BranchButton button = _activeButtons[i];
                _branchButtonsPool.Release(button);
            }
            _activeButtons.Clear();
        }

        private void OnFinishAnimation() => executionEngine.OnFinishAnimation();

        private void ToggleToBox(DialogueBox box) {
            if (_lastBox == box) return;
            box.ToggleBox(true);
            if (_lastBox) _lastBox.ToggleBox(false);
            _lastBox = box;
        }

        private void ENGINE_OnUpdateActorSpeech(DialogueActor actor, bool thinking, string name, string speech, Portrait portrait, bool rightSide) {
            m_BastheetHalo.SetActive(actor.actor == DialogueActor.Actor.Bastheet);
            m_DialogueBoxWithActor.Setup(name, portrait, rightSide, actor.dialogueFont);
            Setup(m_DialogueBoxWithActor, m_DialogueBoxWithActor.SetSpeech(speech, thinking, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
        }

        private void ENGINE_OnUpdateNarrator(string speech) {
            Setup(m_NarratorBox, m_NarratorBox.SetSpeech(speech, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
        }

        private void ENGINE_OnUpdateCrystal(string speech, bool hurt) {
            m_CrystalBox.SetHurt(hurt);
            Setup(m_CrystalBox, m_CrystalBox.SetSpeech(speech, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
        }

        private void ENGINE_OnUpdateRobotic(DialogueActor actor, string speech) {
            Setup(m_RoboticBox, m_RoboticBox.SetSpeech(speech, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
            m_RoboticBox.PlaySound(actor);
        }

        private void ENGINE_OnUpdateJournal(string speech, bool isMark) {
            Setup(m_JournalBox, m_JournalBox.SetSpeech(speech, isMark, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
        }

        private void ENGINE_OnUpdateDualCharacters(DialogueActor actorA, string nameA, Portrait portraitA, DialogueActor actorB, string nameB, Portrait portraitB, string speech) {
            m_DualCharactersBox.SetNames(nameA, nameB);
            m_DualCharactersBox.SetPortraits(portraitA, portraitB);
            Setup(m_DualCharactersBox, m_DualCharactersBox.SetSpeech(speech, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
        }

        private void ENGINE_OnUpdateTripleCharacters(DialogueActor actorA, string nameA, Portrait portraitA, DialogueActor actorB, string nameB, Portrait portraitB, DialogueActor actorC, string nameC, Portrait portraitC, string speech) {
            m_TripleCharactersBox.SetNames(nameA, nameB, nameC);
            m_TripleCharactersBox.SetPortraits(portraitA, portraitB, portraitC);
            Setup(m_TripleCharactersBox, m_TripleCharactersBox.SetSpeech(speech, OnFinishAnimation, out _currentVertexAnimator, out _typeRoutine));
        }

        private void Setup(DialogueBox box, bool drawing) {
            if (drawing)
                executionEngine.currentHandler?.onDialogueStartDraw?.Invoke();
            ToggleToBox(box);
            _startedTextTime = Time.time;
        }

        private void ENGINE_CreateBranch(Branch branch, string branchLabel, bool wasSelectedBefore, bool locked, bool selected) {
            var button = _branchButtonsPool.Get();
            m_DialogueBranchBox.CreateBranch(button, branch, branchLabel, wasSelectedBefore, locked, selected, executionEngine.SelectBranch);
            _activeButtons.Add(button);
        }

        private void ENGINE_CreateBranching() {
            m_BranchingGroup.alpha = m_StepGroupAlpha;
            m_DialogueBranchBox.CreateButtons(_activeButtons, executionEngine.branchButtonScrollRectTransform);
            m_BranchButtonsGroup.ToggleGroup(true);
            ToggleToBox(m_DialogueBranchBox);
            SelectBranch(executionEngine.inputSelectedBranch);
        }

        private void ENGINE_ProcessGameTrigger(GameTriggerProcessor.GameTriggerHandler handler) {
            m_MainGroup.ToggleGroup(false);
            transform.GetChild(0).gameObject.SetActive(false);
            handler.onReturnToDialogue.AddListener(() => {
                GameLogger.dialogue.Log("Processed the game trigger. Returning dialogue", LogLevel.Verbose);
                m_MainGroup.ToggleGroup(true);
                transform.GetChild(0).gameObject.SetActive(true);
                executionEngine.Step();
            });
        }

        private void ENGINE_SelectBranch(Branch branch) {
            m_BranchingGroup.alpha = 0.0f;
        }

        private void ENGINE_BeforeBranching() {
            m_StepGroup.alpha = 0.0f;
            m_BranchButtonsGroup.ToggleGroup(false);
            ClearBranches();
        }

        private void ENGINE_Finish() {
            if (_lastBox) {
                _lastBox.ToggleBox(false);
                _lastBox = null;
            }

            m_MainGroup.ToggleGroup(false);
            transform.GetChild(0).gameObject.SetActive(false);
            if (UserInterfaceInput.instance) UserInterfaceInput.instance.SetInteractable(k_UiBlockKey, true);
            InputReader.instance.PopMap(InputReader.InputMap.Dialogue | InputReader.InputMap.UI);
            EventSystem.current.sendNavigationEvents = true;
            _uiActive = false;
            DialogueToggled?.Invoke(false);
        }

        private void ENGINE_CanStepChanged(bool canStep) {
            m_StepGroup.alpha = canStep ? m_StepGroupAlpha : 0.0f;
        }

        private void INPUT_StepDialogue() {
            if (!executionEngine.running) return;

            if (_currentVertexAnimator?.textAnimating == true && Time.time - _startedTextTime >= m_SkipDialogueTime) {
                _currentVertexAnimator.SkipToEndOfCurrentText();
                return;
            }

            if (executionEngine.canStep) {
                _currentVertexAnimator.FinishAnimating(null);
                _lastBox.StopCoroutine(_typeRoutine);
            }

            executionEngine.InputStep();
        }

        private void INPUT_DialogueNavigate(int direction) {
            if (!executionEngine.isBranching || !executionEngine.running) return;

            executionEngine.Navigate(direction);
            SelectBranch(executionEngine.inputSelectedBranch);
        }

        private void SelectBranch(int idx) {
            m_DialogueBranchBox.SelectBranch(idx, executionEngine.branchButtonsScrollRect);
        }

        private void Branch_OnMouseEnter(BranchButton branch) {
            m_DialogueBranchBox.BranchMouseEnter(branch);
            executionEngine.inputSelectedBranch = executionEngine.currentBranches.IndexOf(branch.branch);
        }

        void IScriptMethodProvider.UnlockAchievement(string achievementKey) {
            if (!IsCalledInForecast)
                executionEngine.UnlockAchievement(achievementKey);
        }

        void IScriptMethodProvider.SetPortraitSide(string character, bool leftSide) {
            if (!IsCalledInForecast)
                executionEngine.SetPortraitSide(character, leftSide);
        }
    }
}
