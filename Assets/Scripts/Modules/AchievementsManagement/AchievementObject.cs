using UnityEngine;

namespace NFHGame.AchievementsManagement {
    [CreateAssetMenu(menuName = "Scriptable/Achievements/Achievement Object")]
    public class AchievementObject : ScriptableObject {
        public string achievementName;
        public string achievementGameKey;
        public bool secret;
        public bool inject;
        [TextArea] public string achievementDescription;

        public string GetName(bool have) => secret && !have ? "?????" : achievementName;
        public string GetNotification() => $"<color=#000>{achievementName}</color>\n{achievementDescription}";
    }
}