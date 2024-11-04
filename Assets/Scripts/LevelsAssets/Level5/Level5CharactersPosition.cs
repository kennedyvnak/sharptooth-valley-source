using UnityEngine;

namespace NFHGame.LevelAssets.Level5 {
    public class Level5CharactersPosition : MonoBehaviour {
        [System.Serializable] public struct Level5Character {
            public string animation;
            public Animator animator;
        }

        [SerializeField] private Level5Character[] m_Characters;
        [SerializeField] private SpriteRenderer m_BastheetRenderer;
        [SerializeField] private SpriteRenderer m_BastheetTailRenderer;

        private void OnEnable() {
            foreach(var character in m_Characters) {
                character.animator.Play(character.animation);
            }
        }

        private void Update() {
            m_BastheetTailRenderer.color = m_BastheetRenderer.color;
        }
    }
}
