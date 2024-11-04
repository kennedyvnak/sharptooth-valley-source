using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.Screens {
    public class CreditsScreen : Singleton<CreditsScreen>, IScreen {
        [SerializeField] private CanvasGroup m_OptionsGroup;
        [SerializeField] private GameObject m_SelectOnOpen;

        [Space]
        [SerializeField] private Button m_BackButton;

        private bool _screenActive;
        bool IScreen.screenActive { get => _screenActive; set => _screenActive = value; }
        public bool poppedByInput => true;
        GameObject IScreen.selectOnOpen => m_SelectOnOpen;
        bool IScreen.dontSelectOnActive => false;

        private void Start() {
            m_BackButton.onClick.AddListener(PerformBackButton);
        }

        private void PerformBackButton() {
            if (!_screenActive) return;

            ScreenManager.instance.PopScreen();
        }

        IEnumerator IScreen.OpenScreen() {
            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_OptionsGroup.ToggleScreen(true).WaitForCompletion();
        }

        IEnumerator IScreen.CloseScreen() {
            yield return m_OptionsGroup.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}