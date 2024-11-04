using System.Collections;
using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.HaloManager;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.Interaction.Behaviours;
using NFHGame.Interaction.Input;
using NFHGame.LevelAssets.Level5;
using NFHGame.RangedValues;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Screens;
using NFHGame.Serialization.States;
using NFHGame.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame.SceneManagement.SceneState {
    public class Level5StateController : SceneStateController {
        public const string CreditsID = "credits";
        private const string k_FirstTimeRooting = "firstTimeRooting";

        public static bool StopSoundtrack = false;

        [SerializeField] private Animator m_ShipAnimator;
        [SerializeField] private Animator m_HeartAnimator;

        [SerializeField] private Level5FrameControl m_StartFrame;
        [SerializeField] private Level5FrameControl m_TopFrame;

        [SerializeField] private Light2D[] m_GlobalLights;

        [Header("Click Controller")]
        [SerializeField] private LayerMask m_IgnoreClicksLayer;
        [SerializeField, RangedValue(-1000.0f, 1000.0f)] private RangedFloat m_RaycastRange, m_RaycastHeightRange;
        [SerializeField] private LayerMask m_GroundLayer;
        [SerializeField] private Interactor m_Interactor;

        [SerializeField] private SceneReference m_Level4Reference;
        [SerializeField] private string m_Level4LoadAnchor;

        [Header("First Time")]
        [SerializeField] private Level5FrameControl m_WombControl;
        [SerializeField] private Level5FrameControl m_HeartControl;
        [SerializeField] private InteractionLoadScene m_RootUpInteraction;
        [SerializeField] private InteractionObject m_RootDownInteraction;
        [SerializeField] private ArticyRef m_FirstTimeRootingRef;
        [SerializeField] private ArticyRef m_FirstTimeAlive, m_FirstTimeDead, m_FirstTimeWomb, m_FirstTimeHeart, m_FirstTimeAwaken;

        [Header("Heart Interaction")]
        [SerializeField] private InteractionPlayDialogue m_HeartInteraction;
        [SerializeField] private ArticyRef m_ActiveCoreDialogue, m_DeadCoreDialogue;

        [Header("Spammy")]
        [SerializeField] private GameObject[] m_SpammyObjects;

        [Header("Soundtrack")]
        [SerializeField] private ChangeSoundtrackOnStart m_Soundtrack;
        [SerializeField] private AudioMusicObject m_STRessurrectDragon;
        [SerializeField] private AudioPlayer[] m_BaseSources;
        
        [Header("Credits")]
        [SerializeField] private Animator m_CreditsObject;
        [SerializeField] private GameObject[] m_PostCreditsDisableObjects;
        [SerializeField] private float m_CreditsTime, m_PostCreditsDialogueDelay, m_PostCreditsFadeDelay;
        [SerializeField] private Animator m_PostCreditsArkenAnimator;
        [SerializeField] private GameObject m_PostCreditsArken;
        [SerializeField] private SceneReference m_TitleScreenSceneRef;
        [SerializeField] private ArticyRef m_PostCreditsRef;

        private InputClickController _clickController;

        protected override void Awake() {
            base.Awake();
            if (GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey)) {
                m_Soundtrack.soundtrack = m_STRessurrectDragon;
            }

            if (StopSoundtrack) {
                m_Soundtrack.enabled = false;
                StopSoundtrack = false;
            }
        }

        private void Start() {
            bool isDragonAlive = GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey);
            m_ShipAnimator.Play(isDragonAlive ? "shipALIVE" : "shipDEAD");
            if (isDragonAlive) {
                foreach (var light in m_GlobalLights) {
                    var haloListener = light.GetComponent<HaloLightIntensityListener>();
                    haloListener.intensityWhenEnabled += 0.1f;
                    haloListener.intensityWhenDisabled = haloListener.intensityWhenEnabled - 0.1f;
                }
            }

            _clickController = new InputClickController(m_IgnoreClicksLayer, m_RaycastRange, m_RaycastHeightRange, m_GroundLayer);

            if (!ArticyVariables.globalVariables.gameState.spamInParty) {
                foreach (var spam in m_SpammyObjects)
                    spam.SetActive(false);
            }

            m_RootUpInteraction.validation = (point) => {
                if (!GameKeysManager.instance.HaveGameKey(k_FirstTimeRooting)) {
                    GameKeysManager.instance.ToggleGameKey(k_FirstTimeRooting, true);
                    DialogueManager.instance.PlayDialogue(m_FirstTimeRootingRef);
                    return false;
                }
                return true;
            };

            m_RootDownInteraction.onInteractorPointClick.AddListener((point) => {
                if (!GameKeysManager.instance.HaveGameKey(k_FirstTimeRooting)) {
                    GameKeysManager.instance.ToggleGameKey(k_FirstTimeRooting, true);
                    DialogueManager.instance.PlayDialogue(m_FirstTimeRootingRef);
                    return;
                }
                Level5CameraController.instance.SetFrame(m_HeartControl);
            });
        }

        private void OnEnable() {
            InputReader.instance.OnMouseClick += INPUT_OnClick;
        }

        private void OnDisable() {
            InputReader.instance.OnMouseClick -= INPUT_OnClick;
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);

            switch (handler.anchorID) {
                case "dragonMouth":
                case SceneStatesData.StateAnchorID:
                    Level5CameraController.instance.StartFrame(m_StartFrame);
                    handler.ResumeInput();
                    if (firstTimeInScene)
                        DialogueManager.instance.PlayDialogue(GameKeysManager.instance.HaveGameKey(Level4StateController.DestroyedDragonKey) ? m_FirstTimeDead : m_FirstTimeAlive);
                    break;
                case "level6Roots":
                    Level5CameraController.instance.StartFrame(m_TopFrame);
                    handler.ResumeInput();
                    break;
                case CreditsID:
                    handler.ResumeInput();
                    StartCoroutine(CreditsSetup());
                    break;
            }

            m_HeartInteraction.dialogueReference = GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey) ? m_ActiveCoreDialogue : m_DeadCoreDialogue;

            Level5CameraController.instance.OnSetFrame += EVENT_SetFrame;
        }

        private IEnumerator CreditsSetup() {
            InputReader.instance.PushMap(InputReader.InputMap.None);

            Level5CameraController.instance.StartFrame(m_TopFrame);
            foreach (var obj in m_PostCreditsDisableObjects)
                obj.SetActive(false);
            m_PostCreditsArkenAnimator.gameObject.SetActive(true);

            m_ShipAnimator.Play("shipALIVE");
            foreach (var light in m_GlobalLights) {
                var haloListener = light.GetComponent<HaloLightIntensityListener>();
                haloListener.intensityWhenEnabled += 0.1f;
                haloListener.intensityWhenDisabled = haloListener.intensityWhenEnabled - 0.1f;
            }

            m_CreditsObject.gameObject.SetActive(true);
            m_CreditsObject.Play("Credits");
            yield return Helpers.GetWaitForSeconds(m_CreditsTime);
            m_Soundtrack.enabled = true;
            m_PostCreditsArkenAnimator.enabled = true;
            m_PostCreditsArken.SetActive(true);

            yield return Helpers.GetWaitForSeconds(m_PostCreditsDialogueDelay);
            DialogueManager.instance.PlayHandledDialogue(m_PostCreditsRef).onDialogueFinished += () => {
                DOVirtual.DelayedCall(m_PostCreditsFadeDelay, () => FadeScreen.instance.FadeFor(m_PostCreditsFadeDelay).onFinishFadeIn += () => {
                    var handler = SceneLoader.instance.CreateHandler(m_TitleScreenSceneRef, "exitFromGameplay");
                    handler.blackScreen = true;
                    handler.saveGame = false;
                    InputReader.instance.PopMap(InputReader.InputMap.None);
                    handler.StopInput();
                    SceneLoader.instance.LoadScene(handler);
                });
            };
        }

        public void CheckAwakeDragon() {
            bool isDragonAlive = GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey);
            m_HeartAnimator.SetInteger("HeartLevel", isDragonAlive ? 4 : 0);
        }

        public void AwakeDragon() {
            bool isDragonAlive = GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey);
            m_ShipAnimator.Play(isDragonAlive ? "shipALIVE" : "shipDEAD");
            m_HeartAnimator.SetInteger("HeartLevel", isDragonAlive ? 4 : 0);
        }

        public void CheckHeartBaseSounds(bool inHeart) {
            if (GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey) && inHeart) {
                foreach (var baseSource in m_BaseSources) {
                    baseSource.Play();
                }
            }
        }

        public void PlayHeartBaseSound(int soundIndex) {
            m_BaseSources[soundIndex].Play();
        }

        public void SetRessurrectDragonMusic() {
            SoundtrackManager.instance.SetSoundtrack(m_STRessurrectDragon);
        }

        public void StopHeartBaseSounds() {
            for (int i = 0; i < m_BaseSources.Length; i++) {
                AudioSource sound = m_BaseSources[i].source;
                if (!sound.isPlaying) break;
                sound.DOFade(0.0f, 1.0f).OnComplete(() => {
                    sound.Stop();
                });
            }
        }

        public void LoadLevel4() {
            var handler = SceneLoader.instance.CreateHandler(m_Level4Reference, m_Level4LoadAnchor);
            handler.charactersToLeftSide = true;
            SceneLoader.instance.LoadScene(handler);
        }

        private void INPUT_OnClick(Vector2 screenPosition) {
            _clickController.Raycast(m_Interactor.point, screenPosition);
        }

        private void EVENT_SetFrame(Level5FrameControl control) {
            static bool contains(string key) => GameKeysManager.instance.HaveGameKey(key);
            static void play(ArticyRef aRef) {
                if (!aRef.ValidStart()) return;

                var handler = DialogueManager.instance.PlayHandledDialogue(aRef);
                handler.onDialogueFinished += () => {
                    InputReader.instance.PopMap(InputReader.InputMap.None);
                };
            }

            if (control == m_WombControl && contains(HeartGame.RessurrectDragonGameKey)) {
                AwakeDragon();
                play(m_FirstTimeAwaken);
            } else if (control == m_WombControl) {
                play(m_FirstTimeWomb);
            } else if (control == m_HeartControl) {
                play(m_FirstTimeHeart);
            }
        }
    }
}
