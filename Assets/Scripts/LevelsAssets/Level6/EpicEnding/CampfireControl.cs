using DG.Tweening;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace NFHGame.LevelAssets.Level6.EpicEnding {
    public class CampfireControl : EpicEndingSceneControl {
        [Header("Fire")]
        [SerializeField] private SpriteRenderer[] m_FakeNormals;
        [SerializeField] private float m_FakeNormalsAlpha;
        [SerializeField] private float m_FireMinIntensity = 0f;
        [SerializeField] private float m_FireMaxIntensity = 1f;
        [SerializeField, Range(1, 50)] private int m_Smoothing = 5;
        [SerializeField] private UnityEvent<float> m_IntensityChanged;

        [Header("Bastheet")]
        [SerializeField] private SpriteRenderer[] m_BastheetRenderers;
        [SerializeField] private Light2D m_BastLight;

        private NativeQueue<float> _smoothQueue;
        private Unity.Mathematics.Random _random;
        private float _lastSum = 0;

        protected override void OnEnable() {
            _smoothQueue = new NativeQueue<float>(Allocator.Persistent);
            _random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            _lastSum = 0;
            base.OnEnable();
        }

        protected override void OnDisable() {
            _smoothQueue.Dispose();
            base.OnDisable();
        }

        private void Update() {
            while (_smoothQueue.Count >= m_Smoothing) {
                _lastSum -= _smoothQueue.Dequeue();
            }

            float newVal = _random.NextFloat(m_FireMinIntensity, m_FireMaxIntensity);
            _smoothQueue.Enqueue(newVal);
            _lastSum += newVal;

            float intensity = _lastSum / (float)_smoothQueue.Count;
            m_IntensityChanged.Invoke(intensity);
            var fakeNormalIntensity = Mathf.InverseLerp(m_FireMinIntensity, m_FireMaxIntensity, intensity) * m_FakeNormalsAlpha;
            foreach (var fakeNormal in m_FakeNormals) {
                var col = fakeNormal.color;
                col.a = fakeNormalIntensity;
                fakeNormal.color = col;
            }
        }

        public void FadeBastheet(float fadeDuration) {
            float intensity = m_BastLight.shadowIntensity;
            m_FakeNormals = new SpriteRenderer[] { m_FakeNormals[1] };
            DOVirtual.Float(1.0f, 0.0f, fadeDuration, (x) => {
                var col = new Color(1.0f, 1.0f, 1.0f, x);
                foreach (SpriteRenderer r in m_BastheetRenderers)
                    r.color = col;
                m_BastLight.shadowIntensity = intensity * x;
            });
        }
    }
}
