using DG.Tweening;
using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.Battle {
    public class FloatingGround : MonoBehaviour {
        [SerializeField] private SpriteRenderer m_Renderer;
        [SerializeField] private SpriteMask m_SpriteMask;
        [SerializeField] private SpriteRenderer m_OverlayRenderer;
        [SerializeField] private Collider2D m_Collider;

        public bool playerEnter { get; private set; }

        private int _currentLife;
        public int currentLife {
            get { return _currentLife; }
            private set {
                _currentLife = Mathf.Max(value, 0);
                UpdateLife();
            }
        }

        private float _timer;
        private Tween _overlayTween;

        private void Awake() {
            currentLife = BattleProvider.instance.floatingGround.defaultGroundLife;
            _timer = Random.Range(0.0f, BattleProvider.instance.floatingGround.regenerationDuration);
        }

        private void Update() {
            if ((_timer += Time.deltaTime) >= BattleProvider.instance.floatingGround.regenerationDuration) {
                _currentLife = Mathf.Clamp(_currentLife + 1, 0, BattleProvider.instance.floatingGround.defaultGroundLife);
                UpdateLife();
                _timer = 0.0f;
            }
        }

        private void OnDestroy() {
            _overlayTween.Kill();
        }

        public void Regen() {
            _currentLife = BattleProvider.instance.floatingGround.defaultGroundLife;
            UpdateLife();
        }

        public void Crack(int life) {
            _currentLife = Mathf.Clamp(_currentLife - life, 0, BattleProvider.instance.floatingGround.defaultGroundLife);
            if (_currentLife == 0) {
                _timer -= Random.Range(0.0f, BattleProvider.instance.floatingGround.regenerationDuration);
                if (_timer < 0.0f) _timer = 0.0f;
            }

            UpdateLife();
        }

        [ContextMenu("Break Ground")]
        public void Break() {
            _currentLife = 0;
            UpdateLife();
        }

        private void UpdateLife() {
            m_Renderer.sprite = BattleProvider.instance.floatingGround.groundSpritesByLife[currentLife];

            bool broke = _currentLife == 0;
            m_Collider.enabled = !broke;
            m_SpriteMask.enabled = broke;

            if (_currentLife == 1) {
                PlayWarningAnim();
            } else {
                StopWarningAnim();
            }
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (!other.gameObject.TryGetComponent<BastheetCharacterController>(out var bastheet)) return;
            playerEnter = true;
        }

        private void OnCollisionExit2D(Collision2D other) {
            if (!other.gameObject.TryGetComponent<BastheetCharacterController>(out var bastheet)) return;
            playerEnter = false;
        }

        private void PlayWarningAnim() {
            _overlayTween.Kill();
            _overlayTween = m_OverlayRenderer.DOFade(0.6f, 0.66666f).OnComplete(() => _overlayTween = m_OverlayRenderer.DOFade(1.0f, 0.35f).SetLoops(-1, LoopType.Yoyo));
        }

        private void StopWarningAnim() {
            if (m_OverlayRenderer.color.a <= 0.01f)
                return;
            _overlayTween.Kill();
            _overlayTween = m_OverlayRenderer.DOFade(0.0f, 0.66666f);
        }
    }
}
