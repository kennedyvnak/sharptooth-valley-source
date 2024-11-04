using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.RangedValues;
using UnityEngine;

namespace NFHGame.Battle {
    public class ShipMiniLaser : MonoBehaviour {
        [SerializeField] private float m_LaserSpeed;
        [SerializeField] private AudioProviderObject m_Audio;
        [SerializeField] private AudioSource m_Source;
        [SerializeField, RangedValue(-360.0f, 360.0f)] private RangedFloat m_Angle;

        public Rigidbody2D rb { get; private set; }
        public TrailRenderer trail { get; private set; }
        public Collider2D trigger { get; private set; }

        public event System.Action OnRelease;

        private bool _released;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
            trail = GetComponent<TrailRenderer>();
            trigger = GetComponent<Collider2D>();
        }

        public void PerformShoot() {
            _released = false;
            trigger.enabled = true;
            transform.localPosition = Vector3.zero;
            trail.Clear();
            Vector2 direction = GameCharactersManager.instance.bastheet.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) + m_Angle.RandomRange() * Mathf.Deg2Rad;
            rb.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * m_LaserSpeed;

            m_Audio.CloneToSource(m_Source);
            m_Source.Play();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_released) return;
            bool deactivate = false;

            BattleManager.GetBastheetCollisionData(other, out var bastheet, out var forceField, out var fieldActive, out var fieldValid);

            if (forceField && fieldActive) {
                forceField.AbsorbForce(BattleProvider.instance.forceField.miniLaserAbsorbForce);
                deactivate = true;
            } else if (!fieldValid && bastheet) {
                bastheet.stateMachine.hitState.Hit(BattleProvider.instance.stunTimes.littleStun);
                deactivate = true;
            } else if (other.TryGetComponent<DinnerCharacterController>(out var dinner)) {
                dinner.dinnerStateMachine.hitState.Hit(BattleProvider.instance.characters.dinnerStunLowTime, false);
                deactivate = true;
            }

            if (deactivate) {
                rb.velocity = Vector2.zero;
                trigger.enabled = false;
                _released = true;
                OnRelease?.Invoke();
            }
        }
    }
}
