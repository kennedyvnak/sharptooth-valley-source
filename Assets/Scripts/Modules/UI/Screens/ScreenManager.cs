using System.Collections;
using System.Collections.Generic;
using NFHGame.Input;
using NFHGame.UI.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NFHGame.Screens {
    public class ScreenManager : Singleton<ScreenManager> {
        private static readonly int k_UiBlockKey = "ScreenMng".GetHashCode();

        [SerializeField] private float m_ScreenFadeDuration;

        private readonly Stack<IScreen> _screens = new Stack<IScreen>();

        private IScreen _currentScreen;

        private Coroutine _internalOperation;
        private bool _isBusy;

        public float screenFadeDuration => m_ScreenFadeDuration;
        public IScreen currentScreen => _currentScreen;
        public int screenCount => _screens.Count;
        public bool isBusy => _isBusy;

        private void Start() {
            InputReader.instance.OnCancel += INPUT_OnCancel;
        }

        protected override void OnDestroy() {
            InputReader.instance.OnCancel -= INPUT_OnCancel;
            base.OnDestroy();
        }

        public void PushScreen(IScreen screen) {
            this.EnsureCoroutineStopped(ref _internalOperation);
            _isBusy = true;
            _internalOperation = StartCoroutine(PushScreenInternal(screen));
        }

        public void PopScreen() {
            if (_screens.Count == 0) return;

            this.EnsureCoroutineStopped(ref _internalOperation);
            _isBusy = true;
            _internalOperation = StartCoroutine(PopScreenInternal());
        }

        public void PopAll() {
            if (_currentScreen == null) return;

            this.EnsureCoroutineStopped(ref _internalOperation);
            _isBusy = true;
            _internalOperation = StartCoroutine(PopAllInternal());
        }

        public void ForcePopAll() {
            if (_currentScreen == null) return;

            this.EnsureCoroutineStopped(ref _internalOperation);
            _screens.Clear();
            SetUI(false);
            _isBusy = false;
        }

        private IEnumerator PushScreenInternal(IScreen screen) {
            if (_currentScreen == null) {
                SetUI(true);
            }

            _screens.Push(screen);
            if (_currentScreen != null) {
                yield return CloseScreen(_currentScreen);
            }

            yield return OpenScreen(screen);
            _isBusy = false;
        }

        private IEnumerator PopScreenInternal() {
            var screen = _screens.Pop();
            yield return CloseScreen(screen);

            if (_screens.Count > 0) {
                yield return OpenScreen(_screens.Peek());
            } else {
                SetUI(false);
            }
            _isBusy = false;
        }

        private IEnumerator PopAllInternal() {
            yield return CloseScreen(_currentScreen);
            _screens.Clear();
            SetUI(false);
            _isBusy = false;
        }

        private IEnumerator OpenScreen(IScreen screen) {
            _currentScreen = screen;
            screen.screenActive = true;
            if (!screen.dontSelectOnActive)
                EventSystem.current.SetSelectedGameObject(screen.selectOnOpen);
            var op = screen.OpenScreen();
            yield return op;
        }

        private IEnumerator CloseScreen(IScreen screen) {
            screen.screenActive = false;
            yield return screen.CloseScreen();
        }

        private void SetUI(bool isUI) {
            if (UserInterfaceInput.instance) UserInterfaceInput.instance.SetInteractable(k_UiBlockKey, !isUI);

            if (isUI) {
                InputReader.instance.PushMap(InputReader.InputMap.UI);
            } else {
                _currentScreen = null;
                InputReader.instance.PopMap(InputReader.InputMap.UI);
            }
        }

        private void INPUT_OnCancel() {
            if (_currentScreen != null && _currentScreen.poppedByInput && !_isBusy) {
                PopScreen();
            }
        }
    }
}
