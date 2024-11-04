using System.Collections;
using NFHGame.AchievementsManagement;
using NFHGame.AudioManagement;
using NFHGame.Serialization;
using NFHGame.UI;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class SacrificeGameOver : GameTrigger {
        [SerializeField] private float m_FadeTime;
        [SerializeField, TextArea] private string m_GameOverText;
        [SerializeField] private AchievementObject m_Achievement;
        [SerializeField] private AudioProviderObject m_SacrificeSound;
        [SerializeField] private float m_BlackScreenDelay;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            SoundtrackManager.instance.StopSoundtrack();
            var fadeHandler = FadeScreen.instance.FadeFor(m_FadeTime);
            fadeHandler.onFinishFadeIn += () => StartCoroutine(GameOver());
            return true;
        }

        private IEnumerator GameOver() {
            AudioPool.instance.PlaySound(m_SacrificeSound);
            
            yield return Helpers.GetWaitForSeconds(m_BlackScreenDelay);

            DialogueManager.instance.executionEngine.FinishFlow();
            AchievementsManager.instance.UnlockAchievement(m_Achievement);
            DataManager.instance.Save();
            GameManager.instance.GameOver(m_GameOverText);
        }
    }
}
