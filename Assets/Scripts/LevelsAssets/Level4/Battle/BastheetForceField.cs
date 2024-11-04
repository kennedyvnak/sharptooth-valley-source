using System.Collections.Generic;
using DG.Tweening;
using NFHGame.Characters;
using NFHGame.Characters.StateMachines;
using NFHGame.External;
using NFHGame.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NFHGame.Battle {
    public class BastheetForceField : MonoBehaviour {
        private static readonly int k_ShapeColorID = Shader.PropertyToID("_HDRColor");

        public enum ShieldState { Disabled = -1, Level0, Level1, Level2, Level3, Level4 }

        public static readonly Dictionary<ShieldState, int> k_AnimatorHashes = new Dictionary<ShieldState, int>() {
            { ShieldState.Disabled, Animator.StringToHash("SHIELDDisabled") },
            { ShieldState.Level0, Animator.StringToHash("SHIELDlevel00") },
            { ShieldState.Level1, Animator.StringToHash("SHIELDlevel01") },
            { ShieldState.Level2, Animator.StringToHash("SHIELDlevel02") },
            { ShieldState.Level3, Animator.StringToHash("SHIELDlevel03") },
            { ShieldState.Level4, Animator.StringToHash("SHIELDlevel04") },
        };

        [SerializeField] private bool m_AutoField;
        [SerializeField] private int m_StartForce;

        [SerializeField] private Animator m_Anim;
        [SerializeField] private Collider2D m_Collider;

        [SerializeField, ColorUsage(true, true)] private Color m_ColorBasic, m_ColorFull;
        [SerializeField] private float m_AbsorbAnimDuration;
        [SerializeField] private Ease m_AborsorbIn;
        [SerializeField] private SpriteRenderer[] m_ShapeRenderers;

        [SerializeField] private float m_FilledBarAnimTime;
        [SerializeField] private Ease m_FilledBarAnimEase;

        public ShieldState currentState { get; private set; }

        public int maxForce => BattleProvider.instance.forceField.maxForce;

        private BastheetCharacterController _bastheet;
        private SlicedFilledImage _filledBar;
        private Tweener _filledBarTweener;
        private Tweener _absorbTween;

        private int _currentForce;
        public int currentForce {
            get { return _currentForce; }
            private set {
                _currentForce = value;
                SetBarValue(_currentForce, true);
                UpdateForce();
            }
        }

        private bool _fieldActive;
        public bool fieldActive {
            get { return _fieldActive; }
            private set {
                _fieldActive = value;
                UpdateForce();
            }
        }

        private bool _fieldShutdown;

        public bool fieldShutdown {
            get { return _fieldShutdown; }
            set {
                _fieldShutdown = value;
                UpdateForce();
            }
        }

        public bool fieldValid => !fieldShutdown && fieldActive && currentState > ShieldState.Level0;

        private void Awake() {
            currentForce = m_StartForce;
            Init(GameCharactersManager.instance.bastheet);
        }

        private void OnDestroy() {
            _absorbTween?.Kill();
            _filledBarTweener?.Kill();
        }

        public void Init(BastheetCharacterController bastheet) {
            _bastheet = bastheet;
        }

        public void AssignFilledBar(SlicedFilledImage image) {
            _filledBar = image;
            SetBarValue(_currentForce, false);
        }

        private void OnEnable() {
            InputReader.instance.OnForceField += INPUT_OnForceField;
        }

        private void OnDisable() {
            InputReader.instance.OnForceField -= INPUT_OnForceField;
        }

        private void Update() {
            if (m_AutoField) {
                var vel = _bastheet.rb.velocity.x;
                if (vel == 0.0f && !fieldActive) {
                    fieldActive = true;
                } else if (vel != 0.0f && fieldActive) {
                    fieldActive = false;
                }
            }

            if (!fieldShutdown && _bastheet.stateMachine.currentState is not IBastheetInputState) {
                fieldShutdown = true;
            } else if (fieldShutdown && _bastheet.stateMachine.currentState is IBastheetInputState) {
                fieldShutdown = false;
            }
        }

        private void UpdateForce() {
            if (!fieldShutdown && fieldActive) {
                if (_currentForce >= maxForce) {
                    currentState = ShieldState.Level4;
                } else {
                    int levels = 4;
                    float percentFilled = Mathf.Clamp01((float)_currentForce / maxForce);
                    currentState = (ShieldState)Mathf.RoundToInt(percentFilled * levels);
                }
            } else {
                currentState = ShieldState.Disabled;
            }

            if (gameObject.activeSelf)
                m_Anim.Play(k_AnimatorHashes[currentState]);
            m_Collider.enabled = fieldValid;
        }

        public void ReduceForce(int reduction) {
            var force = currentForce;
            currentForce = Mathf.Max(currentForce - reduction, 0);
            if (force < currentForce) {
                PlayAbsorbAnim();
            }
        }

        public void AbsorbForce(int absorption) {
            var force = currentForce;
            currentForce = Mathf.Min(currentForce + absorption, maxForce);
            if (force < currentForce) {
                PlayAbsorbAnim();
            }
        }

        public void SetFieldActive(bool isFieldActive) {
            fieldActive = isFieldActive;
        }

        public void SetBarValue(int iValue, bool animated) {
            if (!_filledBar) return;

            float value = (float)iValue / maxForce;

            _filledBarTweener?.Kill();
            if (!animated) {
                _filledBar.fillAmount = value;
            } else {
                _filledBarTweener = DOVirtual.Float(_filledBar.fillAmount, value, m_FilledBarAnimTime, (x) => {
                    _filledBar.fillAmount = x;
                }).SetEase(m_FilledBarAnimEase);
            }
        }

        public void PlayAbsorbAnim() {
            if (_absorbTween?.IsPlaying() == true) return;

            _absorbTween = DOVirtual.Color(m_ColorBasic, m_ColorFull, m_AbsorbAnimDuration, (x) => {
                foreach (var shape in m_ShapeRenderers) {
                    shape.material.SetColor(k_ShapeColorID, x);
                }
            }).SetEase(m_AborsorbIn).SetLoops(2, LoopType.Yoyo).OnComplete(() => _absorbTween = null);
        }

        private void INPUT_OnForceField(bool forceFieldEnabled) {
            if (!m_AutoField)
                fieldActive = forceFieldEnabled;
        }
    }
}
