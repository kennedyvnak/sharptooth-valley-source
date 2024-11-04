using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionFacingDirection : InteractionBehaviour {
        [SerializeField] private float m_Center;

        public float center => m_Center + transform.position.x;

        protected override void Awake() {
            base.Awake();
        }

        protected override void BindEvents() {
        }

        protected override void UnbindEvents() {
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            var bast = FindObjectOfType<Characters.BastheetCharacterController>();
            float y = bast ? bast.transform.position.y : 0.0f;
            Gizmos.DrawSphere(new Vector3(center, y, 0.0f), 0.2f);
        }
#endif
    }
}