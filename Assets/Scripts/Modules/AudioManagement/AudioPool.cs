using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace NFHGame.AudioManagement {
    public class AudioPool : Singleton<AudioPool> {
        [SerializeField] private AudioSource m_AudioSourcePrefab;

        private ObjectPool<PooledAudioHandler> _pool;
        private HashSet<PooledAudioHandler> _activeSources;
        private HashSet<PooledAudioHandler> _disposedSources;

        private bool _checkCollection = true;

        protected override void Awake() {
            base.Awake();
            _activeSources = new HashSet<PooledAudioHandler>();
            _disposedSources = new HashSet<PooledAudioHandler>();
            _pool = new ObjectPool<PooledAudioHandler>(CreateSource, GetSource, ReleaseSource, DestroySource);
            Application.focusChanged += EVENT_FocusChanged;
        }

        protected override void OnDestroy() {
            Application.focusChanged -= EVENT_FocusChanged;
            base.OnDestroy();
        }

        private void Update() {
            if (_activeSources.Count == 0 || !_checkCollection) return;
            foreach (PooledAudioHandler handler in _activeSources)
                if (!handler.source.isPlaying) _disposedSources.Add(handler);

            if (_disposedSources.Count == 0) return;
            foreach (PooledAudioHandler handler in _disposedSources) 
                _pool.Release(handler);
            _disposedSources.Clear();
        }
        
        private void EVENT_FocusChanged(bool hasFocus) {
#if !UNITY_EDITOR
            _checkCollection = hasFocus;
#endif
        }

        private void OnApplicationPause(bool isPaused) {
            _checkCollection = !isPaused;
        }

        public PooledAudioHandler PlaySound(AudioProviderObject audioObject) => PlaySoundAt(audioObject, Vector3.zero);

        public PooledAudioHandler PlaySoundAt(AudioProviderObject audioObject, Vector3 position) {
            var handler = _pool.Get();
            handler.Set(audioObject, position);
            handler.source.Play();
            return handler;
        }

        public PooledAudioHandler PlayResourcedAudio(string resourceLocation) {
            var handler = PlaySound(Resources.Load<AudioProviderObject>(resourceLocation));
            handler.onRelease.AddListener(() => Resources.UnloadAsset(handler.provider));
            return handler;
        }

        private PooledAudioHandler CreateSource() {
            var handler = new PooledAudioHandler(Instantiate(m_AudioSourcePrefab, transform));
            handler.source.name = typeof(PooledAudioHandler).Name;
            return handler;
        }

        private void GetSource(PooledAudioHandler handler) {
            _activeSources.Add(handler);
            handler.source.gameObject.SetActive(true);
        }

        private void ReleaseSource(PooledAudioHandler handler) {
            _activeSources.Remove(handler);
            handler.onRelease.Invoke();
            handler.Clear();
            handler.source.gameObject.SetActive(false);
            handler.source.transform.SetParent(transform);
            handler.source.transform.position = Vector3.zero;
        }

        private void DestroySource(PooledAudioHandler handler) {
            _activeSources.Remove(handler);
            Destroy(handler.source.gameObject);
        }
    }

    public class PooledAudioHandler {
        public readonly AudioSource source;

        public AudioProviderObject provider;
        public UnityEvent onRelease;

        public PooledAudioHandler(AudioSource source) {
            this.source = source;
            onRelease = new UnityEvent();
        }

        public void Clear() {
            provider = null;
            onRelease.RemoveAllListeners();
        }

        public void Set(AudioProviderObject provider, Vector3 position) {
            this.provider = provider;
            source.transform.position = position;
            provider.CloneToSource(source);
        }
    }
}
