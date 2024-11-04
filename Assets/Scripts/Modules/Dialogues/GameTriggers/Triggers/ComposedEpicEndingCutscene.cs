using UnityEngine;
using NFHGame.LevelAssets.Level6.EpicEnding;
using UnityEngine.UI;
using NFHGame.AudioManagement;
using NFHGame.UI;
using DG.Tweening;
using NFHGame.UI.Input;
using NFHGame.DinnerTrust;
using Cinemachine;
using NFHGame.PostProcessing;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using NFHGame.Input;
using NFHGame.AchievementsManagement;
using NFHGame.Characters;
using System.Collections;
using NFHGame.SceneManagement;
using NFHGame.SceneManagement.SceneState;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedEpicEndingCutscene : GameTriggerBase {
        [SerializeField] private float m_FadeDuration;

        [Header("Final slideshow")]
        [SerializeField] private SubtitleManager.Subtitle m_BlackScreenSubtitle;
        [SerializeField] private AudioMusicObject m_AfterVideoMusic;
        [SerializeField] private SubtitleManager.Subtitle m_IntroScreenSubtitle;
        [SerializeField] private SubtitleManager.Subtitle m_FloatingInDarknessSubtitle;
        [SerializeField] private SubtitleManager.Subtitle m_CampfireSubtitle;
        [SerializeField] private SubtitleManager.Subtitle m_CampfireOnlyDinnerSubtitle;
        [SerializeField] private SubtitleManager.Subtitle m_AfterCampfireSubtitle;

        [Header("Forgive Yourself")]
        [SerializeField] private float m_ForgiveYourselfDuration;

        [Header("Outside the Ship")]
        [SerializeField] private AchievementObject m_EpicEndingAchievement;
        [SerializeField] private float m_OutsideShipFadeDelay;
        [SerializeField] private float m_ShotOutReturnDelay, m_ShotOutFadeDelay;
        [SerializeField] private SceneReference m_CreditsScene;

        private VideoPlayerHandler _videoPlayer;
        private RawImage _videoImage;
        private IntroScreenSceneControl _introScreenControl;
        private EpicEndingSceneControl _floatingInDarknessControl;
        private CampfireControl _campfireControl;

        private GameTriggerProcessor.GameTriggerHandler _triggerHandler;
        private FadeScreen.FadeHandler _fadeHandler;

        private List<Light2D> _globalLights;

        private bool _setup = false;

        public override bool Match(string id) {
            return id switch {
                "startSlideshow" => true,
                "introScreen" => true,
                "floatingInDarkness" => true,
                "campfireScene" => true,
                "forgiveYourself" => true,
                "outsideTheShip" => true,
                "outsideShotOut" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _triggerHandler = handler;
            switch (id) {
                case "startSlideshow":
                    StartSlideshow();
                    return true;
                case "introScreen":
                    IntroScreen();
                    return true;
                case "floatingInDarkness":
                    FloatingInTheDarkness();
                    return true;
                case "campfireScene":
                    CampfireScene();
                    return true;
                case "forgiveYourself":
                    StartCoroutine(ForgiveYourself());
                    return true;
                case "outsideTheShip":
                    OutsideTheShip();
                    return true;
                case "outsideShotOut":
                    OutsideShotOut();
                    return true;
                default:
                    return false;
            };
        }

        public void SetupScene() {
            if (_setup) return;

            var ui = Instantiate(Resources.Load<GameObject>("EpicEnding/EpicEndingUI"), ScreenCanvas.instance.transform);
            var world = Instantiate(Resources.Load<GameObject>("EpicEnding/EpicEnding"), transform);

            _videoImage = ui.transform.GetChild(0).GetComponent<RawImage>();
            _videoPlayer = world.transform.GetChild(0).GetComponent<VideoPlayerHandler>();

            var confiner = Helpers.vCam.GetComponent<CinemachineConfiner2D>();
            var volume = GammaController.instance.GetComponent<UnityEngine.Rendering.Volume>();

            var lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
            _globalLights = new List<Light2D>();
            foreach (var light in lights) {
                if (light.lightType == Light2D.LightType.Global)
                    _globalLights.Add(light);
            }

            var controlParent = world.transform.GetChild(1);
            InitControl(ref _introScreenControl, 0);
            InitControl(ref _floatingInDarknessControl, 1);
            InitControl(ref _campfireControl, 2);

            _setup = true;

            void InitControl<T>(ref T control, int childIndex) where T : EpicEndingSceneControl {
                control = controlParent.GetChild(childIndex).GetComponent<T>();
                control.Init(this, confiner, volume);
            }
        }

        public void ToggleSceneLights(bool enabled) {
            _globalLights.ForEach(light => light.gameObject.SetActive(enabled));
        }

        private void StartSlideshow() {
            SetupScene();

            if (GameCharactersManager.instance)
                GameCharactersManager.instance.bastheet.stateMachine.avatarState.StopSounds();

            _fadeHandler = FadeScreen.instance.FadeFor(m_FadeDuration);
            SoundtrackManager.instance.StopSoundtrack();
            _fadeHandler.onFinishFadeIn += () => {
                SubtitleManager.instance.StartSubtitle(m_BlackScreenSubtitle, () => {
                    _videoPlayer.PlayVideo(() => {
                        _videoImage.gameObject.SetActive(true);
                        _videoImage.DOFade(1.0f, m_FadeDuration);
                    });
                    _videoPlayer.videoFinished.AddListener(EVENT_VideoFinished);
                });
            };
        }

        private void IntroScreen() {
            SubtitleManager.instance.ToggleBackground(true);
            FadeOutAndSubtitle(_introScreenControl, m_IntroScreenSubtitle, () => FadeAndStep(_introScreenControl));
        }

        private void FloatingInTheDarkness() {
            FadeOutAndSubtitle(_floatingInDarknessControl, m_FloatingInDarknessSubtitle, () => FadeAndStep(_floatingInDarknessControl));
        }

        private void CampfireScene() {
            FadeOutAndSubtitle(_campfireControl, m_CampfireSubtitle, DisappearBastheet);

            void DisappearBastheet() {
                _campfireControl.FadeBastheet(m_FadeDuration);
                SubtitleManager.instance.StartSubtitle(m_CampfireOnlyDinnerSubtitle, DarkScreen);
            }

            void DarkScreen() {
                SubtitleManager.instance.ToggleBackground(false);
                _fadeHandler = FadeScreen.instance.FadeFor(m_FadeDuration);
                _fadeHandler.onFinishFadeIn += () => {
                    SubtitleManager.instance.StartSubtitle(m_AfterCampfireSubtitle, () => {
                        _triggerHandler.onReturnToDialogue.Invoke();
                    });
                };
            }
        }

        private IEnumerator ForgiveYourself() {
            var text = SubtitleManager.instance.forgiveYourselfText;
            var vertexAnimator = new TextVertexAnimator(text);
            text.alpha = 1.0f;
            var commands = DialogueUtility.ProcessInputString(text.text, out var message);
            var textCoroutine = StartCoroutine(vertexAnimator.AnimateTextIn(commands, message, null));
            yield return Helpers.GetWaitForSeconds(m_ForgiveYourselfDuration);
            StopCoroutine(textCoroutine);
            text.DOFade(0.0f, 2.0f);
            _campfireControl.gameObject.SetActive(false);
            _fadeHandler.FadeOut();
            _fadeHandler.onFinishFadeOut += () => {
                _triggerHandler.onReturnToDialogue.Invoke();
            };
        }

        private void OutsideTheShip() {
            _fadeHandler = FadeScreen.instance.FadeFor(m_FadeDuration);
            _fadeHandler.onFinishFadeIn += () => {
                _introScreenControl.gameObject.SetActive(true);
                _introScreenControl.SetOutsideShip();
                _fadeHandler.FadeOut();
                DOVirtual.DelayedCall(m_OutsideShipFadeDelay, () => _triggerHandler.onReturnToDialogue.Invoke());
            };
        }

        private void OutsideShotOut() {
            DOVirtual.DelayedCall(m_ShotOutReturnDelay, () => {
                _triggerHandler.onReturnToDialogue.Invoke();
                InputReader.instance.PushMap(InputReader.InputMap.None);
                _introScreenControl.OutsideShotOut().OnComplete(() => {
                    AchievementsManager.instance.UnlockAchievement(m_EpicEndingAchievement);
                    DOVirtual.DelayedCall(m_ShotOutFadeDelay, () => {
                        _fadeHandler = FadeScreen.instance.FadeFor(m_FadeDuration);
                        _fadeHandler.onFinishFadeIn += () => {
                            Level5StateController.StopSoundtrack = true;
                            var handler = SceneLoader.instance.CreateHandler(m_CreditsScene, "credits");
                            handler.blackScreen = true;
                            handler.saveGame = false;
                            SceneLoader.instance.LoadScene(handler);
                        };
                    });
                });
            });
        }

        private void EVENT_VideoFinished() {
            UserInterfaceInput.instance.gameObject.SetActive(false);
            DinnerTrustBarController.instance.gameObject.SetActive(false);
            Interaction.InteractionPointIconManager.instance.gameObject.SetActive(false);

            _videoPlayer.videoFinished.RemoveListener(EVENT_VideoFinished);
            _videoImage.DOFade(0.0f, m_FadeDuration).OnComplete(() => {
                _videoImage.enabled = false;
                SoundtrackManager.instance.SetSoundtrack(m_AfterVideoMusic);
                _triggerHandler.onReturnToDialogue.Invoke();
            });
        }

        private void FadeOutAndSubtitle(EpicEndingSceneControl control, SubtitleManager.Subtitle subtitle, System.Action onFinishSubtitle) {
            control.gameObject.SetActive(true);
            _fadeHandler.FadeOut();
            _fadeHandler.onFinishFadeOut += () => {
                SubtitleManager.instance.StartSubtitle(subtitle, onFinishSubtitle);
            };
        }

        private void FadeAndStep(EpicEndingSceneControl control) {
            _fadeHandler = FadeScreen.instance.FadeFor(m_FadeDuration);
            _fadeHandler.onFinishFadeIn += () => {
                control.gameObject.SetActive(false);
                _triggerHandler.onReturnToDialogue.Invoke();
            };
        }
    }
}
