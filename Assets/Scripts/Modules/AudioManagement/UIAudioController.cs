using UnityEngine;

namespace NFHGame.AudioManagement {
    public class UIAudioController : SingletonPersistent<UIAudioController> {
        [SerializeField] private AudioSource m_ClickedSource;
        [SerializeField] private AudioSource m_SelectedSource;

        private void Start() {
            UIAudioManager.instance.onPlaySelected.AddListener(PlaySelected);
            UIAudioManager.instance.onPlayClicked.AddListener(PlayClicked);
        }

        private void PlayClicked() {
            m_ClickedSource.Play();
        }

        private void PlaySelected() {
            m_SelectedSource.Play();
        }
    }
}