using System.Collections;
using DG.Tweening;
using NFHGame.SceneManagement;
using NFHGame.Serialization;
using NFHGame.Serialization.States;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.Screens {
    public class TitleScreen : Singleton<TitleScreen>, IScreen {
        [SerializeField] private Button m_NewGameButton, m_LoadGameButton, m_ContinueButton, m_OptionsButton, m_ExitButton, m_CreditsButton, m_AboutNFHButton;

        [Space]
        [SerializeField] private CanvasGroup m_TitleScreenGroup;

        [Space]
        [SerializeField] private SceneReference m_CrystalDreamSceneRef;

        [SerializeField] private string m_AboutNfhUrl;

        [Space]
        [SerializeField, TextArea] private string m_NewGameWarning;
        [SerializeField] private float m_NewGameSureDelay;

        bool IScreen.screenActive { get; set; }

        bool IScreen.poppedByInput => false;
        bool IScreen.dontSelectOnActive => false;

        GameObject IScreen.selectOnOpen => m_NewGameButton.gameObject;

        private void Start() {
            m_NewGameButton.onClick.AddListener(PerformNewGame);
            m_LoadGameButton.onClick.AddListener(PerformLoadGame);
            m_ContinueButton.onClick.AddListener(PerformContinue);
            m_OptionsButton.onClick.AddListener(PerformOptions);
            m_ExitButton.onClick.AddListener(PerformExit);
            m_CreditsButton.onClick.AddListener(PerformCredits);
            m_AboutNFHButton.onClick.AddListener(PerformAboutNFH);

            UpdateLoadButtons(SavesScreen.instance.hasAnyGameData);
            if (PlatformManager.currentPlatform == PlatformManager.Platform.WebGL)
                m_ExitButton.interactable = false;

            m_NewGameButton.SetNavigation(up: m_AboutNFHButton, down: m_ContinueButton.interactable ? m_ContinueButton : m_OptionsButton);
            m_ContinueButton.SetNavigation(up: m_NewGameButton, down: m_OptionsButton, right: m_LoadGameButton);
            m_LoadGameButton.SetNavigation(up: m_NewGameButton, down: m_OptionsButton, left: m_ContinueButton);
            m_OptionsButton.SetNavigation(up: m_ContinueButton.interactable ? m_ContinueButton : m_NewGameButton, down: m_ExitButton);
            m_ExitButton.SetNavigation(up: m_OptionsButton, down: m_CreditsButton);
            m_CreditsButton.SetNavigation(up: m_ExitButton, down: m_AboutNFHButton);
            m_AboutNFHButton.SetNavigation(up: m_CreditsButton, down: m_NewGameButton);            
        }

        public void UpdateLoadButtons(bool canLoad) {
            m_LoadGameButton.interactable = canLoad;
            m_ContinueButton.interactable = canLoad;
        }

        private void PerformNewGame() {
            ScreenManager.instance.ForcePopAll();
            DataManager.instance.userManager.SetUser(UserManager.MaxUserId + 1);
            var handler = SceneLoader.instance.CreateHandler(m_CrystalDreamSceneRef, string.Empty);
            handler.blackScreen = true;
            handler.saveGame = false;
            SceneLoader.instance.LoadScene(handler);
        }

        private void PerformLoadGame() {
            SavesScreen.instance.GetUser(false, (user) => {
                if (user == -1) return;
                ScreenManager.instance.ForcePopAll();
                DataManager.instance.userManager.SetUser(user);
                var state = DataManager.instance.gameData.state;
                var handler = string.IsNullOrEmpty(state.sceneRef.scenePath) ? SceneLoader.instance.CreateHandler(m_CrystalDreamSceneRef, string.Empty) : SceneLoader.instance.CreateHandler(state.sceneRef, SceneStatesData.StateAnchorID);
                handler.blackScreen = true;
                handler.saveGame = false;
                SceneLoader.instance.LoadScene(handler);
                handler.StopInput();
            });
        }

        private void PerformContinue() {
            m_TitleScreenGroup.interactable = false;
            ScreenManager.instance.ForcePopAll();
            DataManager.instance.userManager.SetUser(DataManager.instance.GetLatestData(SavesScreen.instance.cachedGameData).userId);
            var state = DataManager.instance.gameData.state;
            var handler = string.IsNullOrEmpty(state.sceneRef.scenePath) ? SceneLoader.instance.CreateHandler(m_CrystalDreamSceneRef, string.Empty) : SceneLoader.instance.CreateHandler(state.sceneRef, SceneStatesData.StateAnchorID);
            handler.blackScreen = true;
            handler.saveGame = false;
            SceneLoader.instance.LoadScene(handler);
            handler.StopInput();
        }

        private void PerformOptions() {
            ScreenManager.instance.PushScreen(OptionsScreen.instance);
        }

        private void PerformExit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void PerformCredits() {
            ScreenManager.instance.PushScreen(CreditsScreen.instance);
        }

        private void PerformAboutNFH() {
            Application.OpenURL(m_AboutNfhUrl);
        }

        IEnumerator IScreen.OpenScreen() {
            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_TitleScreenGroup.ToggleScreen(true).WaitForCompletion();
        }

        IEnumerator IScreen.CloseScreen() {
            yield return m_TitleScreenGroup.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}