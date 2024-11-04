using System.Collections;
using Articy.Unity;
using DG.Tweening;
using NFHGame.AchievementsManagement;
using NFHGame.DialogueSystem;
using NFHGame.RangedValues;
using NFHGame.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NFHGame.SceneManagement.SceneState {
    public class TitleScreenStateController : SceneStateController {
        [System.Serializable]
        public struct FadeObject {
            public SpriteRenderer renderer;
            [RangedValue(0.0f, 1.0f)] public RangedFloat fade;
            public float duration;
            public float delay;
        }

        [SerializeField] private Animator m_AnimCamera;
        [SerializeField] private Vector3 m_EndPosition;
        [SerializeField] private FadeObject[] m_FadeObjects;
        [SerializeField] private CanvasGroup m_ClickToEnterGroup;
        [SerializeField] private Button m_ClickToEnterButton;
        [SerializeField] private AudioSource m_ClickToEnterSound;
        [SerializeField] private float m_GroupFadeDuration;
        [SerializeField] private float m_GroupFadeFadeDelay;

        [Header("Cave Exit")]
        [SerializeField] private ArticyRef m_CaveExitDialogue;
        [SerializeField] private Transform m_CameraAnim;
        [SerializeField] private GameObject m_Characters;
        [SerializeField] private float m_CaveExitCamAnimTime, m_CaveExitCamPosition, m_CaveExitCamDelay;
        [SerializeField] private Ease m_CaveExitCamEase;
        [SerializeField] private GameObject[] m_DisableOnCaveExit;
        [SerializeField] private SceneReference m_PostCreditsScene;

        [Header("Version")]
        [SerializeField] private TextMeshProUGUI m_VersionText;

        private void Start() {
            m_VersionText.text = string.Format(m_VersionText.text, GameLogger.gameVersion);

            if (SceneLoader.instance.currentSceneKey != null) return;

            m_AnimCamera.enabled = true;
            foreach (var fadeObject in m_FadeObjects) {
                var color = fadeObject.renderer.color;
                color.a = fadeObject.fade.min;
                fadeObject.renderer.color = color;
                fadeObject.renderer.DOFade(fadeObject.fade.max, fadeObject.duration).SetDelay(fadeObject.delay);
            }

            EventSystem.current.SetSelectedGameObject(m_ClickToEnterButton.gameObject);
            m_ClickToEnterButton.onClick.AddListener(() => {
                m_ClickToEnterGroup.ToggleGroupAnimated(false, m_GroupFadeDuration);
                m_ClickToEnterSound.Play();
                ScreenManager.instance.PushScreen(TitleScreen.instance);
            });

            m_ClickToEnterGroup.ToggleGroupAnimated(true, m_GroupFadeDuration).SetDelay(m_GroupFadeFadeDelay);
        }

        protected override void OnDestroy() {
            foreach (var fadeObject in m_FadeObjects)
                fadeObject.renderer.DOKill();
            base.OnDestroy();
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);
            if (handler.anchorID == "caveExitEnding") {
                handler.ResumeInput();
                CaveExitEnding();
                return;
            }

            m_AnimCamera.transform.position = m_EndPosition;
            handler.ResumeInput();
            ScreenManager.instance.PushScreen(TitleScreen.instance);
            GameManager.instance.Resume();
            GameManager.instance.playTimeCouting = false;
        }

        private void CaveExitEnding() {
            m_AnimCamera.transform.position = m_EndPosition;
            GameManager.instance.Resume();
            m_Characters.SetActive(true);

            foreach (var obj in m_DisableOnCaveExit)
                obj.SetActive(false);

            var handler = DialogueManager.instance.PlayHandledDialogue(m_CaveExitDialogue);
            handler.onDialogueFinished += () => {
                m_CameraAnim.DOMoveX(m_CaveExitCamPosition, m_CaveExitCamAnimTime).SetEase(m_CaveExitCamEase).OnComplete(() => {
                    DOVirtual.DelayedCall(m_CaveExitCamDelay, () => {
                        var handler = SceneLoader.instance.CreateHandler(m_PostCreditsScene, CrystalDreamStateController.PostCreditsID);
                        handler.blackScreen = true;
                        handler.saveGame = false;
                        SceneLoader.instance.LoadScene(handler);
                    });
                });
            };
        }
    }
}