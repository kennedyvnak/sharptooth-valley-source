using NFHGame.Animations;
using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.LevelAssets.Level1e1 {
    public class Level1e1SpiderOnGround : MonoBehaviour {
        [SerializeField] private SpriteArrayAnimator m_Animator;
        [SerializeField] private Transform[] m_FollowPath;
        [SerializeField] private float m_Speed;
        [SerializeField] private float m_NextWaypointDistance;

        public Rigidbody2D rb { get; private set; }
        private int _currentWaypoint;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate() {
            var behaviourActive = !HaloManager.HaloManager.instance.haloActive;

            if (_currentWaypoint >= m_FollowPath.Length) {
                enabled = false;
                return;
            }

            Vector2 path = m_FollowPath[_currentWaypoint].position;
            Vector2 direction = (path - rb.position).normalized;
            Vector2 velocity = (behaviourActive ? 1.0f : 0.0f) * m_Speed * direction;

            rb.velocity = velocity;

            float distance = Vector2.Distance(rb.position, path);
            if (distance < m_NextWaypointDistance)
                _currentWaypoint++;
            m_Animator.enabled = behaviourActive || GameCharactersManager.instance.bastheet.rb.velocity.x != 0.0f;
        }
    }
}
