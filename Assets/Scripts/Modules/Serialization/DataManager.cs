using DG.Tweening;
using NFHGame.Characters;
using NFHGame.Configs;
using NFHGame.SceneManagement;
using NFHGame.SceneManagement.SceneState;
using NFHGame.ScriptableSingletons;
using NFHGame.Serialization.Handlers;
using NFHGame.Serialization.States;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NFHGame.Serialization {
    public class DataManager : ScriptableSingleton<DataManager>, IBootableSingleton {
        [Flags] public enum ToggleIconFlag { None = 0, Save = 1 << 0, Global = 1 << 1, Injection = 1 << 2 }

        [SerializeField] private GameDataAsset m_DefaultGameDataAsset;
        [SerializeField] private float m_IconToggleTime;
        [SerializeField] private UnityEvent<GameData> m_BeforeSerializeGameData;
        [SerializeField] private UnityEvent<GameData> m_AfterDeserializeGameData;

        public UserManager userManager { get; private set; }
        public DataHandler dataHandler { get; private set; }
        public GameData gameData { get; private set; }
        public GlobalGameData globalGameData { get; private set; }

        public GameDataAsset defaultGameDataAsset => m_DefaultGameDataAsset;
        public UnityEvent<GameData> beforeSerializeGameData => m_BeforeSerializeGameData;
        public UnityEvent<GameData> afterDeserializeGameData => m_AfterDeserializeGameData;

        private ToggleIconFlag _iconFlags;
        private Tweener _toggleIconTween;

        public void Initialize() {
            dataHandler = PlatformManager.currentPlatform == PlatformManager.Platform.WebGL ? new WebDataHandler() : new FileDataHandler();
            GameLogger.data.Log($"Data Handler is {dataHandler.GetType().FullName}", LogLevel.Verbose);

            userManager = new UserManager(dataHandler, GenerateNewGameData);
            userManager.GameDataChanged += UserManager_GameDataChanged;

            globalGameData = dataHandler.DeserializeGlobal();
        }

        private void Save(string state, int slot, bool autoFlag) {
            GameLogger.data.Log($"Serialize data with state {state} on slot {slot}. AutoSave: {autoFlag}", LogLevel.Verbose);

            ToggleIcon(true, ToggleIconFlag.Save);

            if (gameData != null)
                m_BeforeSerializeGameData?.Invoke(gameData);

            if (SceneStateController.instance && SceneStateController.instance.stateData) {
                float pos = 0.0f;
                if (GameCharactersManager.instance && GameCharactersManager.instance.bastheet)
                    pos = GameCharactersManager.instance.bastheet.transform.position.x;
                gameData.state = new SerializationState(SceneManager.GetActiveScene().path, pos, state);
            }

            gameData.lastSerialization = DateTime.Now.ToBinary();
            gameData.userId = slot;
            userManager.UpdateId(slot);
            dataHandler.Serialize(gameData, slot);

            ToggleIcon(false, ToggleIconFlag.Save);
        }

        public void Save(string state) {
            int slot = GetAutoNextSaveSlot();
            Save(state, slot, true);
        }

        public void Save(int slot) {
            Save(gameData.state?.id, slot, false);
        }

        public void Save() {
            Save(gameData.state?.id);
        }

        public void ClearSave() {
            Save(null);
        }

        public void SaveCheckpoint(string state) {
            if (gameData.state.id != state)
                Save(state);
        }

        public void SaveGlobal() {
            GameLogger.data.Log("Serialize global game data", LogLevel.Verbose);
            ToggleIcon(true, ToggleIconFlag.Global);
            dataHandler.SerializeGlobal(globalGameData);
            ToggleIcon(false, ToggleIconFlag.Global);
        }

        public GameData GetLatestData(GameData[] list) => FilterData(list, (x, y) => y.CompareTo(x));

        public GameData GetOldestData(GameData[] list) => FilterData(list, (x, y) => x.CompareTo(y));

        public GameData FilterData(GameData[] list, Comparison<DateTime> comparer) {
            GameData lastData = null;
            DateTime lastDataTime = DateTime.MinValue;

            for (int i = 0; i < list.Length; i++) {
                var data = list[i];
                var dataTime = data.serializationDate;
                if (lastData == null || comparer.Invoke(lastDataTime, dataTime) == 1) {
                    lastData = data;
                    lastDataTime = dataTime;
                }
            }

            return lastData;
        }

        public GameData[] GetAllGameData(bool? autoSaveOnly = null) {
            List<GameData> list = new List<GameData>();
            int start = autoSaveOnly == true ? UserManager.UserSavesCount : 0;
            int end = autoSaveOnly == false ? UserManager.UserSavesCount - 1 : UserManager.MaxUserId;

            for (int i = start; i <= end; i++) {
                if (dataHandler.HaveUser(i))
                    list.Add(dataHandler.Deserialize(i));
            }
            return list.ToArray();
        }

        public int GetAutoNextSaveSlot() {
            GameData[] allData = GetAllGameData(true);
            if (allData.Length == UserManager.AutoSavesCount) {
                return GetOldestData(allData).userId;
            } else {
                int dataIdx = 0;
                for (int i = UserManager.UserSavesCount; i <= UserManager.MaxUserId; i++) {
                    var data = dataIdx < allData.Length ? allData[dataIdx] : null;
                    if (data != null && data.userId == i) {
                        dataIdx++;
                        continue;
                    } else {
                        return i;
                    }
                }
            }

            GameLogger.data.LogError("Cannot find a oldest save or a empty slot");
            return 0;
        }

        public void InjectData(Func<GameData, bool> inject) {
            ToggleIcon(true, ToggleIconFlag.Injection);
            inject(gameData);
            var data = dataHandler.Deserialize(gameData.userId);
            if (inject(data)) dataHandler.Serialize(data, gameData.userId);
            ToggleIcon(false, ToggleIconFlag.Injection);
        }

        private void ToggleIcon(bool enabled, ToggleIconFlag flag) {
            if (enabled)
                _iconFlags |= flag;
            else
                _iconFlags &= ~flag;

            if (_iconFlags != ToggleIconFlag.None) {
                _toggleIconTween.Kill();
                _toggleIconTween = SceneLoader.instance.saveIcon.ToggleGroupAnimated(true, m_IconToggleTime);
            } else {
                if (_toggleIconTween.IsComplete())
                    _toggleIconTween = SceneLoader.instance.saveIcon.ToggleGroupAnimated(false, m_IconToggleTime);
                else
                    _toggleIconTween.OnComplete(() => SceneLoader.instance.saveIcon.ToggleGroupAnimated(false, m_IconToggleTime));
            }
        }

        private GameData GenerateNewGameData(int userID) {
            return defaultGameDataAsset.GenerateNew(userID);
        }

        private void UserManager_GameDataChanged(GameData gameData) {
            this.gameData = gameData;
            m_AfterDeserializeGameData?.Invoke(gameData);
        }
    }
}