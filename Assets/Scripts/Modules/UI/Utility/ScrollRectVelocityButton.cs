using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.Inventory.UI {
    public class ScrollRectVelocityButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private Vector2 m_Velocity;

        private bool _isDown;

        private void Update() {
            if (!_isDown) return;

            m_ScrollRect.velocity = m_Velocity;
        }

        public void OnPointerDown(PointerEventData eventData) {
            _isDown = true;
        }

        public void OnPointerUp(PointerEventData eventData) {
            _isDown = false;
        }
    }
}