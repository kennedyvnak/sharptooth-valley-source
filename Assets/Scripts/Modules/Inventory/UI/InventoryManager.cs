using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NFHGame.AchievementsManagement;
using NFHGame.AchievementsManagement.UI;
using NFHGame.AudioManagement;
using NFHGame.DialogueSystem;
using NFHGame.Input;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.SceneManagement.SceneState;
using NFHGame.Screens;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.Inventory.UI {
    public class InventoryManager : Singleton<InventoryManager>, IScreen {
        [SerializeField] private InventoryItemThumb m_ItemThumbPrefab;

        [Header("UI")]
        [SerializeField] private Image m_BlockInputMask;
        [SerializeField] private float m_FocusSpeed;
        [SerializeField] private CanvasGroup m_InventoryGroup;
        [SerializeField] private ScrollRect m_ItemThumbScrollRect;
        [SerializeField] private RectTransform m_ItemThumbContent;
        [SerializeField] private Image m_ItemDisplayImage;
        [SerializeField] private TMPro.TextMeshProUGUI m_ItemNameText;
        [SerializeField] private InventoryItemDialogue m_ItemDialogue;
        [SerializeField] private AudioObject m_BackpackSound;
        [SerializeField] private AudioSource m_BackpackSource;
        [SerializeField] private RectTransform m_BastheetHeadObject;

        [Space]
        [SerializeField] private Button m_CloseInventoryButton, m_OpenAchievementsButton;

        [SerializeField] private UnityEvent<InventoryItem, InventoryItemState> m_OnItemStateChange;

        public Image itemDisplayImage => m_ItemDisplayImage;
        public Button openAchievementsButton => m_OpenAchievementsButton;
        public InventoryItemDialogue itemDialogue => m_ItemDialogue;

        private InventoryItem _activeItem;
        private Coroutine _focusInThumbCoroutine;

        private bool _screenActive;
        private bool _interaction = true;
        bool IScreen.screenActive { get => _screenActive; set => _screenActive = value; }
        public bool poppedByInput => !DialogueManager.instance.executionEngine.running;
        GameObject IScreen.selectOnOpen => GetFirstValidThumb();
        bool IScreen.dontSelectOnActive => true;

        public UnityEvent<InventoryItem, InventoryItemState> onItemStateChange => m_OnItemStateChange;

        private void Start() {
            foreach (var item in InventoryDatabase.instance.data.Values) {
                var itemThumb = Instantiate(m_ItemThumbPrefab, m_ItemThumbContent);
                itemThumb.item = item;
                itemThumb.onThumbClick.AddListener(OnThumbClicked);
            }

            m_OpenAchievementsButton.onClick.AddListener(PerformOpenAchievements);
            m_CloseInventoryButton.onClick.AddListener(PerformCloseInventory);

            if (SceneStateController.instance.stateData) {
                m_BastheetHeadObject.anchoredPosition = SceneStateController.instance.stateData.bastheetHeadMapPosition;
            }
        }

        private void OnEnable() {
            InputReader.instance.OnOpenInventory += INPUT_OpenInventory;
            InputReader.instance.OnCloseInventory += INPUT_CloseInventory;
            InventoryOpener.instance.button.interactable = true;
        }

        private void OnDisable() {
            InputReader.instance.OnOpenInventory -= INPUT_OpenInventory;
            InputReader.instance.OnCloseInventory -= INPUT_CloseInventory;
            InventoryOpener.instance.button.interactable = false;
        }

        public void SetItem(InventoryItemThumb thumb) {
            var item = thumb ? thumb.item : null;
            if (item && _activeItem == item) return;
            SetupItem(thumb, item, out bool hasItem);

            if (hasItem) {
                this.EnsureCoroutineStopped(ref _focusInThumbCoroutine);
                _focusInThumbCoroutine = StartCoroutine(m_ItemThumbScrollRect.FocusOnItemCoroutine((RectTransform)thumb.transform, m_FocusSpeed));
            }
        }

        public void InitItem(InventoryItemThumb thumb) {
            SetupItem(thumb, thumb ? thumb.item : null, out bool hasItem);

            if (!hasItem) {
                var pos = m_ItemThumbScrollRect.content.anchoredPosition;
                pos.y = 0.0f;
                m_ItemThumbScrollRect.content.anchoredPosition = pos;
            } else {
                m_ItemThumbScrollRect.FocusOnItem((RectTransform)thumb.transform);
            }
        }

        private void SetupItem(InventoryItemThumb thumb, InventoryItem item, out bool hasItem) {
            hasItem = item;

            m_ItemDisplayImage.enabled = hasItem;
            m_ItemDisplayImage.sprite = hasItem ? item.display : null;
            m_ItemNameText.text = hasItem ? item.itemName : string.Empty;
            m_ItemDialogue.SetItem(item, thumb, m_OpenAchievementsButton);

            _activeItem = item;
            
            if (hasItem)
                StartCoroutine(Helpers.DelayForFramesCoroutine(1, () => thumb.SetRightNav(m_ItemDialogue.transform.GetChild(0).GetComponent<Selectable>())));
        }

        public GameObject GetFirstValidThumb() {
            var firstChild = m_ItemThumbContent.transform.GetChild(0);
            return GetItemState(firstChild.GetComponent<InventoryItemThumb>().item) == InventoryItemState.FoundInThisSave ? firstChild.gameObject : m_OpenAchievementsButton.gameObject;
        }

        public void ToggleInput(bool enabled) {
            m_BlockInputMask.raycastTarget = !enabled;
        }

        public void AddItem(InventoryItem item) {
            var globalData = DataManager.instance.globalGameData;

            var itemKey = InventoryDatabase.instance.GetKeyForData(item);

            if (!globalData.foundItems.Contains(itemKey)) {
                globalData.foundItems.Add(itemKey);
                DataManager.instance.SaveGlobal();
            }

            GameKeysManager.instance.ToggleGameKey(itemKey, true);

            onItemStateChange?.Invoke(item, InventoryItemState.FoundInThisSave);
        }

        public void RemoveItem(InventoryItem item) {
            var itemKey = InventoryDatabase.instance.GetKeyForData(item);
            GameKeysManager.instance.ToggleGameKey(itemKey, false);
            onItemStateChange?.Invoke(item, DataManager.instance.globalGameData.foundItems.Contains(itemKey) ? InventoryItemState.Found : InventoryItemState.NotFound);
        }

        public InventoryItemState GetItemState(InventoryItem item) {
            var itemKey = InventoryDatabase.instance.GetKeyForData(item);

            var global = DataManager.instance.globalGameData.foundItems.Contains(itemKey);
            var local = GameKeysManager.instance.HaveGameKey(itemKey);

            return local ? InventoryItemState.FoundInThisSave : global ? InventoryItemState.Found : InventoryItemState.NotFound;
        }

        IEnumerator IScreen.OpenScreen() {
            transform.GetChild(0).gameObject.SetActive(true);
            OrderItemsTransform();

            var item = GetFirstValidThumb();
            EventSystem.current.SetSelectedGameObject(item);
            if (_focusInThumbCoroutine != null)
                StopCoroutine(_focusInThumbCoroutine);
            InitItem(item.GetComponent<InventoryItemThumb>());

            yield return m_InventoryGroup.ToggleScreen(true).WaitForCompletion();
            if (!_interaction)
                m_InventoryGroup.interactable = false;
        }

        IEnumerator IScreen.CloseScreen() {
            yield return m_InventoryGroup.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);
        }

        private void OrderItemsTransform() {
            List<InventoryItemThumb> children = new List<InventoryItemThumb>();
            foreach (Transform child in m_ItemThumbContent.transform) {
                var thumb = child.GetComponent<InventoryItemThumb>();
                if (thumb.item.hiddenItem && thumb.currentState != InventoryItemState.FoundInThisSave) {
                    thumb.gameObject.SetActive(false);
                    continue;
                }
                thumb.gameObject.SetActive(true);
                children.Add(thumb);
            }

            children.Sort((a, b) => {
                return GetStateCompare(a.currentState).CompareTo(GetStateCompare(b.currentState));
                int GetStateCompare(InventoryItemState state) => state switch {
                    InventoryItemState.FoundInThisSave => -1,
                    InventoryItemState.Found => 1,
                    _ => 0,
                };
            });

            for (int i = 0; i < children.Count; i++) {
                var child = children[i];
                child.transform.SetSiblingIndex(i);
                
                child.SetNavigation(
                    up: i == 0 ? m_OpenAchievementsButton : children[i - 1],
                    down: i + 1 == children.Count || children[i + 1].currentState != InventoryItemState.FoundInThisSave ? null : children[i + 1], 
                    left: child.navigation.selectOnLeft, 
                    right: null
                );
            }

            var upperItem = children[0].currentState != InventoryItemState.FoundInThisSave ? null : children[0];
            m_OpenAchievementsButton.SetNavigation(down: upperItem, left: upperItem, right: m_CloseInventoryButton);
            m_CloseInventoryButton.SetNavigation(left: m_OpenAchievementsButton);
        }

        private void PerformOpenAchievements() {
            ScreenManager.instance.PushScreen(AchievementsScreen.instance);
        }

        private void PerformCloseInventory() {
            ScreenManager.instance.PopAll();
        }

        private void OnThumbClicked(InventoryItemThumb itemThumb) {
            if (itemThumb.item) {
                SetItem(itemThumb);
            }
        }

        private void INPUT_OpenInventory() {
            if (_screenActive) return;

            ScreenManager.instance.PushScreen(this);
        }

        private void INPUT_CloseInventory() {
            if (!_screenActive || !poppedByInput || ScreenManager.instance.isBusy) return;

            ScreenManager.instance.PopScreen();
        }

        public void SetInteraction(bool canInteract) {
            m_InventoryGroup.interactable = canInteract;
            _interaction = canInteract;
        }
    }
}
