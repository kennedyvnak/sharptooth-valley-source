using DG.Tweening;
using NFHGame.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame {
    public class SaveSlot : MonoBehaviour, IPointerEnterHandler {
        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private Button m_Slot, m_Delete;
        [SerializeField] private TextMeshProUGUI m_SlotText;
        [SerializeField] private float m_MoveDuration;
        [SerializeField] private Button m_SlotButton, m_DeleteButton;

        public event System.Action<SaveSlot> SlotClick;
        public event System.Action<SaveSlot> SlotSelected;
        public event System.Action<SaveSlot> DeleteClick;
        
        public int id { get; private set; }
        public GameData cachedData { get; private set; }

        public Button slotButton { get => m_SlotButton; set => m_SlotButton = value; }
        public Button deleteButton { get => m_DeleteButton; set => m_DeleteButton = value; }
        public CanvasGroup group => m_Group;

        private Tweener _moveTweener;
        private bool _pushed, _pointEnter;

        private void OnEnable() {
            m_Slot.onClick.AddListener(() => SlotClick?.Invoke(this));
            m_Delete.onClick.AddListener(() => DeleteClick?.Invoke(this));
        }

        public void Set(int id, GameData data, string text) {
            m_SlotText.text = text;
            this.id = id;
            this.cachedData = data;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _pointEnter = true;
            EventSystem.current.SetSelectedGameObject(slotButton.gameObject);
        }

        public void Push() {
            SlotSelected?.Invoke(this);

            if (!_pointEnter) {
                _pointEnter = false;
                SavesScreen.instance.MoveToSlot((RectTransform)transform);
            }

            if (cachedData == null || _pushed) return;
            _pushed = true;
            _moveTweener.Kill();
            _moveTweener = m_Content.DOAnchorPosX(-((RectTransform)m_Delete.transform).sizeDelta.x, m_MoveDuration).SetEase(Helpers.CameraInEase).SetUpdate(true);
        }

        public void Pop() {
            if (cachedData == null || !_pushed) return;
            _pushed = false;
            _moveTweener.Kill();
            _moveTweener = m_Content.DOAnchorPosX(0.0f, m_MoveDuration).SetEase(Helpers.CameraOutEase).SetUpdate(true);
        }
    }
}
