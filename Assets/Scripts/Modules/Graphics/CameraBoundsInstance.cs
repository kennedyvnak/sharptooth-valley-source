using UnityEngine;

namespace NFHGame.Graphics {
    [RequireComponent(typeof(Collider2D))]
    public class CameraBoundsInstance : Singleton<CameraBoundsInstance> {
        private Collider2D _boundingShape;

        public Collider2D boundingShape => _boundingShape;

        protected override void Awake() {
            base.Awake();
            _boundingShape = GetComponent<Collider2D>();
        }
    }
}
