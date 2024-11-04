using DG.Tweening;
using NFHGame.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.HaloManager {
    public class HaloToggleButton : Singleton<HaloToggleButton> {
        [SerializeField] private Image m_Block;

        [Header("Minigame")]
        [SerializeField] private CanvasGroup m_MinigameGroup;
        [SerializeField] private Image m_MinigameFill;
        [SerializeField] private float m_DecreaseDelta;
        [SerializeField] private float m_StepDelta;

        [Header("Minigame Pulse")]
        [SerializeField] private Vector3 m_Punch;
        [SerializeField] private float m_PunchDuration;

        private float _currentMinigameVal;
        private System.Action _onMinigameFinish;
        private Sequence _minigameSequence;

        private void Start() {
            m_Block.enabled = HaloManager.instance.blackout;
            HaloManager.instance.haloBlackoutToggled.AddListener(BlackoutChanged);
        }

        protected override void OnDestroy() {
            if (HaloManager.instance)
                HaloManager.instance.haloBlackoutToggled.RemoveListener(BlackoutChanged);
            base.OnDestroy();
        }

        public void Toggle() {
            HaloManager.instance.Toggle();
        }

        public void HaloMinigame(System.Action onFinish) {
            _onMinigameFinish = onFinish;
            InputReader.instance.QTE_ToggleHalo += EVENT_ToggleHalo;
            InputReader.instance.PushMap(InputReader.InputMap.QuickTimeEvents);

            var groupTransform = (RectTransform)m_MinigameGroup.gameObject.transform;
            _minigameSequence = DOTween.Sequence()
                .Append(groupTransform.DOScale(m_Punch, m_PunchDuration).SetDelay(m_PunchDuration))
                .Append(groupTransform.DOScale(Vector3.one, m_PunchDuration))
                .SetLoops(-1, LoopType.Restart);

            m_MinigameGroup.gameObject.SetActive(true);
            m_MinigameGroup.ToggleGroupAnimated(true, 1.0f / 3.0f);
            StartCoroutine(MinigameCoroutine());
        }

        private void BlackoutChanged(bool blackout) {
            m_Block.enabled = blackout;
        }

        private void EVENT_ToggleHalo() {
            _currentMinigameVal += m_StepDelta;
        }

        private IEnumerator MinigameCoroutine() {
            while (_currentMinigameVal < 1.0f) {
                yield return null;
                _currentMinigameVal -= m_DecreaseDelta * Time.deltaTime;
                m_MinigameFill.fillAmount = _currentMinigameVal;
            }

            InputReader.instance.QTE_ToggleHalo -= EVENT_ToggleHalo;
            InputReader.instance.PopMap(InputReader.InputMap.QuickTimeEvents);
            _onMinigameFinish?.Invoke();

            HaloManager.instance.SetActive(true);

            m_MinigameFill.fillAmount = 1.0f;
            m_MinigameGroup.ToggleGroupAnimated(false, 1.0f / 3.0f).onComplete += () => {
                m_MinigameGroup.gameObject.SetActive(false);
                _minigameSequence.Kill();
            };
        }
    }
}