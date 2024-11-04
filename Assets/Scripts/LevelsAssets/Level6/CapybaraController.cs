using NFHGame.RangedValues;
using UnityEngine;

namespace NFHGame.LevelAssets.Level6 {
    public class CapybaraController : MonoBehaviour {
        [SerializeField] private Sprite[] m_Sprites;
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private Transform m_LookAt;
        [SerializeField] private RangedFloat m_LookRange;

        private int _lenght;

        private void Start() {
            _lenght = m_Sprites.Length - 1;
        }

        private void LateUpdate() {
            float posX = Mathf.Clamp(m_LookAt.position.x, m_LookRange.min, m_LookRange.max);
            float lerp = Mathf.InverseLerp(m_LookRange.min, m_LookRange.max, posX);
            m_Renderer.sprite = m_Sprites[Mathf.RoundToInt(lerp * _lenght)];
        }
    }
}
