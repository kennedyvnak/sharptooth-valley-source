using System.Collections;
using DG.Tweening;
using NFHGame.Animations;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using UnityEngine;
using UnityEngine.Rendering;

namespace NFHGame.Battle {
    public class FallenRock : MonoBehaviour {
        public enum RockSize { Small, Medium, Large }

        [SerializeField] private SpriteRenderer m_ShadowRenderer;
        [SerializeField] private SpriteRenderer m_RockRenderer;
        [SerializeField] private BoxCollider2D m_Trigger;
        [SerializeField] private BoxCollider2D m_Collider;
        [SerializeField] private SpriteArrayAnimator m_SplashAnimatorPrefab;
        [SerializeField] private AudioObject m_SplashSound;

        [Header("Rendering")]
        [SerializeField] private SortingGroup m_SortingGroup;
        [SerializeField] private int m_ForegroundLayer;
        [SerializeField] private int m_ForegroundRenderIndex;
        [SerializeField] private int m_BackgroundLayer;
        [SerializeField] private int m_BackgroundRenderIndex;

        public Rigidbody2D rb { get; private set; }

        public RockSize size { get; private set; }
        public float maxFallSpeed { get; private set; }
        public bool broke { get; private set; }
        public bool inAnimation { get; private set; }

        public bool fakeRock { get; set; }
        public bool fakeBroke { get; set; }
        public float targetY { get; set; }
        public int overrideBreak { get; set; } = -1;

        private float _breakStartPositionY;
        private Vector2 _shadowScale;
        private Sprite[] _breakAnimFrames;
        private AudioProviderObject _sound;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Setup(RockSize size, Rect collider, Sprite sprite, Sprite[] breakAnimFrames, AudioProviderObject sound) {
            this.size = size;
            _sound = sound;

            _breakAnimFrames = breakAnimFrames;

            m_RockRenderer.sprite = sprite;
            m_Collider.size = collider.size;
            m_Trigger.size = collider.size + new Vector2(0.1f, 0.1f);
            m_Collider.offset = m_Trigger.offset = collider.position;

            _shadowScale = new Vector2(collider.width, collider.width * BattleProvider.instance.fallenRock.shadowHeightRatio);

            var mass = (collider.width + collider.height) * 0.5f;
            rb.gravityScale = BattleProvider.instance.fallenRock.gravityScale * mass;
            maxFallSpeed = mass * BattleProvider.instance.fallenRock.maxFallSpeed;
        }

        public void SetIsForeground(bool isForeground) {
            if (isForeground) {
                m_SortingGroup.sortingLayerID = m_ForegroundLayer;
                m_SortingGroup.sortingOrder = m_ForegroundRenderIndex;
            } else {
                m_SortingGroup.sortingLayerID = m_BackgroundLayer;
                m_SortingGroup.sortingOrder = m_BackgroundRenderIndex;
            }
        }

        private void OnDestroy() {
            m_RockRenderer.DOKill();
            m_ShadowRenderer.DOKill();
        }

        private void Update() {
            if (inAnimation) return;

            if (fakeRock) {
                m_ShadowRenderer.enabled = false;
                m_Collider.enabled = false;
                if (rb.position.y < targetY && !broke) {
                    if (fakeBroke)
                        BreakRock();
                    else
                        Splash();
                }
                return;
            }

            var hit = BattleProvider.instance.fallenRock.RaycastGround(transform.position);
            UpdateShadowPosition(hit);

            if (broke) {
                var lerp = Mathf.InverseLerp(_breakStartPositionY, hit.point.y, rb.position.y);
                m_RockRenderer.sprite = _breakAnimFrames[Mathf.RoundToInt(lerp * (_breakAnimFrames.Length - 1))];
                var col = m_ShadowRenderer.color;
                col.a = (1.0f - lerp) * BattleProvider.instance.fallenRock.shadowAlpha;
                m_ShadowRenderer.color = col;
                return;
            } else if (hit) {
                var progress = Mathf.InverseLerp(BattleProvider.instance.fallenRock.shadowStartY, hit.point.y, rb.position.y);

                m_ShadowRenderer.transform.localScale = _shadowScale * BattleProvider.instance.fallenRock.shadowScaleCurve.Evaluate(progress);
                var col = m_ShadowRenderer.color;
                col.a = BattleProvider.instance.fallenRock.shadowAlphaCurve.Evaluate(progress) * BattleProvider.instance.fallenRock.shadowAlpha;
                m_ShadowRenderer.color = col;
            } else {
                m_ShadowRenderer.enabled = false;
            }
        }

        private void FixedUpdate() {
            var vel = rb.velocity;
            vel.y = Mathf.Max(vel.y, -maxFallSpeed);
            rb.velocity = vel;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            var layer = 1 << other.gameObject.layer;

            if (fakeRock) return;

            if ((layer & BattleProvider.instance.fallenRock.groundLayer.value) != 0) {
                CollideOnGround(other);
            } else if ((layer & BattleProvider.instance.fallenRock.characterLayer.value) != 0) {
                CollideOnCharacter(other);
            } else if (!broke) {
                BattleManager.GetBastheetCollisionData(other, out var bastheet, out var forceField, out var fieldActive, out var fieldValid);
                if (fieldValid) {
                    switch (size) {
                        case RockSize.Medium:
                            forceField.ReduceForce(BattleProvider.instance.forceField.mediumRockReduction);
                            break;
                        case RockSize.Large:
                            forceField.ReduceForce(BattleProvider.instance.forceField.largeRockReduction);
                            break;
                    }
                    BreakRock();
                }
            } else if (other.TryGetComponent<PoundTrigger>(out var poundTrigger) && !broke) {
                Splash();
            }
        }

        private void CollideOnCharacter(Collider2D character) {
            if (broke) return;

            if (size == RockSize.Large) {
                overrideBreak = int.MaxValue;
                return;
            }

            if (character.TryGetComponent<BastheetCharacterController>(out var bastheet)) {
                if (transform.position.y < bastheet.transform.position.y + BattleProvider.instance.fallenRock.bastheetHeadHeight) return;

                bastheet.stateMachine.hitState.Hit(size switch {
                    RockSize.Small => BattleProvider.instance.stunTimes.littleStun,
                    RockSize.Medium => BattleProvider.instance.stunTimes.hightStun,
                    _ => 0.0f
                });
                BreakRock();
            } else if (character.TryGetComponent<DinnerCharacterController>(out var dinner)) {
                if (transform.position.y < dinner.transform.position.y + BattleProvider.instance.fallenRock.dinnerHeadHeight) return;

                dinner.dinnerStateMachine.hitState.Hit(size switch {
                    RockSize.Small => BattleProvider.instance.characters.dinnerStunLowTime,
                    RockSize.Medium => BattleProvider.instance.characters.dinnerStunMediumTime,
                    _ => 0.0f
                }, size >= RockSize.Medium);
                BreakRock();
            }
        }

        private void CollideOnGround(Collider2D ground) {
            if (broke) return;

            if (ground.TryGetComponent<FloatingGround>(out var floatingGround)) {
                if (overrideBreak == -1) {
                    if (size == RockSize.Medium) {
                        floatingGround.Crack(BattleProvider.instance.fallenRock.mediumRockGroundCrack);
                    } else if (size == RockSize.Large) {
                        floatingGround.Crack(BattleProvider.instance.fallenRock.largeRockGroundCrack);
                    }
                } else {
                    floatingGround.Crack(overrideBreak);
                }
            }

            BreakRock();
        }

        private void BreakRock() {
            if (!broke)
                StartCoroutine(BreakRockCoroutine());
        }

        private IEnumerator BreakRockCoroutine() {
            _breakStartPositionY = rb.position.y;
            broke = true;
            rb.rotation = 0.0f;
            rb.freezeRotation = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0.0f;
            inAnimation = true;

            AudioPool.instance.PlaySoundAt(_sound, transform.position);

            float elapsedTime = 0.0f;

            while (elapsedTime < BattleProvider.instance.fallenRock.rockFadeDelay) {
                var hit = BattleProvider.instance.fallenRock.RaycastGround(transform.position);
                UpdateShadowPosition(hit);

                var lerp = Mathf.Clamp01(elapsedTime / BattleProvider.instance.fallenRock.rockFadeDelay);
                m_RockRenderer.sprite = _breakAnimFrames[Mathf.RoundToInt(lerp * (_breakAnimFrames.Length - 1))];

                var col = m_ShadowRenderer.color;
                col.a = (1 - lerp) * BattleProvider.instance.fallenRock.shadowAlpha;
                m_ShadowRenderer.color = col;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield return m_RockRenderer.DOFade(0.0f, BattleProvider.instance.fallenRock.rockFadeDuration).WaitForCompletion();

            m_RockRenderer.DOKill();
            m_ShadowRenderer.DOKill();
            Destroy(gameObject);
        }

        private void UpdateShadowPosition(RaycastHit2D hit) {
            m_ShadowRenderer.transform.position = hit ? hit.point + BattleProvider.instance.fallenRock.shadowOffset : new Vector3(0.0f, 0.0f, -10000.0f);
        }

        private void Splash() {
            var splashAnimation = Instantiate(m_SplashAnimatorPrefab, transform);
            splashAnimation.loopFinished.AddListener(Destroy);
            var renderer = GetComponent<SpriteRenderer>();
            splashAnimation.valueChanged.AddListener(sp => renderer.sprite = sp);
            broke = true;
            rb.rotation = 0.0f;
            rb.freezeRotation = true;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0.0f;
            inAnimation = true;

            AudioPool.instance.PlaySoundAt(m_SplashSound, transform.position);
        }

        public void Destroy() { Destroy(gameObject); }
    }
}
