using DG.Tweening;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedSecretEntranceMinigame : GameTriggerBase {
        [SerializeField] private CanvasGroup m_ProgressGroup;
        [SerializeField] private Image[] m_ProgressImages;
        [SerializeField] private Sprite[] m_Icons;

        private int _currentImage;
        private GameTriggerProcessor.GameTriggerHandler _handler;

        public override bool Match(string id) {
            return id switch {
                "startTunnelMiniGame" => true,
                "stepTunnelMiniGame" => true,
                "loseTunnelMiniGame" => true,
                "stopTunnelMiniGame" => true,
                _ => false
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _handler = handler;
            switch (id) {
                case "startTunnelMiniGame":
                    StartTunnelMiniGame();
                    break;
                case "stepTunnelMiniGame":
                    StepTunnelMiniGame();
                    break;
                case "loseTunnelMiniGame":
                    LoseTunnelMiniGame();
                    break;
                case "stopTunnelMiniGame":
                    StopTunnelMiniGame();
                    break;
            }
            return true;
        }

        private void StartTunnelMiniGame() {
            m_ProgressGroup.gameObject.SetActive(true);
            m_ProgressGroup.ToggleGroupAnimated(true, 1.0f);
            DataManager.instance.Save();
            _handler.onReturnToDialogue.Invoke();
        }

        private void StepTunnelMiniGame() {
            m_ProgressImages[_currentImage].sprite = m_Icons[1];
            _currentImage++;
            _handler.onReturnToDialogue.Invoke();

            if (_currentImage == m_ProgressImages.Length) {
                m_ProgressGroup.ToggleGroupAnimated(false, 1.0f).SetDelay(5.0f).onComplete += () => {
                    m_ProgressGroup.gameObject.SetActive(false);
                };
            }
        }

        private void LoseTunnelMiniGame() {
            m_ProgressImages[_currentImage].sprite = m_Icons[2];
            _currentImage++;
            _handler.onReturnToDialogue.Invoke();
        }

        private void StopTunnelMiniGame() {
            m_ProgressGroup.ToggleGroupAnimated(false, 1.0f).onComplete += () => {
                m_ProgressGroup.gameObject.SetActive(false);
                foreach (var image in m_ProgressImages)
                    image.sprite = m_Icons[0];
                _currentImage = 0;
            };
            _handler.onReturnToDialogue.Invoke();
        }
    }
}
