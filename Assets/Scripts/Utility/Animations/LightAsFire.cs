using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame {
    public class LightAsFire : MonoBehaviour {
        [SerializeField] private Light2D m_FireLight;
        [SerializeField] private float m_FireMinIntensity = 0f;
        [SerializeField] private float m_FireMaxIntensity = 1f;
        [SerializeField, Range(1, 50)] private int m_Smoothing = 5;

        private NativeQueue<float> _smoothQueue;
        private Unity.Mathematics.Random _random;
        private float _lastSum = 0;

        public float fireMinIntensity { get => m_FireMinIntensity; set => m_FireMinIntensity = value; }
        public float fireMaxIntensity { get => m_FireMaxIntensity; set => m_FireMaxIntensity = value; }
        public int smoothing { get => m_Smoothing; set => m_Smoothing = value; }

        private void Start() {
            _smoothQueue = new NativeQueue<float>(Allocator.Persistent);
            _random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        }

        private void OnDestroy() {
            if (_smoothQueue.IsCreated)
                _smoothQueue.Dispose();
        }

        private void Update() {
            if (!m_FireLight) return;

            while (_smoothQueue.Count >= m_Smoothing) {
                _lastSum -= _smoothQueue.Dequeue();
            }

            float newVal = _random.NextFloat(m_FireMinIntensity, m_FireMaxIntensity);
            _smoothQueue.Enqueue(newVal);
            _lastSum += newVal;

            m_FireLight.intensity = _lastSum / (float)_smoothQueue.Count;
        }
    }
}
