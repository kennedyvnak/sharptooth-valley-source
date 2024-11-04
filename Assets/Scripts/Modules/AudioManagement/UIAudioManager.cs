using NFHGame.ScriptableSingletons;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.AudioManagement {
    public class UIAudioManager : ScriptableSingleton<UIAudioManager> {
        [SerializeField] private UnityEvent m_OnPlaySelected;
        [SerializeField] private UnityEvent m_OnPlayClicked;

        public UnityEvent onPlaySelected => m_OnPlaySelected;
        public UnityEvent onPlayClicked => m_OnPlayClicked;

        public void PlaySelected() {
            m_OnPlaySelected?.Invoke();
        }

        public void PlayClicked() {
            m_OnPlayClicked?.Invoke();
        }
    }
}