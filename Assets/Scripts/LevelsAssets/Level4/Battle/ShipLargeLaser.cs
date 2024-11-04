using System;
using System.Collections;
using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.Input;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace NFHGame.Battle {
    public class ShipLargeLaser : MonoBehaviour {
        [System.Serializable]
        public struct AnimationData {
            public AudioSource source;
            public AudioObject blastSound;
        }

        [SerializeField] private float m_BlastSoundFadeDuration;

        [SerializeField] private AnimationData m_Animation;
        [SerializeField] private float m_LargeLaserSpeed;
        [SerializeField] private float m_EndLargeLaserX;
        [SerializeField] private float m_LaserShakeForce;
        [SerializeField] private int m_AbsordShieldAmount;
        [SerializeField] private FusionFx m_FusionFx;

        [Header("Death Animation")]
        [SerializeField] private float m_EndAnimDuration;
        [SerializeField] private Volume m_Volume;
        [SerializeField] private float m_EndBloomIntensity;
        [SerializeField] private Image m_WhiteScreenOverrideImage;
        [SerializeField] private float m_WhiteOverrideAppearDelay;
        [SerializeField] private float m_WhiteOverrideDuration;

        [SerializeField, TextArea] private string m_GameOverLabel;

        public Rigidbody2D rb { get; private set; }
        public Collider2D trigger { get; private set; }
        public TrailRenderer trail { get; private set; }

        private ShipCanon _shipCanon;
        private Vector2 _startPosition;
        private bool _active;
        private bool _idle = true;
        private bool _cutscene = false;
        private int _absorbExtra = 0;

        private System.Action _gameOverAction;

        private void Awake() {
            trail = GetComponent<TrailRenderer>();
            rb = GetComponent<Rigidbody2D>();
            trigger = GetComponent<Collider2D>();
        }

        private void Start() {
            _startPosition = rb.position;
        }

        private void OnDestroy() {
            this.DOKill();
        }

        private void FixedUpdate() {
            if (_active) {
                if (rb.position.x < m_EndLargeLaserX) {
                    Deactivate();
                }
            }
        }

        private void Deactivate() {
            _active = false;
            _idle = true;
            rb.velocity = Vector2.zero;
            trigger.enabled = false;
            m_Animation.source.DOFade(0.0f, m_BlastSoundFadeDuration).SetTarget(this);
            _shipCanon.CloseTeeth();
        }

        [ContextMenu("Shoot")]
        public void PerformShoot() {
            _cutscene = false;
            _idle = false;
            transform.position = _startPosition;
            trail.Clear();

            int force = GameCharactersManager.instance.bastheet.forceField.currentForce;
            _absorbExtra = m_AbsordShieldAmount < force ? m_AbsordShieldAmount : force;
            GameCharactersManager.instance.bastheet.forceField.ReduceForce(m_AbsordShieldAmount);

            DOVirtual.DelayedCall(_shipCanon.OpenTeeth(), () => {
                m_FusionFx.StartAnim();
                m_FusionFx.ShotAnim(() => {
                    rb.velocity = new Vector2(m_LargeLaserSpeed, 0.0f);
                    trigger.enabled = true;
                    _active = true;

                    m_Animation.blastSound.CloneToSource(m_Animation.source);
                    m_Animation.source.Play();
                    BattleManager.instance.shipCanon.ShakeCamera(m_LaserShakeForce);
                    BattleManager.instance.ResetRockWave();
                    _shipCanon.CloseTeeth();
                });
            });
        }

        private void OnTriggerEnter2D(Collider2D other) {
            BattleManager.GetBastheetCollisionData(other, out var bastheet, out var forceField, out var fieldActive, out var fieldValid);

            if (other.TryGetComponent<FloatingGround>(out var floatingGround) && !floatingGround.playerEnter && !_cutscene) {
                floatingGround.Break();
            } else if (forceField && forceField.fieldActive) {
                forceField.AbsorbForce(BattleProvider.instance.forceField.plasmaAbsorbForce + _absorbExtra);
                Deactivate();
            } else if (!fieldValid && (other.TryGetComponent<DinnerCharacterController>(out var dinner) || bastheet)) {
                GameOver();
            }
        }

        public void ChargeAttack(bool openTeeth = true) {
            if (openTeeth) {
                DOVirtual.DelayedCall(_shipCanon.OpenTeeth(), () => {
                    m_FusionFx.StartAnim();
                });
            } else {
                m_FusionFx.StartAnim();
            }
        }

        public void Blast(System.Action onBlast, System.Action gameOver) {
            m_FusionFx.ShotAnim(() => {
                _cutscene = true;
                rb.velocity = new Vector2(m_LargeLaserSpeed, 0.0f);
                trail.enabled = true;
                trigger.enabled = true;
                _active = true;
                _gameOverAction = gameOver;

                m_Animation.blastSound.CloneToSource(m_Animation.source);
                m_Animation.source.Play();
                BattleManager.instance.shipCanon.ShakeCamera(m_LaserShakeForce);
                BattleManager.instance.rockSpawner.onlyFakeRocks = true;
                BattleManager.instance.ResetRockWave();

                onBlast?.Invoke();
            });
        }

        public void CancelAttack() {
            m_FusionFx.EndAnim();
            _shipCanon.CloseTeeth();
        }

        private void GameOver() {
            if (BattleManager.instance.gameOver) return;
            BattleManager.instance.gameOver = true;
            Time.timeScale = 0.0f;
            _gameOverAction?.Invoke();
            InputReader.instance.PushMap(InputReader.InputMap.None);
            m_Volume.profile.TryGet<Bloom>(out var bloom);
            DOVirtual.Float(bloom.intensity.value, m_EndBloomIntensity, m_EndAnimDuration, (x) => {
                bloom.intensity.value = x;
            }).SetUpdate(true).SetEase(Ease.InCubic).SetTarget(this);
            m_WhiteScreenOverrideImage.DOFade(1.0f, m_WhiteOverrideDuration).SetDelay(m_WhiteOverrideAppearDelay).SetUpdate(true).OnComplete(() => {
                Time.timeScale = 1.0f;
                InputReader.instance.PopMap(InputReader.InputMap.None);
                BattleManager.instance.GameOver(m_GameOverLabel);
            }).SetTarget(this);
        }

        public void ForceStopLaser() {
            rb.velocity = Vector2.zero;
            trigger.enabled = false;
            _active = false;
            m_Animation.source.Stop();
            transform.position = _startPosition;
            trail.Clear();
            _shipCanon.CloseTeeth();
        }

        public void Setup(ShipCanon shipCanon) {
            _shipCanon = shipCanon;
        }

        public bool IsIdle() {
            return _idle;
        }
    }
}