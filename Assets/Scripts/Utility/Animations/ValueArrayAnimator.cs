using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.Animations {
    public class ValueArrayAnimator<T> : MonoBehaviour {
        [SerializeField] private bool m_UnscaledTime = false;
        [SerializeField] private float m_Duration = 1.0f;
        [SerializeField] private bool m_Loop = true;
        [SerializeField] private float m_LoopDelay = 0.0f;

        [Header("Values")]
        [SerializeField] private T[] m_Values;

        [Header("Events")]
        [SerializeField] private UnityEvent<T> m_ValueChanged;
        [SerializeField] private UnityEvent<int> m_IndexChanged;
        [SerializeField] private UnityEvent m_LoopFinished;

        public float duration { get => m_Duration; set => SetDuration(value); }
        public T[] values { get => m_Values; set => SetValues(value); }
        public UnityEvent<T> valueChanged => m_ValueChanged;

        public UnityEvent<int> indexChanged => m_IndexChanged;
        public UnityEvent loopFinished => m_LoopFinished;

        private int _index = 0;
        private float _timer = 0.0f;
        private float _loopDelayTime = 0.0f;

        private float _frameDuration;

        private void Start() {
            UpdateFrameDuration();
        }

        private void Update() {
            if (_loopDelayTime > 0.0f) {
                _loopDelayTime -= Time.deltaTime;
                return;
            }

            _timer += GetDeltaTime();
            if (_timer >= _frameDuration) {
                _timer = 0.0f;
                
                m_ValueChanged?.Invoke(m_Values[_index]);
                _index = (_index + 1) % m_Values.Length;
                m_IndexChanged?.Invoke(_index);

                if (_index == 0) {
                    _loopDelayTime = m_LoopDelay;
                    if (!m_Loop)
                        enabled = false;
                    m_LoopFinished?.Invoke();
                }
            }
        }

        public void Replay() {
            _index = 0;
            _timer = 0.0f;
            _loopDelayTime = 0.0f;
            m_ValueChanged?.Invoke(m_Values[0]);
            enabled = true;
        }

        public void SetValuesAndSetIndex(T[] values, int startIndex) {
            _index = startIndex;
            _timer = 0.0f;
            _loopDelayTime = 0.0f;
            SetValues(values);
            m_ValueChanged?.Invoke(m_Values[startIndex]);
        }

        public void SetValues(T[] values) {
            m_Values = values;
            UpdateFrameDuration();
        }

        public void SetDuration(float value) {
            m_Duration = value;
            UpdateFrameDuration();
        }

        private float GetDeltaTime() => m_UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private void UpdateFrameDuration() => _frameDuration = m_Duration / m_Values.Length;
    }
}