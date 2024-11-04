using System.Text;

namespace NFHGame.Serialization.Handlers {
    public abstract class DataHandler {
        public const string FileName = "user_{0}";
        public const string FileExtension = "save";

        public const string EncryptionKey = "You won't hack my game data!";

        public abstract GameData Deserialize(int userId);
        public abstract GlobalGameData DeserializeGlobal();

        public abstract void Serialize(GameData data, int slot);
        public abstract void SerializeGlobal(GlobalGameData data);

        public abstract bool HaveUser(int userID);

        public abstract void DeleteUser(int userId);

        public static string EncryptDecrypt(string input) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
                sb.Append((char)(input[i] ^ EncryptionKey[i % EncryptionKey.Length]));
            return sb.ToString();
        }
    }
}
