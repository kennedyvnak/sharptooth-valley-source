using NFHGame.Serialization;
using NFHGame.Serialization.States;
using TMPro;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class GameOverStateController : SceneStateController {
        [SerializeField] private TextMeshProUGUI m_GameOverTooltipText;
        public TextMeshProUGUI gameOverTooltipText => m_GameOverTooltipText;

        protected override void OnDestroy() {
            base.OnDestroy();
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);
            gameOverTooltipText.text = GameManager.instance.gameOverLabel;
            handler.ResumeInput();
            GameManager.instance.Resume();
            GameManager.instance.playTimeCouting = false;
        }

        public void ReturnToGameplay() {
            if (SceneLoader.instance.isLoadingScene) return;

            DataManager.instance.userManager.ReloadUser();
            GameData data = DataManager.instance.gameData;
            var handler = SceneLoader.instance.CreateHandler(data.state.sceneRef, SceneStatesData.StateAnchorID);
            handler.saveGame = false;
            handler.blackScreen = true;
            handler.StopInput();
            SceneLoader.instance.LoadScene(handler);
        }
    }
}
