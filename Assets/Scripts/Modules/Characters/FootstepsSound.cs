using NFHGame.AudioManagement;
using UnityEngine;

namespace NFHGame.Characters {
    public class FootstepsSound : MonoBehaviour {
        [Header("Sounds")]
        [SerializeField] private AudioSource m_FootstepSource;
        [SerializeField] private AudioObject m_FootstepWalkSound;
        [SerializeField] private AudioObject m_FootstepRunSound;

        private float _lastVelocity;
        private bool _lastRunning;

        public void UpdateSound(float velocity, bool isRunning) {
            if (velocity == 0) {
                if (_lastVelocity != 0)
                    m_FootstepSource.Stop();
            } else if (isRunning) {
                if (!_lastRunning || _lastVelocity == 0) {
                    m_FootstepRunSound.CloneToSource(m_FootstepSource);
                    m_FootstepSource.Play();
                }
            } else {
                if (_lastRunning || _lastVelocity == 0) {
                    m_FootstepWalkSound.CloneToSource(m_FootstepSource);
                    m_FootstepSource.Play();
                }
            }

            _lastVelocity = velocity;
            _lastRunning = isRunning;
        }
    }
}