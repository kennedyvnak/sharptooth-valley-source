using NFHGame.Serialization.Handlers;
using System;

namespace NFHGame.Serialization {
    public class UserManager {
        public const int NoUserId = -1;
        public const int UserSavesCount = 10, AutoSavesCount = 20, MaxUserId = UserSavesCount + AutoSavesCount - 1;

        public readonly DataHandler dataHandler;
        public readonly Func<int, GameData> CreateData;

        public event Action<GameData> GameDataChanged;

        public int currentUserId { get; private set; }

        public UserManager(DataHandler dataHandler, Func<int, GameData> createData) {
            this.dataHandler = dataHandler;
            this.CreateData = createData;

            currentUserId = NoUserId;
        }

        public void SetUser(int id) {
            currentUserId = id;
            GameDataChanged(id != NoUserId ? dataHandler.Deserialize(id) ?? CreateData(id) : null);
        }

        public void ResetUser(int userID) {
            dataHandler.DeleteUser(userID);
            currentUserId = userID;
            GameDataChanged.Invoke(CreateData(userID));
        }

        public void ReloadUser() {
            SetUser(currentUserId);
        }

        public void UpdateId(int slot) {
            currentUserId = slot;
        }
    }
}
