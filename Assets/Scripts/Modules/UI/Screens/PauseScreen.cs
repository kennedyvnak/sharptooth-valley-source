using System.Collections;
using DG.Tweening;
using NFHGame.Input;
using NFHGame.SceneManagement;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace NFHGame.Screens {
    public class PauseScreen : Singleton<PauseScreen>, IScreen {
        [SerializeField] private CanvasGroup m_PauseGroup;
        [SerializeField] private GameObject m_SelectOnOpen;
        [SerializeField] private SceneReference m_TitleScreenSceneRef;

        [Space]
        [SerializeField] private Button m_OptionsButton, m_ReturnButton, m_SaveButton, m_ExitButton;

        [Space]
        [SerializeField, TextArea] private string m_SaveOverWarning;
        [SerializeField] private float m_SaveOverPopupDelay;

        private bool _screenActive;
        bool IScreen.screenActive { get => _screenActive; set => _screenActive = value; }
        public bool poppedByInput => true;
        GameObject IScreen.selectOnOpen => m_SelectOnOpen;
        bool IScreen.dontSelectOnActive => false;

        public bool canSave { get; set; } = true;

        private void Start() {
            m_OptionsButton.onClick.AddListener(PerformOptionsClick);
            m_ReturnButton.onClick.AddListener(PerformReturnClick);
            m_SaveButton.onClick.AddListener(PerformSaveClick);
            m_ExitButton.onClick.AddListener(PerformExitClick);

            m_SaveButton.SetNavigation(up: m_OptionsButton, down: m_ReturnButton, right: m_ExitButton);
        }

        private void OnEnable() {
            InputReader.instance.OnPause += INPUT_OnPause;
        }

        private void OnDisable() {
            InputReader.instance.OnPause -= INPUT_OnPause;
        }

        public void OpenPause() {
            ScreenManager.instance.PushScreen(this);
        }

        private void PerformOptionsClick() {
            ScreenManager.instance.PushScreen(OptionsScreen.instance);
        }

        private void PerformReturnClick() {
            ScreenManager.instance.PopScreen();
        }

        private void PerformSaveClick() {
            SavesScreen.instance.GetUser(true, (user) => {
                if (user == -1) return;
                if (DataManager.instance.dataHandler.HaveUser(user)) {
                    ConfirmPopup.instance.Popup(m_SaveOverWarning, "Save", "Cancel", m_SaveOverPopupDelay, (b) => {
                        ConfirmPopup.instance.ClosePopup();
                        if (b)
                            Save(user);
                    });
                } else {
                    ConfirmPopup.instance.ClosePopup();
                    Save(user);
                }
            });

            static void Save(int user) {
                DataManager.instance.Save(user);
                SavesScreen.instance.UpdateSlot(user, DataManager.instance.gameData);
                ScreenManager.instance.PopScreen();
            }
        }

        private void PerformExitClick() {
            LoadTitleScreen();
        }

        public void LoadTitleScreen() {
            var handler = SceneLoader.instance.CreateHandler(m_TitleScreenSceneRef, "exitFromGameplay");
            InputReader.instance.PopMap(InputReader.InputMap.UI);
            handler.blackScreen = true;
            handler.saveGame = canSave;
            handler.StopInput();
            SceneLoader.instance.LoadScene(handler);
        }

        IEnumerator IScreen.OpenScreen() {
            m_SaveButton.interactable = canSave;
            m_ReturnButton.SetNavigation(up: canSave ? m_SaveButton : m_ExitButton, down: m_OptionsButton);
            m_OptionsButton.SetNavigation(up: m_ReturnButton, down: canSave ? m_SaveButton : m_ExitButton);
            m_ExitButton.SetNavigation(up: m_OptionsButton, down: m_ReturnButton, left: canSave ? m_SaveButton : null);
            GameManager.instance.Pause();
            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_PauseGroup.ToggleScreen(true).WaitForCompletion();
        }

        IEnumerator IScreen.CloseScreen() {
            yield return m_PauseGroup.ToggleScreen(false).WaitForCompletion();
            if (ScreenManager.instance.screenCount == 0)
                ResumeGameplay();
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void ResumeGameplay() {
            GameManager.instance.Resume();
        }

        private void INPUT_OnPause() {
            if (_screenActive) return;

            OpenPause();
        }
    }
}