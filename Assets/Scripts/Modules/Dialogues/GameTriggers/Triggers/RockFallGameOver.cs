using DG.Tweening;
using NFHGame.Characters;
using NFHGame.Input;
using NFHGame.UI;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class RockFallGameOver : GameTrigger {
        [SerializeField] private float m_FadeTime = 2.0f;
        [SerializeField, TextArea] private string m_GameOverText;

        [SerializeField] private GameObject m_RockObject;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            var pos = m_RockObject.transform.position;
            pos.x = GameCharactersManager.instance.bastheet.transform.position.x;
            m_RockObject.transform.position = pos;
            m_RockObject.SetActive(true);

            DOVirtual.DelayedCall(m_FadeTime, () => {
                var fadeHandler = FadeScreen.instance.FadeFor(m_FadeTime);
                fadeHandler.onFinishFadeIn += () => {
                    var tweens = DOTween.TweensById(RockFall.RockFallScreenShakeTweenID);
                    tweens?.ForEach(x => x.Kill());

                    Input.InputReader.instance.PopMap(InputReader.InputMap.Dialogue | InputReader.InputMap.UI);
                    GameManager.instance.GameOver(m_GameOverText);
                };
            });
            return true;
        }
    }
}
