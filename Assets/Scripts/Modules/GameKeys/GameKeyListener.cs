using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NFHGame.SceneManagement.GameKeys {
    public class GameKeyListener : MonoBehaviour {
        [FormerlySerializedAs("m_Checkpoint")]
        [SerializeField] private string m_GameKey;

        [FormerlySerializedAs("m_CheckpointToggled")]
        [SerializeField] private UnityEvent<bool> m_GameKeyToggled;
        [FormerlySerializedAs("m_TrueWithCheckpoint")]
        [SerializeField] private bool m_TrueWithGameKey = true;

        [SerializeField] private bool m_Listen;

        private bool _listen;

        private void OnEnable() {
            if (m_Listen) {
                _listen = true;
                GameKeysManager.instance.gameKeyToggled.AddListener(EVENT_GameKeyToggled);
            }
        }

        private void OnDisable() {
            if (_listen) {
                GameKeysManager.instance.gameKeyToggled.RemoveListener(EVENT_GameKeyToggled);
                _listen = false;
            }
        }

        private void Start() {
            ToggleGameKey(GameKeysManager.instance.HaveGameKey(m_GameKey));
        }

        public void ActiveGameKey() {
            GameKeysManager.instance.ToggleGameKey(m_GameKey, true);
        }

        private void ToggleGameKey(bool keyEnabled) {
            m_GameKeyToggled?.Invoke((keyEnabled && m_TrueWithGameKey) || (!keyEnabled && !m_TrueWithGameKey));
        }

        private void EVENT_GameKeyToggled(string gameKey, bool keyEnabled) {
            if (m_GameKey == gameKey)
                ToggleGameKey(keyEnabled);
        }
    }
}
    