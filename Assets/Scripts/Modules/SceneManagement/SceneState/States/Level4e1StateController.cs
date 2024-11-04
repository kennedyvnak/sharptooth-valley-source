using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.LevelAssets.Level4;
using NFHGame.RangedValues;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Serialization;
using NFHGame.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static NFHGame.SceneManagement.SceneState.Level4StateController;

namespace NFHGame.SceneManagement.SceneState {
    public class Level4e1StateController : SceneStateController {
        public enum PuzzleState : byte { Empty, Full, Flooded }

        [Serializable]
        public class CarrancaLight {
            public Light2D light;
            public SpriteRenderer emission;
            public LightAsFire lightAsFire;
            public float alphaLit, alphaUnlit;
            public RangedFloat unlitRange;

            public Color defaultEmissionColor { get; set; }

            public void Setup(bool litLake) {
                defaultEmissionColor = emission.color;
                if (!litLake) {
                    lightAsFire.fireMinIntensity = unlitRange.min;
                    lightAsFire.fireMaxIntensity = unlitRange.max;
                }
            }

            public void SetIntensity(bool litLake) {
                var color = defaultEmissionColor;
                color.a = Mathf.InverseLerp(lightAsFire.fireMinIntensity, lightAsFire.fireMaxIntensity, light.intensity) * (litLake ? alphaLit : alphaUnlit);
                emission.color = color;
            }
        }

        private const string k_ToggledPuzzleDialogueGameKey = "dialogueToggledPuzzle";
        private const string k_FullDialogueGameKey = "dialogueToggledPuzzle";
        private const string k_SacrificeLocationDialogueGameKey = "sacrificeLocation";

        [System.Serializable]
        public class PuzzleKey {
            public string gameKey;
            public int value;
        }

        [SerializeField] private PuzzleKey[] m_Keys;
        [SerializeField] private int m_ActiveValue;

        [SerializeField] private GameObject[] m_Empty;
        [SerializeField] private GameObject[] m_Full;
        [SerializeField] private GameObject[] m_Flooded;
        [SerializeField] private GameObject[] m_FullOrFlooded;
        [SerializeField] private GameObject[] m_EmptyOrFull;
        [SerializeField] private GameObject[] m_ToggledPuzzle;

        [Header("Soundtrack")]
        [SerializeField] private ChangeSoundtrackOnStart m_SoundtrackController;
        [SerializeField] private AudioMusicObject m_IntoxicatingSoundtrack, m_DragonSoundtrack, m_AliveSoundtrack;

        [Header("Unlit Lake")]
        [SerializeField] private GlobalLight m_GlobalLight;
        [SerializeField] private GlobalLight m_ForegroundLight;
        [SerializeField] private SpriteRenderer[] m_UnlitLakeRenderers;
        [SerializeField] private Color m_UnlitLakeColor;
        [SerializeField] private SpriteRenderer m_ReflectionRenderer;
        [SerializeField] private Color m_ReflectionColor;
        [SerializeField] private Light2D[] m_LitLights;

        [Header("Dialogues")]
        [SerializeField] private SceneLoadAnchorWalkIn m_SceneLoadAnchor;
        [SerializeField] private ArticyRef m_ToggledPuzzleDialogue;
        [SerializeField] private ArticyRef m_LakeRaisedDialogue, m_LakeShallowDialogue;
        [SerializeField] private ArticyRef m_FirstTimeFloodedDialogue, m_FirstTimeEmptyDialogue, m_FirstTimeFullDialogue;
        [SerializeField] private ArticyRef m_SacrificeLocationDialogue;

        [Header("Show Chamber")]
        [SerializeField] private Transform m_AnimCamera;
        [SerializeField] private float m_EndCameraPos;
        [SerializeField] private float m_CameraMoveSpeed;

        [Header("Carranca Lights")]
        [SerializeField] private CarrancaLight[] m_CarrancaLights;

        private Coroutine _sacrificeLocationDialogueCoroutine;
        private int _chamberLakeLevel;
        private bool _litLake;
        private PuzzleState _puzzleState;

        protected override void Awake() {
            base.Awake();

            _chamberLakeLevel = 0;
            foreach (var key in m_Keys) {
                if (GameKeysManager.instance.HaveGameKey(key.gameKey))
                    _chamberLakeLevel += key.value;
            }
            _puzzleState = (PuzzleState)(_chamberLakeLevel < m_ActiveValue ? 0 : _chamberLakeLevel == m_ActiveValue ? 1 : 2);

            _litLake = GameKeysManager.instance.HaveGameKey(LitLakeKey);
            bool dragonAlive = GameKeysManager.instance.HaveGameKey(DragonAliveKey);
            m_SoundtrackController.soundtrack = dragonAlive ? m_AliveSoundtrack : (_litLake ? m_DragonSoundtrack : m_IntoxicatingSoundtrack);
        }

        private void Start() {
            switch (_puzzleState) {
                case PuzzleState.Empty:
                    ActiveList(m_Empty);
                    ActiveList(m_EmptyOrFull);
                    if (GameKeysManager.instance.HaveGameKey(PondPuzzleManager.ToggledPuzzleGameKey))
                        ActiveList(m_ToggledPuzzle);
                    break;
                case PuzzleState.Full:
                    ActiveList(m_Full);
                    ActiveList(m_FullOrFlooded);
                    ActiveList(m_EmptyOrFull);
                    break;
                case PuzzleState.Flooded:
                    ActiveList(m_Flooded);
                    ActiveList(m_FullOrFlooded);
                    break;
            }

            foreach (var carrancaLight in m_CarrancaLights) {
                carrancaLight.Setup(_litLake);
            }

            if (!_litLake) {
                foreach (var unlitRenderer in m_UnlitLakeRenderers) {
                    unlitRenderer.color = m_UnlitLakeColor;
                }
                m_ReflectionRenderer.color = m_ReflectionColor;
            }

            m_GlobalLight.Init(_litLake);
            m_ForegroundLight.Init(_litLake);

            foreach (var litLight in m_LitLights) {
                litLight.enabled = _litLake;
            }
        }

        private void Update() {
            foreach (var carrancaLight in m_CarrancaLights) {
                carrancaLight.SetIntensity(_litLake);
            }
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);

            GameData gameData = DataManager.instance.gameData;
            PuzzleState lastPuzzleState = gameData.lastPuzzleState;
            gameData.lastPuzzleState = _puzzleState;

            if (firstTimeInScene) {
                switch (_puzzleState) {
                    case PuzzleState.Flooded:
                        m_SceneLoadAnchor.onFinish.AddListener(() => ShowChamber());
                        break;
                    case PuzzleState.Empty:
                        m_SceneLoadAnchor.onFinish.AddListener(() => DialogueManager.instance.PlayDialogue(m_FirstTimeEmptyDialogue));
                        break;
                }
                return;
            } else if (_puzzleState == PuzzleState.Full && !GameKeysManager.instance.HaveGameKey(k_FullDialogueGameKey)) {
                GameKeysManager.instance.ToggleGameKey(k_FullDialogueGameKey, true);
                m_SceneLoadAnchor.onFinish.AddListener(() => DialogueManager.instance.PlayDialogue(m_FirstTimeFullDialogue));
                return;
            }

            if (!GameKeysManager.instance.HaveGameKey(k_ToggledPuzzleDialogueGameKey) && GameKeysManager.instance.HaveGameKey(PondPuzzleManager.ToggledPuzzleGameKey)) {
                if (!firstTimeInScene && _puzzleState == PuzzleState.Empty) {
                    GameKeysManager.instance.ToggleGameKey(k_ToggledPuzzleDialogueGameKey, true);
                    m_SceneLoadAnchor.onFinish.AddListener(() => DialogueManager.instance.PlayDialogue(m_ToggledPuzzleDialogue));
                    return;
                }
            }

            if (lastPuzzleState != _puzzleState) {
                var dialogue = lastPuzzleState < _puzzleState ? m_LakeRaisedDialogue : m_LakeShallowDialogue;
                m_SceneLoadAnchor.onFinish.AddListener(() => DialogueManager.instance.PlayDialogue(dialogue));
            }
        }

        private void ShowChamber() {
            var handler = DialogueManager.instance.CreateHandler();
            handler.onDialogueProcessGameTrigger += (gt) => {
                if (gt.Equals("cameraShowsChamber")) {
                    m_AnimCamera.transform.position = GameCharactersManager.instance.bastheet.transform.position;
                    Helpers.vCam.Follow = m_AnimCamera;
                    m_AnimCamera.DOMoveX(m_EndCameraPos, m_CameraMoveSpeed).SetSpeedBased(true).SetEase(Helpers.CameraInEase);
                }
            };
            handler.onDialogueFinished += () => {
                var fadeHandler = FadeScreen.instance.FadeFor(1.0f);
                fadeHandler.onFinishFadeIn += () => {
                    Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                    Helpers.vCam.PreviousStateIsValid = false;
                    fadeHandler.FadeOut();
                };
            };
            DialogueManager.instance.PlayHandler(m_FirstTimeFloodedDialogue, handler);
        }

        public void CheckCrystalSacrificed() {
            if (!GameKeysManager.instance.HaveGameKey(Level4StateController.LitLakeKey) && GameKeysManager.instance.HaveGameKey(Level4StateController.ThrowAlphaReefKey)) {
                GameKeysManager.instance.ToggleGameKey(Level4StateController.LitLakeKey, true);
                ArticyVariables.globalVariables.gameState.FirstTimeLitLake = true;
                ArticyVariables.globalVariables.gameState.litLake = true;
            }
        }

        public void EnterSacrificeLocation() {
            _sacrificeLocationDialogueCoroutine = StartCoroutine(SacrificeLocationDialogueCoroutine());
        }

        public void ExitSacrificeLocation() {
            StopCoroutine(_sacrificeLocationDialogueCoroutine);
        }

        private IEnumerator SacrificeLocationDialogueCoroutine() {
            var bast = GameCharactersManager.instance.bastheet;
            while (true) {
                if (bast.moveDirection == 0) {
                    GameKeysManager.instance.ToggleGameKey(k_SacrificeLocationDialogueGameKey, true);
                    DialogueManager.instance.PlayDialogue(m_SacrificeLocationDialogue);
                }
                yield return null;
            }
        }

        private void ActiveList(GameObject[] list) {
            foreach (var obj in list)
                obj.SetActive(true);
        }
    }
}
