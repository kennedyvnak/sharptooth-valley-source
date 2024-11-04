using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace NFHGame {
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerHandler : MonoBehaviour {
        [SerializeField] private string m_VideoPath;
        [SerializeField] private bool m_PlayOnAwake;
        [SerializeField] private UnityEvent m_VideoFinished;
        [SerializeField] private UnityEvent m_VideoStarted;

        private VideoPlayer _player;

        public UnityEvent videoFinished => m_VideoFinished;
        public UnityEvent videoStarted => m_VideoStarted;

        private void Awake() {
            _player = GetComponent<VideoPlayer>();
            _player.loopPointReached += _ => m_VideoFinished.Invoke();
            _player.started += _ => m_VideoStarted.Invoke();
            if (m_PlayOnAwake)
                PlayVideo();
        }

        public void PlayVideo() {
            if (_player) {
                string path = Path.Combine(Application.streamingAssetsPath, m_VideoPath);
                _player.url = path;
                _player.Play();
            }
        }

        public void PlayVideo(System.Action videoStarted) {
            m_VideoStarted.AddListener(EVENT_VideoStarted);
            PlayVideo();

            void EVENT_VideoStarted() {
                m_VideoStarted.RemoveListener(EVENT_VideoStarted);
                videoStarted?.Invoke();
            }
        }
    }
}
