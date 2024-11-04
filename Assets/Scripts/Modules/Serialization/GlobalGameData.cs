using System.Collections.Generic;

namespace NFHGame.Serialization {
    [System.Serializable]
    public class GlobalGameData {
        public List<string> foundAchievements = new List<string>();
        public List<string> foundItems = new List<string>();
    }
}