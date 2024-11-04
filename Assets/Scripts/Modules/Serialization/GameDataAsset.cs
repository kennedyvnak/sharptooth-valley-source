using UnityEngine;

namespace NFHGame.Serialization {
    public class GameDataAsset : ScriptableObject {
        [SerializeField] private GameData m_GameData;

        public GameData gameData { get => m_GameData; set => m_GameData = value; }

        public void LoadFromJson(string json) {
            m_GameData = JsonUtility.FromJson<GameData>(json);
        }

        public string ToJson() => JsonUtility.ToJson(m_GameData);

        public GameData GenerateNew(int userId) {
            GameData gameData = this.gameData.Clone();
            gameData.userId = userId;
            gameData.lastSerialization = 0;
            return gameData;
        }
    }
}