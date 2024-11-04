using NFHGame.AchievementsManagement.UI;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.Configs;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.ScriptableSingletons;
using NFHGame.Serialization;
using System;
using UnityEngine;

namespace NFHGame.AchievementsManagement {
    public class AchievementsManager : ScriptableSingleton<AchievementsManager>, IBootableSingleton {
        [Serializable]
        public class ArticyConditionsAchievement {
            public AchievementObject achievement;
            public string regex;
            public string[] variables;
            public int compare;
        }

        [SerializeField] private AchievementObject[] m_Achievements;
        [SerializeField] private AchievementObject m_AllAchievementsAchievement;
        [SerializeField] private AchievementObject m_AllDinosAchievement;
        [SerializeField] private int m_DinosCount;

        [SerializeField] private ArticyConditionsAchievement[] m_ArticyAchievements;

        public void UnlockAchievement(string achievementKey) {
            var achievementIndex = Array.FindIndex(m_Achievements, (achievement) => achievement.achievementGameKey == achievementKey);
            if (achievementIndex != -1) {
                UnlockAchievement(m_Achievements[achievementIndex]);
            } else {
                GameLogger.achievements.LogError($"Can't find achievement {achievementKey}.");
            }
        }

        public void UnlockAchievement(AchievementObject achievement) {
            var globalData = DataManager.instance.globalGameData;
            var achievementKey = achievement.achievementGameKey;

            if (!globalData.foundAchievements.Contains(achievementKey)) {
                GameLogger.achievements.Log($"Add global achievement {achievementKey}", LogLevel.Verbose);
                globalData.foundAchievements.Add(achievementKey);
                if (globalData.foundAchievements.Count + 1 == m_Achievements.Length)
                    UnlockAchievement(m_AllAchievementsAchievement);
                else
                    DataManager.instance.SaveGlobal();
            }

            if (!GameKeysManager.instance.HaveGameKey(achievementKey))
                NotificationManager.instance.Notify(achievement.GetNotification());

            GameKeysManager.instance.ToggleGameKey(achievementKey, true);
            if (achievement.inject) {
                DataManager.instance.InjectData((data) => {
                    var gameKeys = data.gameKeys;
                    bool add = !gameKeys.Contains(achievementKey);
                    if (add) gameKeys.Add(achievementKey);
                    return add;
                });
            }

            if (AchievementsScreen.instance)
                AchievementsScreen.instance.UpdateAchievement(achievement, true);
        }

        public AchievementObject[] GetAchievements() {
            return m_Achievements;
        }

        void IBootableSingleton.Initialize() {
            ArticyManager.notifications.AddListener("gameState.seenCarving*", UnlockArticyCarvingsAchievement);
            ArticyManager.notifications.AddListener("items.*", UnlockArticyItemsAchievement);
            ArticyManager.notifications.AddListener("gameState.dinosCount", UnlockDinosAchievement);
        }

        private void UnlockArticyItemsAchievement(string v, object value) {
            for (int i = 1; i < m_ArticyAchievements.Length; i++) {
                ArticyConditionsAchievement articyAchievement = m_ArticyAchievements[i];

                Func<string, bool> haveVar = articyAchievement.compare switch {
                    1 => (v) => GetIntOr(v, 1, 2),
                    2 => (v) => GetIntEqual(v, 2),
                    3 => (v) => GetIntEqual(v, 1),
                    _ => (v) => false,
                };

                bool flag = true;
                foreach (var variable in articyAchievement.variables) {
                    if (!haveVar.Invoke(variable)) {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                    UnlockAchievement(articyAchievement.achievement);
            }

            bool GetIntEqual(string variable, int a) => ArticyVariables.globalVariables.GetVariableByString<int>(variable) == a;
            bool GetIntOr(string variable, int a, int b) {
                int v = ArticyVariables.globalVariables.GetVariableByString<int>(variable);
                return v == a || v == b;
            }
        }

        private void UnlockArticyCarvingsAchievement(string v, object value) {
            ArticyConditionsAchievement articyAchievement = m_ArticyAchievements[0];

            foreach (var variable in articyAchievement.variables) {
                if (!ArticyVariables.globalVariables.GetVariableByString<bool>(variable))
                    return;
            }
            UnlockAchievement(articyAchievement.achievement);
        }

        private void UnlockDinosAchievement(string v, object value) {
            int iVal = (int)value;
            if (iVal == m_DinosCount)
                UnlockAchievement(m_AllDinosAchievement);
        }
    }
}