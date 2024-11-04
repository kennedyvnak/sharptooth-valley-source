using Cinemachine;
using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.RangedValues;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace NFHGame.Battle {
    public class ShipCanon : MonoBehaviour {
        public static readonly int ShipMoodAngryHash = Animator.StringToHash("MOODangry");
        public static readonly int ShipMoodCalmHash = Animator.StringToHash("MOODcalm");
        public static readonly int ShipMoodDeadHash = Animator.StringToHash("MOODdead");
        public static readonly int ShipMoodIdleHash = Animator.StringToHash("MOODidle");

        [Header("Mood")]
        [SerializeField] private Animator m_MoodAnimator;

        [SerializeField] private ShipMiniLaser m_MiniLaserPrefab;
        [SerializeField] private Transform[] m_MiniLasersParents;
        [SerializeField] private ShipLargeLaser m_LargeLaser;
        [SerializeField] private float[] m_MiniLaserChance;

        [Header("Teeth")]
        [SerializeField] private SpriteRenderer m_TeethRenderer;
        [SerializeField] private Sprite[] m_OpenTeethArray;
        [SerializeField] private Sprite[] m_CloseTeethArray;
        [SerializeField] private float m_TeethAnimTime;
        [SerializeField] private AudioObject m_TeethOpenSound;
        [SerializeField] private AudioObject m_TeethCloseSound;
        [SerializeField] private AudioSource m_TeethSoundSource;

        [Header("Shake")]
        [SerializeField] private CinemachineImpulseSource m_ImpulseSource;

        private ObjectPool<ShipMiniLaser> _miniLaserPool;

        public ShipLargeLaser largeLaser => m_LargeLaser;

        private void Start() {
            _miniLaserPool = new ObjectPool<ShipMiniLaser>(() => {
                var laser = Instantiate(m_MiniLaserPrefab);
                laser.OnRelease += () => _miniLaserPool.Release(laser);
                return laser;
            }, (x) => x.gameObject.SetActive(true), (x) => x.gameObject.SetActive(false), (x) => Destroy(x.gameObject));
            m_LargeLaser.Setup(this);
        }

        private void OnDestroy() {
            this.DOKill();
        }

        public void PerformShoot() => m_LargeLaser.PerformShoot();

        public void PerformMiniLasers(int comboCount, RangedFloat comboDelay) {
            float currentDelay = 0.0f;
            for (int i = 0; i < comboCount; i++) {
                DOVirtual.DelayedCall(currentDelay, ShootMiniLaser).SetTarget(this);
                currentDelay += comboDelay.RandomRange();
            }
        }

        public void ShakeCamera(float force) {
            m_ImpulseSource.GenerateImpulseWithForce(force);
        }

        private void ShootMiniLaser() {
            int v = Random.Range(0, m_MiniLaserChance.Length);
            for (int i = 0; i < m_MiniLasersParents.Length; i++) {
                float rng = Random.value;
                if (m_MiniLaserChance[v] > rng) {
                    var parent = m_MiniLasersParents[i];
                    var laser = _miniLaserPool.Get();
                    laser.transform.SetParent(parent);
                    laser.PerformShoot();
                }
                v = (v + 1) % m_MiniLaserChance.Length;
            }
        }

        public void EndBattle() {
            foreach (var parent in m_MiniLasersParents) {
                foreach (Transform child in parent) {
                    Destroy(child.gameObject);
                }
            }
            m_LargeLaser.ForceStopLaser();
        }

        public float OpenTeeth() {
            StartCoroutine(AnimateTeeth(m_OpenTeethArray, m_TeethOpenSound));
            return m_TeethAnimTime;
        }

        public float CloseTeeth() {
            StartCoroutine(AnimateTeeth(m_CloseTeethArray, m_TeethCloseSound));
            return m_TeethAnimTime;
        }

        public void SetMood(int moodHash) {
            m_MoodAnimator.Play(moodHash);
        }

        private IEnumerator AnimateTeeth(Sprite[] anim, AudioObject audio) {
            audio.CloneToSource(m_TeethSoundSource);
            m_TeethSoundSource.Play();

            float step = m_TeethAnimTime / anim.Length;
            int idx = 0;

            while (idx < anim.Length) {
                m_TeethRenderer.sprite = anim[idx];
                yield return Helpers.GetWaitForSeconds(step);
                idx++;
            }
        }

        public bool IsIdle() {
            return m_LargeLaser.IsIdle();
        }
    }
}