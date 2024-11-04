using System.Collections;
using UnityEngine;

namespace NFHGame {
    public class BattleOffFieldDinnerArrow : Singleton<BattleOffFieldDinnerArrow> {
        [SerializeField] private Transform m_Dinner;
        [SerializeField] private Transform m_Camera;
        [SerializeField] private float m_OffFieldDistance;
        [SerializeField] private CanvasGroup m_ArrowGroup;
        [SerializeField] private float m_CanvasOffsetX;

        public bool arrowEnabled { get; private set; }
        public bool arrowToLeft { get; private set; }

        private void Update() {
            bool oldEnabled = arrowEnabled;
            bool oldLeft = arrowToLeft;

            var dinnerX = m_Dinner.transform.position.x;
            var cameraX = m_Camera.transform.position.x;

            arrowToLeft = dinnerX < cameraX;
            arrowEnabled = Mathf.Abs(dinnerX - cameraX) > m_OffFieldDistance;

            if (oldLeft != arrowToLeft) {
                var t = (RectTransform)transform;
                var pos = t.anchoredPosition;
                pos.x = arrowToLeft ? -m_CanvasOffsetX : m_CanvasOffsetX;
                t.anchoredPosition = pos;

                var scale = t.localScale;
                scale.x = arrowToLeft ? 1 : -1;
                t.localScale = scale;
            }

            if (oldEnabled != arrowEnabled) {
                m_ArrowGroup.ToggleGroup(arrowEnabled);
            }
        }
    }
}
