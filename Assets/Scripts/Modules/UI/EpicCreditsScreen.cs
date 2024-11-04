using DG.Tweening;
using NFHGame.Input;
using NFHGame.Screens;
using System.Collections;
using UnityEngine;

namespace NFHGame {
    public class EpicCreditsScreen : Singleton<EpicCreditsScreen>, IScreen {
        [SerializeField] private CanvasGroup m_CanvasGroup;

        public bool dontSelectOnActive => true;
        public bool poppedByInput => false;
        public GameObject selectOnOpen => null;
        bool IScreen.screenActive { get; set; }

        public IEnumerator OpenScreen() {
            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_CanvasGroup.ToggleScreen(true).WaitForCompletion();
        }

        public IEnumerator CloseScreen() {
            yield return m_CanvasGroup.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void LoadMainMenu() {
            InputReader.instance.PopMap(InputReader.InputMap.None);
            PauseScreen.instance.LoadTitleScreen();
        }
    }
}
