using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.DinnerTrust;
using NFHGame.Input;
using NFHGame.SceneManagement;
using NFHGame.Serialization.States;
using NFHGame.Serialization;
using NFHGame.UI.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NFHGame.ArticyImpl;
using UnityEngine.Rendering.Universal;
using NFHGame.SceneManagement.GameKeys;
using TMPro;
using Cinemachine;

namespace NFHGame.SpammyEvents {
    public class JournalSpammyBattle : Singleton<JournalSpammyBattle> {
        public const string TrailDialogueKey = "trailDialogue";
        public const string BattleDialogueKey = "journalSpamBattle";

        [Header("Dialogues")]
        [SerializeField] private ArticyRef m_BattleRef;
        [SerializeField] private ArticyRef m_HoleRef;

        [Header("Environments")]
        [SerializeField] private GameObject m_HoleObject;
        [SerializeField] private GameObject m_HoleTrigger;
        [SerializeField] private GameObject m_JournalEnvObject;

        [Header("Positioning")]
        [SerializeField] private float m_SpammyPos;

        [Header("Boss Bar")]
        [SerializeField] private CanvasGroup m_SpammyBattleBar;
        [SerializeField] private Slider m_SpammyBarSlider;
        [SerializeField] private float m_BastHeadMoveDuration = 2.0f / 3.0f;

        [Header("Part 1")]
        [SerializeField] private float m_BattleBastheetPos;
        [SerializeField] private float m_DinnerGoesAheadTime;
        [SerializeField] private float m_BattleDinnerPos;
        [SerializeField] private float m_TrustMadnessRatio;
        [SerializeField] private float m_TrustMadnessReturnDelay;

        [Header("Spammy Leaves")]
        [SerializeField] private float m_SpamLeaveFaceBastheetPositionX;
        [SerializeField] private float m_SpamLeaveEndPositionX;
        [SerializeField] private float m_SpamLeaveFaceBastheetDuration;

        [Header("Soundtrack")]
        [SerializeField] private AudioSource m_MusicalCueSource;
        [SerializeField] private AudioMusicObject m_BattleMusic, m_Level3Music;

        [Header("Camera Control")]
        [SerializeField] private Transform m_TargetCamera;
        [SerializeField] private float m_PansDuration;
        [SerializeField] private Ease m_PansEase;
        [SerializeField] private float m_CameraPosition;

        [Header("Spammy")]
        [SerializeField] private float m_SpammyDrawsDuration;
        [SerializeField] private float m_SpammyDrawsReturnDuration;
        [SerializeField] private float m_RedEyeTurnDuration;
        [SerializeField] private float m_AvatarStateEndY;

        [Header("Arrow")]
        [SerializeField] private Vector2 m_GameOverArrowShotPosition;
        [SerializeField] private Vector2 m_GameOverArrowShotForce;
        [SerializeField] private ArrowBehaviour m_ArrowBehaviour;
        [SerializeField] private float m_SpammyShootDelay;

        [Header("Explosion")]
        [SerializeField] private CinemachineImpulseSource m_ExplosionImpulseSource;
        [SerializeField] private AudioObject m_ExplosionSound;
        [SerializeField] private Vector2 m_ExplosionEnd;
        [SerializeField] private Vector2 m_ExplosionDuration;
        [SerializeField] private Transform m_FrontExplosion, m_BackExplosion;
        [SerializeField] private float m_ExplosionXDelay;
        [SerializeField] private float m_ExplosionFrontDelay;
        [SerializeField] private Ease m_ExplosionEase;
        [SerializeField] private Image m_ExplosionScreenOverride;
        [SerializeField] private Material m_AfterExplosionMaterial;
        [SerializeField] private SpriteRenderer m_NergathEyes;
        [SerializeField] private float m_NergathEyesFadeDuration;
        [SerializeField] private float m_BastheetExplosionY;
        [SerializeField] private float m_BastheetFadeDuration;
        [SerializeField] private Light2D m_ForegroundLight;
        [SerializeField] private Vector3 m_NergathBastheetOffset;
        [SerializeField] private float m_IDontRememberDuration;
        [SerializeField] private float m_IDontRememberFadeDuration;

        [Header("Spammy Dinner Hug")]
        [SerializeField] private SpriteRenderer m_SpammyDinnerHugRenderer;
        [SerializeField] private float m_SpammyHugPosX;

        private GameTriggerProcessor.GameTriggerHandler _currentHandler;
        private DialogueHandler _battleHandler;
        private bool _explosion, _redEye;

        public void StartHoleDialogue() {
            m_HoleTrigger.SetActive(false);
            GameKeysManager.instance.ToggleGameKey(TrailDialogueKey, true);
            DialogueManager.instance.PlayDialogue(m_HoleRef);
        }

        public void SetupEnv() {
            m_HoleObject.SetActive(true);
            if (!GameKeysManager.instance.HaveGameKey(TrailDialogueKey))
                m_HoleTrigger.SetActive(true);
            m_JournalEnvObject.SetActive(false);
        }

        public void StartBattleDialogue() {
            GameKeysManager.instance.ToggleGameKey(BattleDialogueKey, true);

            var spammy = GameCharactersManager.instance.spammy;
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.InitSpammy(spammy);
            spammy.gameObject.SetActive(true);
            spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
            spammy.SetPositionX(m_SpammyPos, true);

            _battleHandler = DialogueManager.instance.PlayHandledDialogue(m_BattleRef);
            _battleHandler.onDialogueProcessGameTrigger += (trigger) => {
                if (trigger.Equals("musicalCue")) {
                    m_MusicalCueSource.Play();
                    ArticyManager.notifications.AddListener("trustPoints.dinnerPoints", SpamPointsChanged);
                    ShowBar();
                } else if (trigger.Equals("spammyJournalJoins")) {
                    spammy.stateMachine.EnterDefaultState();
                    GameManager.instance.ReloadSpammyInParty(true);
                    SoundtrackManager.instance.SetSoundtrack(m_Level3Music);

                    var dinner = GameCharactersManager.instance.dinner;
                    dinner.stateMachine.EnterDefaultState();
                    DisableBar();
                } else if (trigger.Equals("spammyLooksBack")) {
                    spammy.SetFacingDirection(-1);
                }
            };

            _battleHandler.onDialogueFinished += () => {
                if (!_explosion) {
                    ArticyVariables.globalVariables.items.lynnJournal = 1;

                    InputReader.instance.PushMap(InputReader.InputMap.None);

                    m_TargetCamera.DOMoveX(GameCharactersManager.instance.bastheet.transform.position.x, m_PansDuration).SetEase(m_PansEase).OnComplete(() => {
                        Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                        InputReader.instance.PopMap(InputReader.InputMap.None);
                        DataManager.instance.ClearSave();
                    });

                    ArticyManager.notifications.RemoveListener("trustPoints.dinnerPoints", SpamPointsChanged);
                }
            };
        }

        public void DinnerGoesAhead(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;
            StartCoroutine(DinnerGoesAhead());
        }

        public void TrustPointsMadness(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;
            DinnerTrustBarController.instance.TrustMadness(m_TrustMadnessRatio);
            DOVirtual.DelayedCall(m_TrustMadnessReturnDelay, handler.onReturnToDialogue.Invoke);

            SoundtrackManager.instance.SetSoundtrack(m_BattleMusic);
        }

        public void BastheetGoesInvestigate(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            GameKeysManager.instance.ToggleGameKey(ArrowSpammyBattle.SawSpammyGameKey, true);

            StartCoroutine(BastheetGoesInvestigate());
        }

        public void SpammyDrawsBow(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var bastheet = GameCharactersManager.instance.bastheet;
            var dinner = GameCharactersManager.instance.dinner;
            var spammy = GameCharactersManager.instance.spammy;

            dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.GetAnimation(true));

            spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingAlphaAAnimationHash);
            DOVirtual.DelayedCall(m_SpammyDrawsDuration, () => spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingAlphaBAnimationHash));

            bastheet.stateMachine.avatarState.EnterAvatarState(2, 0, m_AvatarStateEndY, false, false, false);
            bastheet.stateMachine.avatarState.StartLoop += AvatarState_FinishUp;

            DOVirtual.DelayedCall(m_SpammyDrawsReturnDuration, handler.onReturnToDialogue.Invoke);
        }

        public void SpammyBastSettle(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var bastheet = GameCharactersManager.instance.bastheet;
            var spammy = GameCharactersManager.instance.spammy;

            bastheet.stateMachine.avatarState.GoDownLikeEnter(true).OnComplete(handler.onReturnToDialogue.Invoke);
            bastheet.stateMachine.avatarState.StopSounds();

            spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingAlphaCAnimationHash);
            GameCharactersManager.instance.dinner.stateMachine.animState.Animate(DinnerCharacterController.DinnerIdleJournalAnimationHash);
        }

        public void SpamBastFire(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            StartCoroutine(DoExplosion());

            DOVirtual.DelayedCall(m_SpammyShootDelay, () => {
                GameCharactersManager.instance.spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingReleaseAnimationHash);
                var bastheetPos = GameCharactersManager.instance.bastheet.rb.position;
                m_ArrowBehaviour.Shoot(m_GameOverArrowShotForce, m_GameOverArrowShotPosition, true, false, () => {
                    GameCharactersManager.instance.bastheet.rb.position = bastheetPos;
                    GameCharactersManager.instance.bastheet.rb.velocity = Vector2.zero;
                });
            });
        }

        public void HeadacheStart(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.HeadacheAnimationHashes.GetAnimation(bastheet.facingDirection));
            handler.onReturnToDialogue.Invoke();
        }

        public void HeadacheStop(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.EnterDefaultState();
            handler.onReturnToDialogue.Invoke();
        }

        public void BastRedEye(GameTriggerProcessor.GameTriggerHandler handler) {
            _currentHandler = handler;

            var renderer = GameCharactersManager.instance.bastheet.GetComponent<SpriteRenderer>();
            var colorID = Shader.PropertyToID("_EmissionColor");

            DOVirtual.Color(new Color(0.0f, 0.0f, 0.0f, 0.0f), new Color(100.0f, 0.0f, 0.0f, 1.0f), m_RedEyeTurnDuration, (x) => {
                renderer.material.SetColor(colorID, x);
            }).OnComplete(handler.onReturnToDialogue.Invoke);
        }

        public void BastTurnsEvil(GameTriggerProcessor.GameTriggerHandler handler) { // RED AVATAR STATE
            _currentHandler = handler;

            var bastheet = GameCharactersManager.instance.bastheet;
            var spammy = GameCharactersManager.instance.spammy;

            StartCoroutine(SpammyDinnerHug());

            _redEye = true;
            bastheet.stateMachine.avatarState.EnterAvatarState(2, 0, false, false, false);
            bastheet.stateMachine.avatarState.StartLoop += () => {
                AvatarState_FinishUp();
                handler.onReturnToDialogue.Invoke();
            };
        }

        public void SecondImpact(GameTriggerProcessor.GameTriggerHandler handler) { // EXPLOSION
            _currentHandler = handler;

            StartCoroutine(DoExplosion());
        }

        public void SpammyJournalLeaves(GameTriggerProcessor.GameTriggerHandler handler) { // SPAMMY LEAVES
            _currentHandler = handler;

            StartCoroutine(SpammyLeaves());
        }

        private IEnumerator DoExplosion() {
            _explosion = true;

            var bastheet = GameCharactersManager.instance.bastheet;
            m_BackExplosion.position = bastheet.transform.position;
            m_FrontExplosion.position = bastheet.transform.position;

            SoundtrackManager.instance.StopSoundtrack();
            AudioPool.instance.PlaySound(m_ExplosionSound);

            if (!_redEye) {
                DOVirtual.DelayedCall(m_SpammyShootDelay * 1.5f, () => StartCoroutine(SpammyDinnerHug()));

                var renderer = GameCharactersManager.instance.bastheet.GetComponent<SpriteRenderer>();
                var colorID = Shader.PropertyToID("_EmissionColor");
                yield return DOVirtual.Color(new Color(0.0f, 0.0f, 0.0f, 0.0f), new Color(100.0f, 0.0f, 0.0f, 1.0f), m_ExplosionXDelay, (x) => renderer.material.SetColor(colorID, x)).WaitForCompletion();
            } else {
                yield return Helpers.GetWaitForSeconds(m_ExplosionXDelay);
            }

            Tween shakeTween = shakeTween = DOVirtual.DelayedCall(m_ExplosionImpulseSource.m_ImpulseDefinition.m_ImpulseDuration, m_ExplosionImpulseSource.GenerateImpulse).SetLoops(-1, LoopType.Restart);
            m_ExplosionImpulseSource.GenerateImpulse();

            Explosion(m_BackExplosion);
            DOVirtual.DelayedCall(m_ExplosionFrontDelay, () => Explosion(m_FrontExplosion));
            DisableBar();

            _battleHandler.onDialogueFinished += FinishExplosion;

            m_ExplosionScreenOverride.enabled = true;
            yield return m_ExplosionScreenOverride.DOFade(1.0f, 1.0f).SetDelay(m_ExplosionDuration.x + m_ExplosionXDelay).WaitForCompletion();

            shakeTween.Kill();
            CinemachineImpulseManager.Instance.Clear();

            DOTween.Kill(bastheet.stateMachine.avatarState);
            bastheet.stateMachine.avatarState.StopSounds();

            UserInterfaceInput.instance.buttonsGroup.ToggleGroup(false);
            HaloManager.HaloManager.instance.ForceToggle(false);
            DinnerTrustBarController.instance.canvasGroup.ToggleGroup(false);

            var backRenderer = m_BackExplosion.GetComponent<SpriteRenderer>();
            backRenderer.material = m_AfterExplosionMaterial;
            var frontRenderer = m_FrontExplosion.GetComponent<SpriteRenderer>();
            frontRenderer.material = m_AfterExplosionMaterial;

            var bastRenderer = bastheet.GetComponent<SpriteRenderer>();
            var tailRenderer = bastheet.transform.Find("Tail").GetComponent<SpriteRenderer>();
            var haloRenderer = bastheet.transform.Find("Halo").GetComponent<SpriteRenderer>();
            bastRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            tailRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            haloRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            bastheet.rb.position = new Vector2(m_TargetCamera.position.x, m_BastheetExplosionY);

            bastRenderer.sortingLayerID = frontRenderer.sortingLayerID;
            tailRenderer.sortingLayerID = frontRenderer.sortingLayerID;
            haloRenderer.sortingLayerID = frontRenderer.sortingLayerID;
            bastRenderer.sortingOrder = frontRenderer.sortingOrder + 10;
            tailRenderer.sortingOrder = frontRenderer.sortingOrder + 5;
            haloRenderer.sortingOrder = frontRenderer.sortingOrder + 1;

            m_ForegroundLight.intensity = 1.0f;

            m_ExplosionScreenOverride.color = Color.clear;
            m_ExplosionScreenOverride.enabled = false;

            yield return DOVirtual.Float(0.0f, 1.0f, m_BastheetFadeDuration, (x) => {
                var col = new Color(1.0f, 1.0f, 1.0f, x);
                bastRenderer.color = col;
                tailRenderer.color = col;
                haloRenderer.color = col;
            }).WaitForCompletion();

            m_NergathEyes.transform.position = bastRenderer.transform.position + m_NergathBastheetOffset;
            m_NergathEyes.DOFade(1.0f, m_NergathEyesFadeDuration);
            m_NergathEyes.gameObject.SetActive(true);

            _currentHandler.onReturnToDialogue.Invoke();
            ArticyManager.notifications.RemoveListener("trustPoints.dinnerPoints", SpamPointsChanged);

            void Explosion(Transform renderer) {
                renderer.DOScaleY(m_ExplosionEnd.y, m_ExplosionDuration.y).SetEase(m_ExplosionEase);
                renderer.DOScaleX(m_ExplosionEnd.x, m_ExplosionDuration.x).SetEase(m_ExplosionEase);
            }

            void FinishExplosion() {
                m_ExplosionScreenOverride.color = Color.clear;
                m_ExplosionScreenOverride.enabled = true;
                m_ExplosionScreenOverride.DOFade(1.0f, m_IDontRememberFadeDuration).OnComplete(() => {
                    var text = m_ExplosionScreenOverride.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    text.gameObject.SetActive(true);
                    text.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    text.DOFade(1.0f, m_IDontRememberFadeDuration);
                });

                DOVirtual.DelayedCall(m_IDontRememberDuration, () => {
                    var handler = SceneLoader.instance.CreateHandler(DataManager.instance.gameData.state.sceneRef, SceneStatesData.StateAnchorID);
                    handler.saveGame = false;
                    handler.blackScreen = true;
                    handler.StopInput();
                    ArticyManager.instance.notify = false;
                    DataManager.instance.userManager.ReloadUser();
                    ArticyManager.instance.notify = true;
                    SceneLoader.instance.LoadScene(handler);
                });
            }
        }

        private IEnumerator SpammyDinnerHug() {
            var spammy = GameCharactersManager.instance.spammy;
            var dinner = GameCharactersManager.instance.dinner;

            yield return spammy.WalkOut(m_SpammyHugPosX, -1, true);

            spammy.gameObject.SetActive(false);
            dinner.gameObject.SetActive(false);
            m_SpammyDinnerHugRenderer.gameObject.SetActive(true);
        }

        private IEnumerator DinnerGoesAhead() {
            DOVirtual.DelayedCall(m_DinnerGoesAheadTime, _currentHandler.onReturnToDialogue.Invoke);

            var dinner = GameCharactersManager.instance.dinner;
            yield return dinner.WalkOut(m_BattleDinnerPos, -1);
            dinner.stateMachine.animState.Animate(DinnerCharacterController.DinnerIdleJournalAnimationHash);
            dinner.SetFacingDirection(-1);
        }

        private IEnumerator BastheetGoesInvestigate() {
            var bastheet = GameCharactersManager.instance.bastheet;
            var giveSpace = ArticyVariables.globalVariables.bossBattle.giveSpace;
            if (!giveSpace) {
                InputReader.instance.PushMap(InputReader.InputMap.None);
                _currentHandler.onReturnToDialogue.Invoke();
            }
            yield return bastheet.WalkToPosition(m_BattleBastheetPos, faceDir: 1);

            SoundtrackManager.instance.SetSoundtrack(m_BattleMusic);

            if (giveSpace) {
                DinnerTrustBarController.instance.EndMadness();
                _currentHandler.onReturnToDialogue.Invoke();
            } else {
                DialogueManager.instance.executionEngine.Step();
                InputReader.instance.PopMap(InputReader.InputMap.None);
            }

            m_TargetCamera.position = GameCharactersManager.instance.bastheet.transform.position;
            Helpers.vCam.Follow = m_TargetCamera;
            yield return m_TargetCamera.DOMoveX(m_CameraPosition, m_PansDuration).SetEase(m_PansEase).WaitForCompletion();
        }

        private IEnumerator SpammyLeaves() {
            var spammy = GameCharactersManager.instance.spammy;
            var dinner = GameCharactersManager.instance.dinner;
            dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.GetAnimation(false));

            _battleHandler.onDialogueFinished += () => dinner.stateMachine.EnterDefaultState();

            yield return spammy.WalkOut(m_SpamLeaveFaceBastheetPositionX, -1);
            yield return Helpers.GetWaitForSeconds(m_SpamLeaveFaceBastheetDuration);
            SoundtrackManager.instance.SetSoundtrack(m_Level3Music);
            DisableBar();
            yield return spammy.WalkOut(m_SpamLeaveEndPositionX, -1, run: true);
            spammy.gameObject.SetActive(false);
            _currentHandler.onReturnToDialogue.Invoke();
        }

        private void SpamPointsChanged(string variable, object value) {
            m_SpammyBarSlider.DOValue(Mathf.Clamp((int)value, 0, 9.25f), m_BastHeadMoveDuration);
        }

        private void ShowBar() {
            m_SpammyBattleBar.gameObject.SetActive(true);
            m_SpammyBattleBar.DOFade(1.0f, m_BastHeadMoveDuration);
        }

        private void DisableBar() {
            m_SpammyBattleBar.DOFade(0.0f, m_BastHeadMoveDuration).OnComplete(() => m_SpammyBattleBar.gameObject.SetActive(false));
        }

        private void AvatarState_FinishUp() {
            var bastheet = GameCharactersManager.instance.bastheet;

            bastheet.stateMachine.avatarState.StartLoop -= AvatarState_FinishUp;
            DOTween.Kill(bastheet.stateMachine.avatarState);
        }
    }
}
