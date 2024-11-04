using UnityEngine;
namespace NFHGame.Graphics {
    public class LetterBoxer : MonoBehaviour {
        [SerializeField] private float m_X = 16;
        [SerializeField] private float m_Y = 9;
        [SerializeField] private float m_Width = 960;
        [SerializeField] private float m_Height = 540;

        private Camera _cam;
        private int _screenWidth, _screenHeight;
        private float _targetRatio;

        public void Awake() {
            _cam = GetComponent<Camera>();
            _targetRatio = m_X / m_Y;
            
            PerformSizing();
        }

        public void Update() {
            if (Screen.width != _screenWidth || Screen.height != _screenHeight) {
                PerformSizing();
            }
        }

        private void OnValidate() {
            m_X = Mathf.Max(1, m_X);
            m_Y = Mathf.Max(1, m_Y);
            m_Width = Mathf.Max(1, m_Width);
            m_Height = Mathf.Max(1, m_Height);
        }

        private void PerformSizing() {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            float windowaspect = (float)_screenWidth / _screenHeight;
            float scaleheight = windowaspect / _targetRatio;
            float scalewidth = 0.0f;

            bool vert = scaleheight < 1.0f;
            if (!vert) scalewidth = 1.0f / scaleheight;
            _cam.rect = new Rect(
                vert ? 0.0f : (1.0f - scalewidth) * 0.5f,
                vert ? (1.0f - scaleheight) * 0.5f : 0.0f,
                vert ? 1.0f : scalewidth,
                vert ? scaleheight : 1.0f
            );
        }
    }
}