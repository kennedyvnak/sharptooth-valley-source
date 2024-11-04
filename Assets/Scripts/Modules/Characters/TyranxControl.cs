using UnityEngine;

namespace NFHGame.Characters {
    public class TyranxControl : MonoBehaviour {
        [SerializeField] private Transform m_Renderer;
        [SerializeField] private float m_FloatingMaxY;
        [SerializeField] private float m_MoveSpeed;
        [SerializeField] private bool m_StartIdle;
        [SerializeField] private string m_Animation;
        [SerializeField] private Animator m_Animator;

        private float _startFloatingTime;
        private bool _move;

        public Animator animator => m_Animator;

        private void Start() {
            _move = !m_StartIdle;
            if (m_Animator) m_Animator.Play(m_Animation);
        }

        private void Update() {
            if (_move) {
                float deltaT = Time.time - _startFloatingTime;
                var pos = m_Renderer.localPosition;
                pos.y = PositiveSin(deltaT * m_MoveSpeed) * m_FloatingMaxY;
                m_Renderer.localPosition = pos;
            }
        }

        public void StartFloating() {
            _startFloatingTime = Time.time;
            _move = true;
        }

        public void EndFloating() {
            _move = false;
        }

        private float PositiveSin(float t) {
            return (1.0f + Mathf.Sin(t)) * 0.5f;
        }
    }
}
