using Articy.Unity;
using DG.Tweening;
using NFHGame.AchievementsManagement;
using NFHGame.Animations;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.Interaction.Behaviours;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.SpammyEvents {
    public class ArrowSpammyBattle : Singleton<ArrowSpammyBattle> {
        public const string InArrowSpammyBattleID = "spamArrowBattle";
        public const string SawSpammyGameKey = "sawSpammy";

        [Header("General")]
        [SerializeField] private float m_ReturnCameraDuration;
        [SerializeField] private ArrowBehaviour m_ArrowBehaviour;

        [Header("Dialogues")]
        [SerializeField] private ArticyRef m_DialogueRef;
        [SerializeField] private InteractionObject m_BattleTrigger;

        [Header("Positioning")]
        [SerializeField] private float m_SpammyPos = 23.84f;
        [SerializeField] private float m_BastheetPos;
        [SerializeField] private float m_DinnerPos;
        [SerializeField] private float m_DinnerShiftsMoveDelay;
        [SerializeField] private float m_DinnerPosShifting;
        [SerializeField] private float m_DinnerShiftsDuration;

        [Header("Arrow Shoot")]
        [SerializeField] private Vector2 m_InitialArrowShotPosition;
        [SerializeField] private Vector2 m_InitialArrowShotForce;
        [SerializeField] private Transform m_ArrowPlim;

        [Header("Camera Pans")]
        [SerializeField] private float m_CharactersWalkDelay;
        [SerializeField] private Transform m_Target;
        [SerializeField] private float m_PansDuration;
        [SerializeField] private Ease m_PansEase;

        [Header("SurpriseMoment")]
        [SerializeField] private float m_SurpriseGetArrowDuration;
        [SerializeField] private AudioSource m_MusicalCueSource;

        [Header("Bow Tension")]
        [SerializeField] private Sprite[] m_BowTensionSprites;

        [Header("Arrow Game Over")]
        [SerializeField] private ArticyRef m_GameOverDialogue;
        [SerializeField] private Vector2 m_GameOverArrowShotPosition;
        [SerializeField] private Vector2 m_GameOverArrowShotForce;
        [SerializeField, TextArea] private string m_GameOverLabel;

        [Header("Spammy Leaves")]
        [SerializeField] private float m_SpamLeaveEndPositionX;
        [SerializeField] private float m_DinnerSpamLeaveEndPositionX;
        [SerializeField] private float m_SpammyLeavesReturnDelay;
        [SerializeField] private InteractionObject[] m_LeaveStopInteractions;
        [SerializeField] private ArticyRef m_TalkToDinnerReference;
        [SerializeField] private InteractionPlayDialogue m_TalkToDinnerInteraction;
        [SerializeField] private GameObject m_DontLeaveWithoutDinnerBlocks;

        [Header("Throw Rock")]
        [SerializeField, TextArea] private string m_RockGameOverLabel;
        [SerializeField] private AmnesiaRockThrow m_RockBehaviour;
        [SerializeField] private float m_HoldRockDelay, m_ThrowRockDelay;
        [SerializeField] private Vector2 m_RockOffset;
        [SerializeField] private Vector2 m_RockForce;

        [Header("Boss Bar")]
        [SerializeField] private GameObject m_SpammyBattleBar;
        [SerializeField] private Slider m_SpammyBarSlider;
        [SerializeField] private float m_SpamHeadMoveDuration = 2.0f / 3.0f;
        [SerializeField] private AudioProviderObject m_TrustDownSound, m_TrustUpSound;

        [Header("Soundtrack")]
        [SerializeField] private AudioMusicObject m_BattleMusic;
        [SerializeField] private AudioMusicObject m_Level3Music;

        private GameTriggerProcessor.GameTriggerHandler _currentHandler;
        private int _finishRevealCount;
        private bool _gotAlterArrow;
        private ArticyRef _defaultTalkToDinner;
        private int _oldPoints;

        private void Start() {
            m_BattleTrigger.onInteractorEnter.AddListener(InteractorEnterBattleTrigger);
        }

        public void InitBattle() {
            DataManager.instance.SaveCheckpoint(InArrowSpammyBattleID);
            m_BattleTrigger.gameObject.SetActive(true);
        }

        public void ShootInitialArrow() {
            var spammy = GameCharactersManager.instance.spammy;
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.EnterState(bastheet.stateMachine.kinematicState);

            bastheet.InitSpammy(spammy);
            spammy.SetPositionX(m_SpammyPos, true);
            spammy.gameObject.SetActive(true);
            spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingNormalBAnimationHash);

            m_ArrowBehaviour.Shoot(m_InitialArrowShotForce, m_InitialArrowShotPosition, false, false, () => {
                m_ArrowPlim.position = m_ArrowBehaviour.transform.position;
                m_ArrowPlim.gameObject.SetActive(true);
                HaloManager.HaloManager.instance.Toggle(true);
                var executionEngine = DialogueManager.instance.executionEngine;
                executionEngine.overridePortraitsSide[DialogueSystem.Actors.DialogueActor.Actor.Bastheet] = true;
                executionEngine.overridePortraitsSide[DialogueSystem.Actors.DialogueActor.Actor.Dinner] = true;
                executionEngine.overridePortraitsSide[DialogueSystem.Actors.DialogueActor.Actor.Spammy] = false;

                SoundtrackManager.instance.SetSoundtrack(m_BattleMusic);

                _oldPoints = ArticyVariables.globalVariables.trustPoints.spamPoints;
                ArticyManager.notifications.AddListener("trustPoints.spamPoints", SpamPointsChanged);
                m_SpammyBarSlider.value = ArticyVariables.globalVariables.trustPoints.spamPoints;

                Time.timeScale = 0.0f;
                var handler = DialogueManager.instance.PlayHandledDialogue(m_DialogueRef);
                handler.onDialogueFinished += () => {
                    ArticyManager.notifications.RemoveListener("trustPoints.spamPoints", SpamPointsChanged);
                    DialogueManager.instance.executionEngine.overridePortraitsSide.Clear();
                };
                handler.onDialogueStartDraw += ReturnTimeScale;
                void ReturnTimeScale() {
                    handler.onDialogueStartDraw -= ReturnTimeScale;
                    Time.timeScale = 1.0f;
                }
            });
        }

        public void SpammyReveal(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var posX = m_Target.position.x;
            m_Target.position = GameCharactersManager.instance.bastheet.transform.position;
            m_Target.DOMoveX(posX, m_PansDuration).SetEase(m_PansEase).OnComplete(FinishReveal);

            GameKeysManager.instance.ToggleGameKey(SawSpammyGameKey, true);

            Helpers.vCam.Follow = m_Target;

            DOVirtual.DelayedCall(m_CharactersWalkDelay, () => StartCoroutine(WalkBD()));

            IEnumerator WalkBD() {
                var bastCoroutine = StartCoroutine(GameCharactersManager.instance.bastheet.WalkToPosition(m_BastheetPos));
                yield return GameCharactersManager.instance.dinner.WalkOut(m_DinnerPos);
                yield return bastCoroutine;
                FinishReveal();
            }
        }

        public void DinnerShifts(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;
            StartCoroutine(DinnerShifts());

            IEnumerator DinnerShifts() {
                var dinner = GameCharactersManager.instance.dinner;

                dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.shit);
                yield return Helpers.GetWaitForSeconds(m_DinnerShiftsMoveDelay);

                yield return dinner.WalkOut(m_DinnerPosShifting, -1, run: true, animHash: DinnerCharacterController.RunAnimationHash.shit);
                dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.shit);

                yield return Helpers.GetWaitForSeconds(m_DinnerShiftsDuration);
                dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.normal);

                _currentHandler.onReturnToDialogue?.Invoke();
            }
        }

        public void SurpriseMoment(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            GetAlterArrow(_currentHandler.onReturnToDialogue.Invoke, true);
        }

        public void ArrowGameOver(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            DialogueManager.instance.executionEngine.Finish();

            if (!_gotAlterArrow) {
                GetAlterArrow(ShootArrow, false);
            } else {
                ShootArrow();
            }

            void ShootArrow() {
                GameCharactersManager.instance.spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingReleaseAnimationHash);
                var bastheet = GameCharactersManager.instance.bastheet;
                bastheet.stateMachine.EnterState(bastheet.stateMachine.kinematicState);
                m_ArrowBehaviour.Shoot(m_GameOverArrowShotForce, m_GameOverArrowShotPosition, true, true, () => {
                    GameManager.instance.GameOver(m_GameOverLabel);
                });
            }
        }

        public void SpammyLeaves(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            foreach (var interaction in m_LeaveStopInteractions) {
                interaction.enabled = false;
            }

            _defaultTalkToDinner = m_TalkToDinnerInteraction.dialogueReference;
            m_TalkToDinnerInteraction.dialogueReference = m_TalkToDinnerReference;
            m_TalkToDinnerInteraction.callbacks.onDialogueFinished.AddListener(FinishTalkToDinner);

            StartCoroutine(SpammyLeaves());

            IEnumerator SpammyLeaves() {
                DialogueManager.instance.executionEngine.overridePortraitsSide.Clear();

                var spammy = GameCharactersManager.instance.spammy;
                var dinner = GameCharactersManager.instance.dinner;

                dinner.shitDinner = true;

                var spammyCoroutine = StartCoroutine(spammy.WalkOut(m_SpamLeaveEndPositionX, animHash: SpammyCharacterController.RunCryingAnimationHash));
                var dinnerCoroutine = StartCoroutine(dinner.WalkOut(m_DinnerSpamLeaveEndPositionX));

                yield return dinnerCoroutine;
                dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.shit);

                yield return spammyCoroutine;
                spammy.gameObject.SetActive(false);

                yield return Helpers.GetWaitForSeconds(m_SpammyLeavesReturnDelay);
                m_SpammyBattleBar.SetActive(false);
                dinner.shitDinner = false;
                SoundtrackManager.instance.SetSoundtrack(m_Level3Music);
                dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.normal);
                m_DontLeaveWithoutDinnerBlocks.SetActive(true);
                _currentHandler.onReturnToDialogue.Invoke();
            }
        }

        public void SpammyJoins(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var spammy = GameCharactersManager.instance.spammy;
            var dinner = GameCharactersManager.instance.dinner;

            spammy.stateMachine.EnterState(spammy.stateMachine.followState);
            dinner.stateMachine.EnterState(dinner.stateMachine.followState);

            DataManager.instance.ClearSave();
            handler.onReturnToDialogue.Invoke();

            m_SpammyBattleBar.SetActive(false);
            SoundtrackManager.instance.SetSoundtrack(m_Level3Music);

            ReturnGameplay();
        }

        public void AmensiaHamster(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.PickAmensiaRockAnimHash);
            DOVirtual.DelayedCall(m_HoldRockDelay, () => {
                bastheet.stateMachine.animState.Animate(BastheetCharacterController.ThrowAmensiaRockAnimHash);
                DOVirtual.DelayedCall(m_ThrowRockDelay, () => {
                    m_RockBehaviour.gameObject.SetActive(true);
                    GameManager.instance.forceOverrideGameOverLabel = m_RockGameOverLabel;
                    m_RockBehaviour.Shoot(m_RockForce, (Vector2)bastheet.transform.position + m_RockOffset, () => {
                        bastheet.stateMachine.EnterDefaultState();
                        _currentHandler.onReturnToDialogue.Invoke();
                    });
                });
            });
        }

        public void FinishTalkToDinner() {
            foreach (var interaction in m_LeaveStopInteractions) {
                interaction.enabled = true;
            }
            m_DontLeaveWithoutDinnerBlocks.SetActive(false);

            var dinner = GameCharactersManager.instance.dinner;

            m_TalkToDinnerInteraction.callbacks.onDialogueFinished.RemoveListener(FinishTalkToDinner);
            m_TalkToDinnerInteraction.dialogueReference = _defaultTalkToDinner;

            dinner.stateMachine.EnterState(dinner.stateMachine.followState);
            DataManager.instance.ClearSave();

            ReturnGameplay();
        }

        private void ReturnGameplay() {
            InputReader.instance.PushMap(InputReader.InputMap.None);
            m_Target.DOMoveX(GameCharactersManager.instance.bastheet.transform.position.x, m_ReturnCameraDuration).OnComplete(() => {
                Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                InputReader.instance.PopMap(InputReader.InputMap.None);
                m_ArrowBehaviour.sRender.enabled = false;
                m_ArrowBehaviour.boxCollider.enabled = false;
            });
        }

        private void FinishReveal() {
            _finishRevealCount++;
            if (_finishRevealCount >= 2) {
                m_SpammyBattleBar.SetActive(true);
                _currentHandler.onReturnToDialogue?.Invoke();
            }
        }

        private void InteractorEnterBattleTrigger(Interactor interactor) {
            m_BattleTrigger.gameObject.SetActive(false);
            ShootInitialArrow();
        }

        private void GetAlterArrow(System.Action onFinish, bool musicalCue) {
            var spammy = GameCharactersManager.instance.spammy;

            spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingAlphaAAnimationHash);
            if (musicalCue) m_MusicalCueSource.Play();
            DOVirtual.DelayedCall(m_SurpriseGetArrowDuration, () => {
                spammy.spammyStateMachine.arrowBattleSpammy.Setup(m_BowTensionSprites);
                spammy.stateMachine.EnterState(spammy.spammyStateMachine.arrowBattleSpammy);
                _gotAlterArrow = true;
                onFinish?.Invoke();
            });
        }

        private void SpamPointsChanged(string variable, object value) {
            int points = (int)value;
            m_SpammyBarSlider.DOValue(Mathf.Clamp(points, 0, 9.25f), m_SpamHeadMoveDuration);
            if (points < 1)
                DialogueManager.instance.executionEngine.ForceSetDialogue(m_GameOverDialogue);
            if (points < _oldPoints)
                AudioPool.instance.PlaySound(m_TrustDownSound);
            else if (points > _oldPoints)
                AudioPool.instance.PlaySound(m_TrustUpSound);

            _oldPoints = points;
        }
    }
}
