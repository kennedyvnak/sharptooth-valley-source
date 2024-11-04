using NFHGame.Animations;
using NFHGame.Characters;
using NFHGame.SceneManagement.GameKeys;
using UnityEngine;

namespace NFHGame.LevelAssets.Level6 {
    public class Level6AnimationTrigger : MonoBehaviour {
        [SerializeField] private SpriteArrayAnimator m_Animator;
        [SerializeField] private string m_GameKey;

        private void Start() {
            if (!string.IsNullOrEmpty(m_GameKey) && GameKeysManager.instance.HaveGameKey(m_GameKey)) {
                Disable();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.TryGetComponent<BastheetCharacterController>(out _)) {
                m_Animator.enabled = true;
                Disable();
                if (!string.IsNullOrEmpty(m_GameKey)) {
                    GameKeysManager.instance.ToggleGameKey(m_GameKey, true);
                }
            }
        }

        private void Disable() {
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
