using NFHGame.Input;
using UnityEngine;

namespace NFHGame.Interaction {
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class InteractorPoint : MonoBehaviour {
        private Rigidbody2D rb;

        public Interactor interactor { get; private set; }

        public Vector2 position => Helpers.mainCamera.ScreenToWorldPoint(screenPosition);
        public Vector2 screenPosition { get; private set; }

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable() {
            InputReader.instance.OnPointerPosition += INPUT_OnPointerPosition;
        }

        private void OnDisable() {
            InputReader.instance.OnPointerPosition -= INPUT_OnPointerPosition;
        }

        private void Update() {
            rb.position = position;
        }

        private void INPUT_OnPointerPosition(Vector2 screenPosition) {
            this.screenPosition = screenPosition;
        }

        internal void SetInteractor(Interactor interactor) {
            this.interactor = interactor;
        }

        internal void RefreshPosition(Vector2 screenPosition) {
            this.screenPosition = screenPosition;
        }
    }
}
