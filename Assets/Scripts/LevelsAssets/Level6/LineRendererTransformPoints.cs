using UnityEngine;

namespace NFHGame.LevelAssets.Level6 {
    public class LineRendererTransformPoints : MonoBehaviour {
        [SerializeField] private Transform[] m_Points;

        private LineRenderer _lineRenderer;

        private void Awake() {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = m_Points.Length;
        }

        private void LateUpdate() {
            for (int i = 0; i < m_Points.Length; i++) {
                _lineRenderer.SetPosition(i, m_Points[i].position);
            }
        }
    }
}
