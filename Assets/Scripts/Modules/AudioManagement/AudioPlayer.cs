using UnityEngine;

namespace NFHGame.AudioManagement {
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour {
        [SerializeField] private bool m_PlayOnAwake;
        [SerializeField] private AudioProviderObject m_AudioObject;

        public bool playOnAwake { get => m_PlayOnAwake; set => m_PlayOnAwake = value; }
        public AudioProviderObject audioObject { get => m_AudioObject; set => m_AudioObject = value; }
        public AudioSource source { get; private set; }

        private void Awake() {
            source = GetComponent<AudioSource>();

            m_AudioObject.CloneToSource(source);
            if (m_PlayOnAwake)
                source.Play();
        }

        public void Play() {
            m_AudioObject.CloneToSource(source);
            source.Play();
        }
    }
}
