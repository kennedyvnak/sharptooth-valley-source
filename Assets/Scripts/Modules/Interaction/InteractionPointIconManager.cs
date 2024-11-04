using NFHGame.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.Interaction {
    public class InteractionPointIconManager : Singleton<InteractionPointIconManager> {
        public enum Icon { Eye, Hand, Foot, Door }

        [SerializeField] private Image m_EyeImage;
        [SerializeField] private Image m_HandImage;
        [SerializeField] private Image m_FootImage;
        [SerializeField] private Image m_DoorImage;
        [SerializeField] private Vector2 m_IconMouseOffset;

        private List<int> _blocksID = new List<int>();
        private Image _currentIcon;

        public Vector2 mouseScreenPosition { get; private set; }

        private void OnEnable() {
            _currentIcon = m_EyeImage;
            InputReader.instance.OnPointerPosition += INPUT_OnPointerPosition;
        }

        private void OnDisable() {
            InputReader.instance.OnPointerPosition -= INPUT_OnPointerPosition;
        }

        public void InsertBlock(int blockID) {
            _blocksID.Add(blockID);
            UpdateTrack();
        }

        public void RemoveBlock(int blockID) {
            if (_blocksID.Remove(blockID))
                UpdateTrack();
        }

        public void SetIcon(Icon icon) {
            var enabled = _currentIcon.enabled;
            _currentIcon.enabled = false;
            _currentIcon = icon switch {
                Icon.Eye => m_EyeImage,
                Icon.Hand => m_HandImage,
                Icon.Foot => m_FootImage,
                Icon.Door => m_DoorImage,
                _ => throw new System.NotImplementedException(),
            };
            _currentIcon.enabled = enabled;
        }

        private void UpdateTrack() {
            _currentIcon.enabled = _blocksID.Count > 0;
        }

        private void Update() {
            _currentIcon.rectTransform.anchoredPosition = mouseScreenPosition + m_IconMouseOffset;
        }

        private void INPUT_OnPointerPosition(Vector2 screenPosition) {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPosition, Helpers.mainCamera, out var localPoint)) {
                mouseScreenPosition = localPoint;
            }

        }
    }
}
