using DG.Tweening;
using NFHGame.Input;
using NFHGame.Screens;
using NFHGame.Serialization;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame {
    public class SavesScreen : Singleton<SavesScreen>, IScreen {
        [Serializable]
        public class SlotSceneData {
            public string name;
            public Expression[] expressions;
            public Sprite[] sprites;
        }

        bool IScreen.screenActive { get; set; }
        bool IScreen.poppedByInput => true;
        bool IScreen.dontSelectOnActive => false;
        GameObject IScreen.selectOnOpen => m_SavesSlotsTab.gameObject;

        [SerializeField] private CanvasGroup m_Group;
        [SerializeField] private bool m_CacheOnAwake;
        [SerializeField] private bool m_CacheAutoSave;

        [Header("Slots")]
        [SerializeField] private SaveSlot m_SlotPrefab;
        [SerializeField] private TabButton m_SavesSlotsTab, m_AutoSavesTab;
        [SerializeField] private Transform m_SaveSlotsParent, m_AutoSavesParent;
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private SerializedDictionary<string, SlotSceneData> m_SceneNames;
        [SerializeField] private float m_FocusSlotTime;
        [SerializeField, TextArea] private string m_EmptyText, m_SlotText;

        [Header("Map")]
        [SerializeField] private Image m_ScreenIconImage;
        [SerializeField] private ProgressMap m_ProgressMap;

        [Header("Operation Buttons")]
        [SerializeField] private Button m_CancelButton;

        [Header("Deletetion")]
        [SerializeField, TextArea] private string m_DeleteSavePopupText;
        [SerializeField] private float m_DeleteSavePopupDelay;

        public GameData[] cachedGameData { get; private set; }

        private SaveSlot[] _slots;
        private SaveSlot[] _sortedSlots;

        public bool hasAnyGameData => cachedGameData.Length > 0;

        private System.Action<int> _selectUser;
        private int _currentSelected;
        private bool _allowEmptyUser;
        private Coroutine _moveToSlotCoroutine;

        protected override void Awake() {
            base.Awake();
            if (m_CacheOnAwake)
                CacheGameData();

            m_CancelButton.onClick.AddListener(CancelOperation);
        }

        private void Start() {
            m_SavesSlotsTab.GetComponent<Button>().onClick.AddListener(SelectSaveSlots);
            m_AutoSavesTab.GetComponent<Button>().onClick.AddListener(SelectAutoSaves);
        }

        public void GetUser(bool allowEmptyUser, System.Action<int> selectedUser) {
            foreach (var slot in _slots)
                slot.group.interactable = allowEmptyUser || slot.cachedData != null;
            m_AutoSavesTab.gameObject.SetActive(!allowEmptyUser);
            _selectUser = selectedUser;
            _allowEmptyUser = allowEmptyUser;
            m_SavesSlotsTab.GetComponent<Button>().SetDownNav(GenSlotsNav(_slots, UserManager.UserSavesCount));

            ScreenManager.instance.PushScreen(this);
        }

        public void DeleteSlot(SaveSlot slot) {
            EventSystem.current.SetSelectedGameObject(null);
            m_Group.interactable = false;
            ConfirmPopup.instance.Popup(m_DeleteSavePopupText, "Delete", "Cancel", m_DeleteSavePopupDelay, (v) => {
                if (v) {
                    DataManager.instance.dataHandler.DeleteUser(slot.id);
                    UpdateSlot(slot.id, null);
                    var datas = cachedGameData.ToList();
                    var idx = datas.FindIndex((gd) => gd.userId == slot.id);
                    if (idx != -1) {
                        datas.RemoveAt(idx);
                        cachedGameData = datas.ToArray();
                    }
                    if (TitleScreen.instance)
                        TitleScreen.instance.UpdateLoadButtons(hasAnyGameData);
                    OrderSlots();
                }
                ConfirmPopup.instance.ClosePopup().onComplete += () => m_Group.interactable = true;
            });
        }

        public void UpdateSlot(int user, GameData data) {
            var slot = _slots[user];
            slot.Set(user, data, GetSlotName(data, user));
            if (_currentSelected == user) {
                if (data == null) {
                    m_ProgressMap.ResetData();
                    _currentSelected = -1;
                } else {
                    m_ProgressMap.SetData(data);
                    _currentSelected = user;
                }
            }

            slot.group.interactable = _allowEmptyUser || data != null;
        }

        IEnumerator IScreen.CloseScreen() {
            _selectUser?.Invoke(UserManager.NoUserId);
            _selectUser = null;
            yield return m_Group.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);            
        }

        IEnumerator IScreen.OpenScreen() {
            CacheGameData();
            if (!_allowEmptyUser)
                m_AutoSavesTab.GetComponent<Button>().SetDownNav(GenSlotsNav(_sortedSlots, _sortedSlots.Length));
            ToggleTab(false, true);
            m_ProgressMap.ResetData();
            _currentSelected = -1;

            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_Group.ToggleScreen(true).WaitForCompletion();
        }

        private void SetScreen(SaveSlot slot) {
            if (slot.cachedData != null) m_ProgressMap.SetData(slot.cachedData);
            else m_ProgressMap.ResetData();

            m_ScreenIconImage.sprite = null;
            if (slot.cachedData != null) {
                m_ScreenIconImage.enabled = true; 
                string val = slot.cachedData.state == null ? null : Path.GetFileNameWithoutExtension(slot.cachedData.state.sceneRef);
                if (!string.IsNullOrWhiteSpace(val) && m_SceneNames.TryGetValue(val, out var sceneData)) {
                    for (int i = 0; i < sceneData.expressions.Length; i++) {
                        if (!sceneData.expressions[i].Get(slot.cachedData)) continue;
                        m_ScreenIconImage.sprite = sceneData.sprites[i];
                        break;
                    }
                }
            }
            
            m_ScreenIconImage.enabled = m_ScreenIconImage.sprite;
            _currentSelected = slot.id;
        }

        private void ConfirmOperation(int id) {
            _selectUser?.Invoke(id);
            _selectUser = null;
        }

        private void CancelOperation() {
            _selectUser?.Invoke(UserManager.NoUserId);
            _selectUser = null;
            ScreenManager.instance.PopScreen();
        }

        private void CacheGameData() {
            if (cachedGameData != null) return;

            cachedGameData = DataManager.instance.GetAllGameData(m_CacheAutoSave ? null : false);
            int dataIndex = 0;
            _slots = new SaveSlot[m_CacheAutoSave ? (UserManager.MaxUserId + 1) : UserManager.UserSavesCount];
            for (int i = 0; i < _slots.Length; i++) {
                var data = dataIndex < cachedGameData.Length ? cachedGameData[dataIndex] : null;
                if (data != null && data.userId == i) {
                    _slots[i] = CreateSlot(i, data, GetSlotName(data, i));
                    dataIndex++;
                } else {
                    _slots[i] = CreateSlot(i, null, GetSlotName(null, i));
                }
            }

            OrderSlots();
        }

        private string GetSlotName(GameData data, int id) {
            return data != null ? string.Format(m_SlotText, GetSceneName(), GetSerializeTime(), GetTimePlayed()) : string.Format(m_EmptyText, id);

            string GetSceneName() {
                string val = data.state == null ? null : Path.GetFileNameWithoutExtension(data.state.sceneRef);
                if (!string.IsNullOrWhiteSpace(val) && m_SceneNames.TryGetValue(val, out var sceneData))
                    return sceneData.name;
                else
                    return "???";
            }

            string GetSerializeTime() {
                return data.serializationDate.ToString("MM/dd/yy HH:mm");
            }

            string GetTimePlayed() {
                TimeSpan time = TimeSpan.FromSeconds(data.playTime);
                if (time.TotalHours < 2.0) return $"{time.Minutes}m";
                else return $"{time.Hours}h{time.Minutes}m";
            }
        }

        private SaveSlot CreateSlot(int id, GameData data, string text) {
            var slotParent = GetParent(id);
            var b = Instantiate(m_SlotPrefab, slotParent);
            b.name = $"Slot_{id}";
            b.Set(id, data, text);
            b.gameObject.SetActive(true);
            b.SlotClick += Event_SlotClick;
            b.SlotSelected += Event_SlotSelected;
            b.DeleteClick += Event_DeleteClick;
            return b;
        }

        private void OrderSlots() {
            if (!m_CacheAutoSave) return;

            _sortedSlots = new SaveSlot[UserManager.AutoSavesCount];
            Array.Copy(_slots, UserManager.UserSavesCount, _sortedSlots, 0, _sortedSlots.Length);
            Array.Sort(_sortedSlots, (x, y) => {
                if (y.cachedData == null && x.cachedData == null) return x.id.CompareTo(y.id);
                if (y.cachedData == null) return -1;
                if (x.cachedData == null) return 1;

                return y.cachedData.serializationDate.CompareTo(x.cachedData.serializationDate);
            });

            for (int i = 0; i < _sortedSlots.Length; i++)
                _sortedSlots[i].transform.SetSiblingIndex(i);
        }

        private void Event_SlotClick(SaveSlot slot) {
            ConfirmOperation(slot.id);
        }

        private void Event_SlotSelected(SaveSlot slot) {
            SetScreen(slot);
        }

        private void Event_DeleteClick(SaveSlot slot) {
            DeleteSlot(slot);
        }

        private Transform GetParent(int id) {
            return id < UserManager.UserSavesCount ? m_SaveSlotsParent : m_AutoSavesParent;
        }

        public void SelectSaveSlots() => ToggleTab(false, false);

        public void SelectAutoSaves() => ToggleTab(true, false);

        private void ToggleTab(bool auto, bool force) {
            m_AutoSavesTab.Set(auto, force);
            m_SavesSlotsTab.Set(!auto, force);
            m_AutoSavesParent.gameObject.SetActive(auto);
            m_SaveSlotsParent.gameObject.SetActive(!auto);
            m_ScrollRect.normalizedPosition = Vector2.up;
            m_ScrollRect.velocity = Vector2.zero;
        }

        public void MoveToSlot(RectTransform button) {
            this.EnsureCoroutineStopped(ref _moveToSlotCoroutine);
            _moveToSlotCoroutine = StartCoroutine(m_ScrollRect.FocusOnItemCoroutine(button, m_FocusSlotTime));
        }

        private Button GenSlotsNav(SaveSlot[] buff, int genLenght) {
            SaveSlot lastSlot = null;
            SaveSlot firstSlot = null;
            for (int i = 0; i < genLenght; i++) {
                SaveSlot slot = null;
                while ((i < genLenght) && !(slot = buff[i]).group.interactable) i++;
                if (i >= genLenght && slot.group.interactable == false) return null;
                while ((i + 1 < genLenght) && !buff[i + 1].group.interactable) i++;

                var downSlot = (i + 1>= genLenght) ? null : buff[i + 1];
                slot.slotButton.SetNavigation(up: lastSlot ? lastSlot.slotButton : m_CancelButton, down: downSlot ? downSlot.slotButton : null, left: m_CancelButton, right: slot.cachedData != null ? slot.deleteButton : null);
                slot.deleteButton.SetNavigation(up: lastSlot ? lastSlot.deleteButton : m_CancelButton, down: downSlot ? downSlot.deleteButton : null, left: slot.slotButton);
                lastSlot = slot;
                if (!firstSlot) firstSlot = slot;
            }
            return firstSlot.slotButton;
        }
    }
}
