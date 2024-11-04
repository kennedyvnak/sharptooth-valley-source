using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.Inventory.UI {
    public enum InventoryItemState { NotFound, Found, FoundInThisSave }

    public class InventoryItemThumb : Button {
        private static Material _lightenedMat, _blackMat;

        [SerializeField] private Image m_ItemThumbImage;
        [SerializeField] private Image m_ThumbBorderImage;
        [SerializeField] private UnityEvent<InventoryItemThumb> m_OnThumbClick;

        private InventoryItem _item;

        public InventoryItemState currentState { get; private set; }

        public InventoryItem item {
            get => _item;
            set {
                _item = value;
                UpdateDisplay();
            }
        }

        public UnityEvent<InventoryItemThumb> onThumbClick => m_OnThumbClick;

        public CanvasGroup group { get; private set; }

        protected override void Awake() {
            base.Awake();
            if (!Application.isPlaying) return;
            onClick.AddListener(() => m_OnThumbClick?.Invoke(this));
            group = GetComponent<CanvasGroup>();
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (!Application.isPlaying) return;
            ChangeItemState(InventoryManager.instance.GetItemState(item));
            InventoryManager.instance.onItemStateChange.AddListener(EVENT_OnAchievementChange);
        }

        protected override void OnDisable() {
            if (Application.isPlaying && InventoryManager.instance) InventoryManager.instance.onItemStateChange.RemoveListener(EVENT_OnAchievementChange);
            base.OnDisable();
        }

        public void UpdateDisplay() {
            if (!item) {
                GameLogger.LogError($"Trying to update a InventoryItemThumb but the field '{nameof(item)}' is null.", this);
                return;
            }

            m_ItemThumbImage.sprite = item.thumb;
        }

        private void ChangeItemState(InventoryItemState state) {
            currentState = state;
            if (state == InventoryItemState.FoundInThisSave) {
                m_ItemThumbImage.material = _lightenedMat;
                group.interactable = true;
                group.blocksRaycasts = true;
            } else {
                m_ItemThumbImage.material = _blackMat;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            m_OnThumbClick?.Invoke(this);
        }

        private void EVENT_OnAchievementChange(InventoryItem changedItem, InventoryItemState state) {
            if (changedItem == item)
                ChangeItemState(state);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetupMaterials() {
            Shader grayScaleShader = Shader.Find("Custom/UI/Grayscale");

            CreateMaterial(out _lightenedMat, 0.0f, 1.0f);
            CreateMaterial(out _blackMat, 0.0f, 0.0f);

            void CreateMaterial(out Material material, float effect, float brightness) {
                material = new Material(grayScaleShader);
                material.SetFloat("_EffectAmount", effect);
                material.SetFloat("_BrightnessAmount", brightness);
            }
        }
    }
}
