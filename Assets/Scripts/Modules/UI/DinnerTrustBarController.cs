using DG.Tweening;
using NFHGame.Animations;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.External;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DinnerTrust {
    public class DinnerTrustBarController : Singleton<DinnerTrustBarController> {
        [SerializeField] private int m_MaxTrustInBar;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private SlicedFilledImage m_Fill;
        [SerializeField] private AudioObject m_LostTrustSound;
        [SerializeField] private AudioObject m_GainTrustSound;
        [SerializeField] private float m_AnimTime;
        [SerializeField] private Ease m_AnimEase;
        [SerializeField] private SpriteArrayAnimator m_PlimAnim;
        [SerializeField] private Image m_PlimImage;
        [SerializeField] private Sprite[] m_PlimSprites;
        [SerializeField] private TextMeshProUGUI m_ExtraPointsText;

        [SerializeField] private Gradient m_ColorTime;

        [SerializeField] private AudioSource m_SoundSource;

        private Sprite[] _plimsReverse;

        private int _oldValue;
        private Tweener _tweener;
        private Tweener _colorTweener;

        private int _anim = 0;

        private Tween _madnessTweener;

        public CanvasGroup canvasGroup => m_CanvasGroup;

        private void Start() {
            _plimsReverse = m_PlimSprites.Clone() as Sprite[];
            Array.Reverse(_plimsReverse);

            var globalVars = ArticyVariables.globalVariables;
            _oldValue = globalVars.trustPoints.dinnerPoints;
            SetValue((float)_oldValue / m_MaxTrustInBar, false);
            m_ExtraPointsText.text = _oldValue > m_MaxTrustInBar ? $"+{_oldValue - m_MaxTrustInBar}" : string.Empty;
            ArticyManager.notifications.AddListener("trustPoints.dinnerPoints", PointsChanged);
        }

        protected override void OnDestroy() {
            _tweener?.Kill();
            _colorTweener?.Kill();
            base.OnDestroy();
            ArticyManager.notifications.RemoveListener("trustPoints.dinnerPoints", PointsChanged);
        }

        public void SetValue(float value, bool animated) {
            _tweener?.Kill();
            _colorTweener?.Kill();
            if (!animated) {
                m_Fill.color = m_ColorTime.Evaluate(1.0f - value);
                m_Fill.fillAmount = value;
            } else {
                _tweener = DOVirtual.Float(m_Fill.fillAmount, value, m_AnimTime, (x) => {
                    m_Fill.fillAmount = x;
                }).SetEase(m_AnimEase);
                _colorTweener = DOVirtual.Float(m_Fill.fillAmount, value, m_AnimTime, (x) => {
                    m_Fill.color = m_ColorTime.Evaluate(1.0f - x);
                });
            }
        }

        public void PointsChanged(string name, object oValue) {
            int value = (int)oValue;

            if (_oldValue > value) { // lost trust
                PlimDownAnim();
                m_LostTrustSound.CloneToSource(m_SoundSource);
                m_SoundSource.Play();
            } else if (_oldValue < value) { // gain trust
                PlimUpAnim();
                m_GainTrustSound.CloneToSource(m_SoundSource);
                m_SoundSource.Play();
            }

            m_ExtraPointsText.text = value > m_MaxTrustInBar ? $"+{value - m_MaxTrustInBar}" : string.Empty;
            SetValue((float)value / m_MaxTrustInBar, true);
            _oldValue = value;
        }

        public void PlimUpAnim() => PlimAnim(1, 0.0f);

        public void PlimDownAnim() => PlimAnim(-1, 1.0f);

        private void PlimAnim(int dir, float time) {
            _anim = dir;
            m_PlimImage.color = m_ColorTime.Evaluate(time);
            m_PlimImage.enabled = true;
            m_PlimAnim.values = m_PlimSprites;
            m_PlimAnim.Replay();
        }

        public void PlimAnimLoopFinished() {
            if (_anim == 0) {
                m_PlimImage.enabled = false;
                return;
            }

            m_PlimAnim.values = _plimsReverse;
            m_PlimAnim.Replay();
            _anim = 0;
        }

        public void TrustMadness(float ratio) {
            PointsChanged(null, ArticyVariables.globalVariables.trustPoints.dinnerPoints + UnityEngine.Random.Range(-3, 3));
            _madnessTweener = DOVirtual.DelayedCall(ratio, () => {
                PointsChanged(null, ArticyVariables.globalVariables.trustPoints.dinnerPoints + UnityEngine.Random.Range(-3, 3));
            }).SetLoops(-1);
        }

        public void EndMadness() {
            _madnessTweener.OnStepComplete(() => {
                _madnessTweener.Kill();
                PointsChanged(null, ArticyVariables.globalVariables.trustPoints.dinnerPoints);
            });
        }
    }
}
