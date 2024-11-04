using NFHGame.Characters;
using System;
using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionArea : InteractionBehaviour {
        [SerializeField] private float m_Min;
        [SerializeField] private float m_Max;
        [SerializeField] private bool m_SetFacingDirection;

        public float min => m_Min + transform.position.x;
        public float max => m_Max + transform.position.x;
        public bool setFacingDirection => m_SetFacingDirection;

        protected override void Awake() {
            base.Awake();
        }

        protected override void BindEvents() {
        }

        protected override void UnbindEvents() {
        }

        public bool BastheetIn() {
            var bastheet = GameCharactersManager.instance.bastheet;
            var pos = bastheet.rb.position.x;
            return pos >= min && pos <= max;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            var bast = FindObjectOfType<Characters.BastheetCharacterController>();
            float y = bast ? bast.transform.position.y : 0.0f;
            Gizmos.DrawLine(new Vector2(min, y), new Vector2(max, y));
        }
#endif
    }
}