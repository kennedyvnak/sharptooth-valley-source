using NFHGame.ArticyImpl;
using NFHGame.SceneManagement;
using NFHGame.Serialization;
using UnityEngine;

namespace NFHGame {
    public class GameManager : SingletonPersistent<GameManager> {
#if GAME_TESTS
        [SerializeField] private SceneReference m_TestPortraitSceneReference;
        [SerializeField] private SceneReference m_SelectGameStateSceneReference;
#endif

        [SerializeField] private SceneReference m_GameOverScene;
        [SerializeField, TextArea] private string m_DefaultGameOverLabel;

        public bool playTimeCouting { get; set; } = false;

        public bool isPaused { get; private set; }
        public bool spammyInParty { get; private set; }

        public string gameOverLabel { get; private set; }
        public string forceOverrideGameOverLabel { get; set; }

#if GAME_TESTS
        public bool showStats;
#endif

        protected override void Awake() {
            base.Awake();
            ArticyManager.notifications.AddListener("gameState.spamInParty", (variable, value) => spammyInParty = (bool)value);
#if UNITY_EDITOR
            DataManager.instance.userManager.SetUser(0);
#endif
        }

        private void Update() {
            if (playTimeCouting)
                DataManager.instance.gameData.playTime += Time.unscaledDeltaTime;

#if GAME_TESTS
            if (UnityEngine.InputSystem.Keyboard.current.f12Key.wasPressedThisFrame) {
                LoadTest(m_TestPortraitSceneReference);
            } else if (UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame) {
                LoadTest(m_SelectGameStateSceneReference);
            } else if (UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame) {
                showStats = !showStats;
            }
            if (_timeCounter < 0.15f) {
                _timeCounter += Time.deltaTime;
                _frameCounter++;
            } else {
                _lastFramerate = _frameCounter / _timeCounter;
                _frameCounter = 0;
                _timeCounter = 0.0f;
            }
#endif
        }

#if GAME_TESTS
        private int _frameCounter = 0;
        private float _timeCounter = 0.0f;
        private float _lastFramerate = 0.0f;

        private void OnGUI() {
            if (!showStats) return;
            Rect r = new Rect(5.0f, 5.0f, Screen.width, 20.0f);
            GUI.Label(r, $"PlayTime: {DataManager.instance.gameData.playTime:N3}");
            r.y += r.height;
            GUI.Label(r, $"SelectedSlot: {(DataManager.instance ? DataManager.instance.userManager.currentUserId : -2)}; CurrentState: {(DataManager.instance ? DataManager.instance.gameData?.state?.id : string.Empty)}");
            r.y += r.height;
            GUI.Label(r, $"FPS: {(int)_lastFramerate}");
            r.y += r.height;
        }

        private void LoadTest(SceneReference reference) {
            var handler = SceneLoader.instance.CreateHandler(reference, "default");
            handler.saveGame = false;
            SceneLoader.instance.LoadScene(handler);
        }
#endif

        public void Pause() {
            if (isPaused) return;

            isPaused = true;
            playTimeCouting = false;
            Time.timeScale = 0.0f;
        }

        public void Resume() {
            if (!isPaused) return;

            isPaused = false;
            playTimeCouting = true;
            Time.timeScale = 1.0f;
        }

        public void GameOver(string label = null) {
            gameOverLabel = string.IsNullOrWhiteSpace(forceOverrideGameOverLabel) ? label ?? m_DefaultGameOverLabel : forceOverrideGameOverLabel;
            forceOverrideGameOverLabel = null;

            var handler = SceneLoader.instance.CreateHandler(m_GameOverScene, string.Empty);
            handler.saveGame = false;
            handler.blackScreen = true;
            handler.StopInput();
            SceneLoader.instance.LoadScene(handler);
        }

        public void ReloadSpammyInParty(bool spammyInParty) {
            this.spammyInParty = spammyInParty;
        }
    }
}