using Articy.Unity;
using Cinemachine;
using DG.Tweening;
using NFHGame.AchievementsManagement;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem.GameTriggers.Triggers;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.Interaction.Behaviours;
using NFHGame.RangedValues;
using NFHGame.SceneManagement;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedFinalPositions : GameTriggerBase {
        [System.Serializable]
        public class FinalLight {
            public Light2D light;
            public RangedFloat intensity;

            public void SetIntensityRange(float range) => light.intensity = Mathf.LerpUnclamped(intensity.min, intensity.max, range);
        }

        public const string FinalPositionsID = "finalPositions";

        private GameTriggerProcessor.GameTriggerHandler _handler;

        [Header("Positions")]
        [SerializeField] private Transform m_FinalPositionsCamera;
        [SerializeField] private float m_PositioningDelay, m_SpammyFP, m_DinnerFP, m_BastheetArtPos, m_SpammyBeforeReturnBastheetOffset, m_SpammyReturnBastheetOffset, m_FinalPositionsCameraPos;

        [Header("Corridor")]
        [SerializeField] private GameObject m_CorridorParent;
        [SerializeField] private GameObject[] m_DisableCorridor;

        [Header("Sunlgiht")]
        [SerializeField] private float m_SunlightDuration;
        [SerializeField] private Ease m_SunlightEase;
        [SerializeField] private float m_GlobalLightIntensity, m_ForegroundLightIntensity;
        [SerializeField] private Light2D m_GlobalLight, m_ForegroundLight;

        [Header("Final Run")]
        [SerializeField] private CinemachineImpulseSource m_CameraShakeImpulse;
        [SerializeField] private AudioMusicObject m_FinalRunMusic;
        [SerializeField] private AudioProviderObject m_CaveQuakeSound;
        [SerializeField] private AnimationCurve m_RockSpawnCurve;
        [SerializeField] private float m_RockSpawmRate, m_FakeRockSpawnRate;
        [SerializeField, RangedValue(-100.0f, 100.0f)] private RangedFloat m_RockRangeX;
        [SerializeField, RangedValue(-100.0f, 100.0f)] private RangedFloat m_RockSpawnY;
        [SerializeField] private float m_CameraOffsetX, m_SpawnRangeX, m_MaxRockAmountTime, m_MaxRunTime, m_GameOverTime;
        [SerializeField] private float m_DinnerExitX;
        [SerializeField, TextArea] private string m_GameOverTimeText, m_GameOverDinnerText;
        [SerializeField] private SceneReference m_EndingScene;
        [SerializeField] private AchievementObject m_FinalRunAchievement;

        [Header("Final light")]
        [SerializeField] private FinalLight[] m_GlobalLights;
        [SerializeField] private AnimationCurve m_FinalLightsCurve;
        [SerializeField] private RangedFloat m_PosRange;
        [SerializeField] private Ease m_FinalLightEase;
        [SerializeField] private float m_FinalLightDuration, m_FinalLightPosX;

        [Header("Environment")]
        [SerializeField] private GameObject m_ExitObject;
        [SerializeField] private InteractionObject m_AmnesiaRock, m_Carving, m_Entry, m_SpammyInteraction, m_DinnerInteraction;
        [SerializeField] private ArticyRef m_SpammyDialogue, m_DinnerDialogue, m_CarvingDialogue;
        [SerializeField] private BigRock m_EntryRock, m_ExitRock;

        private Tween _cameraShakeTween;
        private ArticyRef _carvingDialogueCache, _dinnerDialogueCache, _spammyDialogueCache;
        private bool _bastheetExit, _endGame, _stopFinalLights;

        public override bool Match(string id) {
            return id switch {
                "finalPositions" => true,
                "finalArt" => true,
                "spammyComesBack" => true,
                "sunlightAtLast" => true,
                "finalShake" => true,
                "finalRun" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _handler = handler;

            switch (id) {
                case "finalPositions":
                    StartCoroutine(FinalPositions());
                    return true;
                case "finalArt":
                    StartCoroutine(FinalArt());
                    return true;
                case "spammyComesBack":
                    StartCoroutine(SpammyComesBack());
                    return true;
                case "sunlightAtLast":
                    SunlightAtLast();
                    return true;
                case "finalShake":
                    FinalShake();
                    return true;
                case "finalRun":
                    FinalRun();
                    return true;
                default:
                    return false;
            }
        }

        public void ExitTunnel() {
            StartCoroutine(ExitTunnelCoroutine());
        }

        public void StartFinalPositions() {
            var dinner = GameCharactersManager.instance.dinner;
            var spammy = GameCharactersManager.instance.spammy;

            spammy.SetPositionX(m_SpammyFP, false);
            spammy.stateMachine.animState.Animate(SpammyCharacterController.ThinkAnimationHash);

            dinner.SetPositionX(m_DinnerFP, false);
            dinner.ToggleLookBack(true);

            SetupInteractions();
        }

        private void OnDestroy() {
            _cameraShakeTween?.Kill();
        }

        private IEnumerator FinalPositions() {
            DataManager.instance.SaveCheckpoint(FinalPositionsID);

            StartCoroutine(SpammyMovement());

            var dinner = GameCharactersManager.instance.dinner;
            var dinnerCoroutine = StartCoroutine(dinner.WalkOut(m_DinnerFP, -1));

            SetupInteractions();
            m_DinnerInteraction.enabled = false;

            yield return Helpers.GetWaitForSeconds(m_PositioningDelay);

            ReturnDialogue();

            yield return dinnerCoroutine;
            m_DinnerInteraction.enabled = true;
            dinner.ToggleLookBack(true);
        }

        private void SetupInteractions() {
            SetDialogueCache(ref _spammyDialogueCache, m_SpammyDialogue, m_SpammyInteraction);
            var carvingDialogue = SetDialogueCache(ref _carvingDialogueCache, m_CarvingDialogue, m_Carving);
            carvingDialogue.lookBack = false;
            SetDialogueCache(ref _dinnerDialogueCache, m_DinnerDialogue, m_DinnerInteraction);

            var carvingClick = m_Carving.GetComponent<InteractionClick>();
            carvingClick.walkToThisOnClick = false;
            carvingClick.needsInteractor = false;

            if (m_AmnesiaRock) {
                m_AmnesiaRock.GetComponent<GameKeyListener>().enabled = false;
                m_AmnesiaRock.enabled = false;
            }

            m_ExitObject.SetActive(true);
            m_Entry.enabled = false;

            static InteractionPlayDialogue SetDialogueCache(ref ArticyRef cache, ArticyRef dialogueRef, InteractionObject obj) {
                var interaction = obj.GetComponent<InteractionPlayDialogue>();
                cache = interaction.dialogueReference;
                interaction.dialogueReference = dialogueRef;
                return interaction;
            }
        }

        private IEnumerator SpammyMovement() {
            var spammy = GameCharactersManager.instance.spammy;
            yield return spammy.WalkOut(m_SpammyFP, -1);
            spammy.stateMachine.animState.Animate(SpammyCharacterController.ThinkAnimationHash);
            spammy.SetFacingDirection(-1);
        }

        private IEnumerator FinalArt() {
            var bastheet = GameCharactersManager.instance.bastheet;

            m_FinalPositionsCamera.transform.position = bastheet.transform.position;
            m_FinalPositionsCamera.transform.DOMoveX(m_FinalPositionsCameraPos, bastheet.moveSpeed).SetSpeedBased(true).SetEase(Helpers.CameraInEase);
            Helpers.vCam.Follow = m_FinalPositionsCamera;

            yield return bastheet.WalkToPosition(m_BastheetArtPos, faceDir: 1);
            bastheet.ToggleLookBack(true);
            ReturnDialogue();
        }

        private IEnumerator SpammyComesBack() {
            var bastheet = GameCharactersManager.instance.bastheet;
            var spammy = GameCharactersManager.instance.spammy;

            spammy.SetPositionX(bastheet.transform.position.x + m_SpammyBeforeReturnBastheetOffset);
            yield return spammy.WalkOut(bastheet.transform.position.x + m_SpammyReturnBastheetOffset, faceDir: 1);

            if (m_AmnesiaRock) {
                m_AmnesiaRock.GetComponent<GameKeyListener>().enabled = true;
                m_AmnesiaRock.enabled = true;
            }
            m_Carving.GetComponent<InteractionPlayDialogue>().dialogueReference = _carvingDialogueCache;
            m_Carving.GetComponent<InteractionClick>().walkToThisOnClick = true;
            m_Carving.GetComponent<InteractionClick>().needsInteractor = true;
            m_Carving.GetComponent<InteractionPlayDialogue>().lookBack = true;
            m_DinnerInteraction.GetComponent<InteractionPlayDialogue>().dialogueReference = _dinnerDialogueCache;
            m_SpammyInteraction.GetComponent<InteractionPlayDialogue>().dialogueReference = _spammyDialogueCache;
            m_ExitObject.SetActive(false);
            m_Entry.enabled = true;

            GameCharactersManager.instance.dinner.ToggleLookBack(false);
            GameCharactersManager.instance.dinner.SetFacingDirection(-1);
            GameCharactersManager.instance.bastheet.ToggleLookBack(false);
            GameCharactersManager.instance.bastheet.SetFacingDirection(false);

            m_FinalPositionsCamera.transform.DOMoveX(bastheet.transform.position.x, bastheet.moveSpeed).SetSpeedBased(true).SetEase(Helpers.CameraInEase).OnComplete(() => Helpers.vCam.Follow = bastheet.transform);

            DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += DataManager.instance.ClearSave;

            ReturnDialogue();
        }

        private void SunlightAtLast() {
            var dinner = GameCharactersManager.instance.dinner;
            dinner.SetBattleOffset(true);
            dinner.ToggleLookBack(false);

            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.SetFacingDirection(false);
            bastheet.ToggleLookBack(false);
            m_FinalPositionsCamera.transform.DOMoveX(bastheet.transform.position.x + m_CameraOffsetX, bastheet.moveSpeed).SetSpeedBased(true).SetEase(Helpers.CameraInEase);

            float global = m_GlobalLight.intensity;
            float foreground = m_ForegroundLight.intensity;
            DOVirtual.Float(0.0f, 1.0f, m_SunlightDuration, (x) => {
                m_GlobalLight.intensity = Mathf.Lerp(global, m_GlobalLightIntensity, x);
                m_ForegroundLight.intensity = Mathf.Lerp(foreground, m_ForegroundLightIntensity, x);
            }).OnComplete(ReturnDialogue).SetEase(m_SunlightEase);
            m_Carving.Disable();
        }

        private void FinalShake() {
            m_DinnerInteraction.enabled = false;

            m_EntryRock.gameObject.SetActive(true);
            SoundtrackManager.instance.SetSoundtrack(m_FinalRunMusic);
            AudioPool.instance.PlaySound(m_CaveQuakeSound);
            _cameraShakeTween = DOVirtual.DelayedCall(m_CameraShakeImpulse.m_ImpulseDefinition.m_ImpulseDuration, () => {
                m_CameraShakeImpulse.GenerateImpulse();
            }).SetLoops(-1, LoopType.Restart);
            m_CameraShakeImpulse.GenerateImpulse();
            DOVirtual.DelayedCall(m_CameraShakeImpulse.m_ImpulseDefinition.m_ImpulseDuration, ReturnDialogue);
        }

        private void FinalRun() {
            m_CorridorParent.SetActive(true);
            foreach (var corridor in m_DisableCorridor) {
                corridor.SetActive(false);
            }
            GameCharactersManager.instance.dinner.shitDinner = true;
            GameCharactersManager.instance.spammy.gameObject.SetActive(false);

            Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
            Helpers.vCam.GetComponent<CinemachineCameraOffset>().m_Offset.x = m_CameraOffsetX;

            StartCoroutine(FinalRunCoroutine());
            ReturnDialogue();
        }

        private void ReturnDialogue() {
            _handler.onReturnToDialogue.Invoke();
        }

        private IEnumerator FinalRunCoroutine() {
            float elapsedTime = 0.0f;
            float rockRate = 0.0f;
            float fakeRockRate = 0.0f;

            bool bigRock = false;

            while (true) {
                float rockAmountNormalized = Mathf.InverseLerp(0.0f, m_MaxRockAmountTime, elapsedTime);
                float rockFactor = m_RockSpawnCurve.Evaluate(rockAmountNormalized);
                rockRate += rockFactor * Time.deltaTime;
                fakeRockRate += rockFactor * Time.deltaTime;

                if (rockRate >= m_RockSpawmRate) {
                    rockRate = 0.0f;
                    SpawnRock(false);
                }

                if (fakeRockRate >= m_FakeRockSpawnRate) {
                    fakeRockRate = 0.0f;
                    SpawnRock(true);
                }

                if (elapsedTime >= m_MaxRunTime) {
                    if (!bigRock) {
                        GameCharactersManager.instance.dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.GetAnimation(true));
                        m_ExitRock.gameObject.SetActive(true);
                        bigRock = true;
                    }
                    if (elapsedTime >= m_GameOverTime && !_endGame) {
                        if (_bastheetExit) {
                            InputReader.instance.PopMap(InputReader.InputMap.None);
                            GameManager.instance.GameOver(m_GameOverDinnerText);
                        } else {
                            GameManager.instance.GameOver(m_GameOverTimeText);
                        }
                        _endGame = true;
                    }
                }

                if (!_stopFinalLights) {
                    float intensityRange = GetBastheetProgressionCurve();
                    foreach (var light in m_GlobalLights)
                        light.SetIntensityRange(intensityRange);
                }

                yield return null;

                elapsedTime += Time.deltaTime;
            }
        }

        private IEnumerator ExitTunnelCoroutine() {
            var dinner = GameCharactersManager.instance.dinner;
            var bastheet = GameCharactersManager.instance.bastheet;

            _bastheetExit = true;

            InputReader.instance.PushMap(InputReader.InputMap.None);

            StartCoroutine(GameCharactersManager.instance.bastheet.WalkToPosition(GameCharactersManager.instance.bastheet.transform.position.x - 10.0f, true, -1, true));

            while (!_endGame) {
                if (m_DinnerExitX >= dinner.transform.position.x)
                    _endGame = true;
                yield return null;
            }

            AchievementsManager.instance.UnlockAchievement(m_FinalRunAchievement);

            _stopFinalLights = true;
            yield return DOVirtual.Float(GetBastheetProgressionCurve(), m_FinalLightPosX, m_FinalLightDuration, (x) => {
                foreach (var light in m_GlobalLights)
                    light.SetIntensityRange(x);
            }).SetEase(m_FinalLightEase).WaitForCompletion();

            var handler = SceneLoader.instance.CreateHandler(m_EndingScene, "caveExitEnding");
            handler.saveGame = false;
            handler.blackScreen = true;
            InputReader.instance.PopMap(InputReader.InputMap.None);
            handler.StopInput();
            SceneLoader.instance.LoadScene(handler);
        }

        private void SpawnRock(bool isFakeRock) {
            var size = RockProvider.instance.GetRandomSize();
            var pos = GetRandomPosition();
            if (!m_RockRangeX.Contains(pos.x)) return;
            var rock = RockProvider.instance.SpawnRock(RockProvider.instance.GetRandomRock(size), size, pos, isFakeRock, transform);
            rock.fakeBroke = true;
        }

        private Vector3 GetRandomPosition() {
            var bastheetPosX = GameCharactersManager.instance.bastheet.transform.position.x;

            bool left = Random.value > 0.5f;
            var posX = bastheetPosX + (left ? -Random.value * m_SpawnRangeX : Random.value * m_SpawnRangeX);

            return new Vector3(posX, m_RockSpawnY.RandomRange(), 0.0f);
        }

        private float GetBastheetProgressionCurve() {
            float intensityRange = Mathf.InverseLerp(m_PosRange.min, m_PosRange.max, GameCharactersManager.instance.bastheet.transform.position.x);
            return m_FinalLightsCurve.Evaluate(intensityRange);
        }
    }
}
