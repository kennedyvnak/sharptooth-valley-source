using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame {
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour {
        [SerializeField] private Graphic m_Fill, m_Border;
        [SerializeField] private Color m_NormalBorder, m_SelectedBorder;
        [SerializeField] private float m_NormalHeight, m_SelectedHeight, m_TransitionDuration;

        private Tweener _sizeTweener, _borderTweener;
        private bool _active;

        public void Set(bool selected, bool force) {
            if (selected)
                Tween(force, m_SelectedHeight, m_SelectedBorder, Helpers.CameraInEase);
            else
                Tween(force, m_NormalHeight, m_NormalBorder, Helpers.CameraOutEase);
            _active = selected;
        }

        public bool IsActive() => _active;

        private void Tween(bool force, float y, Color border, Ease ease) {
            if (force) {
                m_Fill.rectTransform.sizeDelta = new Vector2(m_Fill.rectTransform.sizeDelta.x, y);
                m_Border.color = border;
                return;
            }

            _sizeTweener.Kill();
            _sizeTweener = m_Fill.rectTransform.DOSizeDelta(new Vector2(m_Fill.rectTransform.sizeDelta.x, y), m_TransitionDuration).SetEase(ease);
            _borderTweener.Kill();
            _borderTweener = m_Border.DOColor(border, m_TransitionDuration).SetEase(ease);
        }
    }
}
