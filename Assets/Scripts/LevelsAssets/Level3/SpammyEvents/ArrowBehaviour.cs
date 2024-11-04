using Cinemachine;
using NFHGame.Animations;
using NFHGame.Battle;
using NFHGame.Characters;
using NFHGame.RangedValues;
using UnityEngine;

namespace NFHGame.SpammyEvents {
    [RequireComponent(typeof(Rigidbody2D))]
    public class ArrowBehaviour : MonoBehaviour {
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private Sprite[] m_StateSprites;
        [SerializeField] private Sprite[] m_StateAltherSprites;
        [SerializeField] private RangedFloat m_AngleRange;
        [SerializeField] private bool m_IsAltherArrow;
        [SerializeField] private float m_AngleClamp;
        [SerializeField] private int m_IgnoreBastheetLayer;
        [SerializeField] private Vector3 m_ExplosionOffset;

        [SerializeField] private CinemachineImpulseSource m_ImpulseSource;
        [SerializeField] private AudioSource m_AlterArrowExplosionSound;

        [SerializeField] private Vector2 m_TestPosition;
        [SerializeField] private Vector2 m_TestForce;

        public Rigidbody2D rb { get; private set; }
        public SpriteRenderer sRender { get; private set; }
        public BoxCollider2D boxCollider { get; private set; }
        public bool isAltherArrow { get => m_IsAltherArrow; set => m_IsAltherArrow = value; }
        public bool destroyBastheet { get; set; } = true;

        private int _defaultLayer;
        private int _lenght;
        private System.Action _onHitBastheet;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
            sRender = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        [ContextMenu("Test Shoot")]
        private void TestShoot() {
            destroyBastheet = false;
            Shoot(m_TestForce, m_TestPosition, isAltherArrow, destroyBastheet);
        }

        private void Start() {
            _lenght = m_StateSprites.Length - 1;
            _defaultLayer = gameObject.layer;
        }

        private void FixedUpdate() {
            Vector2 velocity = rb.velocity;

            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            if (angle > -m_AngleClamp && angle < m_AngleClamp) angle = 0.0f;

            float lerp = Mathf.InverseLerp(m_AngleRange.min, m_AngleRange.max, angle);
            var index = Mathf.RoundToInt(lerp * _lenght);
            m_Renderer.sprite = isAltherArrow ? m_StateAltherSprites[index] : m_StateSprites[index];
            m_Renderer.flipX = velocity.x < 0.5f;
        }

        public void Shoot(Vector2 force, Vector2 position, bool alterArrow, bool destroyBast, System.Action onHitBastheet = null) {
            sRender.enabled = true;
            boxCollider.enabled = true;
            isAltherArrow = alterArrow;
            destroyBastheet = destroyBast;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.position = position;
            rb.velocity = Vector2.zero;
            rb.rotation = 0.0f;
            rb.angularVelocity = 0.0f;
            _onHitBastheet = onHitBastheet;
            gameObject.layer = _defaultLayer;
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.collider.TryGetComponent<BastheetCharacterController>(out var bastheet) || collision.collider.TryGetComponent<ShipCanon>(out var shipCanon)) {
                _onHitBastheet?.Invoke();
                _onHitBastheet = null;
                gameObject.layer = m_IgnoreBastheetLayer;

                if (isAltherArrow) {
                    rb.bodyType = RigidbodyType2D.Static;
                    var explosion = transform.GetChild(0);
                    explosion.position = bastheet? bastheet.transform.position + m_ExplosionOffset : collision.contacts[0].point + (Vector2)m_ExplosionOffset;
                    explosion.gameObject.SetActive(true);
                    explosion.GetComponent<SpriteArrayAnimator>().Replay();

                    if (m_AlterArrowExplosionSound)
                        m_AlterArrowExplosionSound.Play();

                    if (destroyBastheet) {
                        if (m_ImpulseSource)
                            m_ImpulseSource.GenerateImpulse();
                        GameCharactersManager.instance.bastheet.gameObject.SetActive(false);
                        GameCharactersManager.instance.dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.GetAnimation(true));
                    }
                    m_Renderer.enabled = false;
                }
            }
        }
    }
}