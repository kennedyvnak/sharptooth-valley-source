using NFHGame.AudioManagement;
using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.SpammyEvents {
    [RequireComponent(typeof(Rigidbody2D))]
    public class AmnesiaRockThrow : MonoBehaviour {
        public Rigidbody2D rb { get; private set; }

        [SerializeField] private Vector2 m_Force;
        [SerializeField] private Vector2 m_Position;
        [SerializeField] private AudioProviderObject m_HitSound;

        private System.Action _onHitSpammy;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Shoot(Vector2 force, Vector2 position, System.Action onHitSpammy = null) {
            rb.position = position;
            rb.velocity = Vector2.zero;
            rb.rotation = 0.0f;
            rb.angularVelocity = 0.0f;
            _onHitSpammy = onHitSpammy;
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (_onHitSpammy != null && collision.collider.TryGetComponent<SpammyCharacterController>(out _)) {
                _onHitSpammy?.Invoke();
                AudioPool.instance.PlaySound(m_HitSound);
                _onHitSpammy = null;
            }
        }
    }
}
