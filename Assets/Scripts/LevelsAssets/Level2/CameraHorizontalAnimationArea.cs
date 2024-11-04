using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.Cutscenes {
    [RequireComponent(typeof(BoxCollider2D))]
    public class CameraHorizontalAnimationArea : MonoBehaviour {
        [SerializeField] private Transform m_Dummy;
        [SerializeField] private float m_Offset;

        private BoxCollider2D _collider;
        private bool _tracking;
        private float _minX, _maxX;

        private void Awake() {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Start() {
            Vector2 size = _collider.size;
            Vector3 centerPoint = _collider.offset;
            var worldPos = transform.TransformPoint(centerPoint);

            _minX = worldPos.x - (size.x * 0.5f);
            _maxX = worldPos.x + (size.x * 0.5f);
        }

        private void Update() {
            if (_tracking) {
                var charPos = GameCharactersManager.instance.bastheet.transform.position;
                m_Dummy.transform.position = new Vector3(CalculateXPosition(charPos.x), charPos.y, charPos.z);
            }
        }

        private float CalculateXPosition(float playerPosition) {
            var offsetPosition = Mathf.Lerp(_minX, _maxX, m_Offset);

            if (playerPosition <= _maxX && playerPosition >= _minX) {
                var progress = Mathf.InverseLerp(_minX, offsetPosition, playerPosition);
                return Mathf.Lerp(_minX, _maxX, progress);
            } else {
                return playerPosition;
            }
        }

        private void StartTracking() {
            if (!_tracking) {
                _tracking = true;
                m_Dummy.transform.position = GameCharactersManager.instance.bastheet.transform.position;
                Helpers.vCam.Follow = m_Dummy;
            }
        }

        private void EndTracking() {
            if (_tracking) {
                _tracking = false;
                if (Helpers.vCam && GameCharactersManager.instance.bastheet)
                    Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject == GameCharactersManager.instance.bastheet.gameObject) {
                StartTracking();
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject == GameCharactersManager.instance.bastheet.gameObject) {
                EndTracking();
            }
        }
    }
}