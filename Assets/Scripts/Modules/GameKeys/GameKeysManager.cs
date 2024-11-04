using NFHGame.ScriptableSingletons;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NFHGame.SceneManagement.GameKeys {
    public class GameKeysManager : ScriptableSingleton<GameKeysManager> {
        [FormerlySerializedAs("m_CheckpointToggled")]
        [SerializeField] private UnityEvent<string, bool> m_GameKeyToggled;
        public UnityEvent<string, bool> gameKeyToggled => m_GameKeyToggled;

        public bool HaveGameKey(string gameKey) => DataManager.instance.gameData.gameKeys.Contains(gameKey);

        public void ToggleGameKey(string gameKey, bool value) {
            var gameKeys = DataManager.instance.gameData.gameKeys;

            if (value && !gameKeys.Contains(gameKey)) {
                gameKeys.Add(gameKey);
            } else if (!value && gameKeys.Contains(gameKey)) {
                gameKeys.Remove(gameKey);
            } else {
                return;
            }

            GameLogger.gameKeys.Log($"Toggle {gameKey} to {value}", LogLevel.Verbose);
            m_GameKeyToggled?.Invoke(gameKey, value);
        }

        public void EnableGameKey(string gameKey) {
            ToggleGameKey(gameKey, true);
        }

        public void DisableGameKey(string gameKey) {
            ToggleGameKey(gameKey, false);
        }
    }
}