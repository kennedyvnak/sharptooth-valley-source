using UnityEngine;

namespace NFHGame.AudioManagement {
    public class ChangeSoundtrackOnStart : MonoBehaviour {
        [SerializeField] private AudioMusicObject m_Soundtrack;
        public AudioMusicObject soundtrack { get => m_Soundtrack; set => m_Soundtrack = value; }

        private void Start() {
            if (!m_Soundtrack)
                SoundtrackManager.instance.StopSoundtrack();
            else
                SoundtrackManager.instance.SetSoundtrack(m_Soundtrack);
        }
    }
}