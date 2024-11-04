using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionPositions : InteractionBehaviour {
        [SerializeField] private float m_BastheetPosition;
        [SerializeField] private float m_DinnerPosition;
        [SerializeField] private float m_SpammyPosition;

        [SerializeField] private bool m_DontControlBastheet;
        [SerializeField] private bool m_DontControlDinner;
        [SerializeField] private bool m_DontControlSpammy;

        [SerializeField] private bool m_AutoFaceDirection = true;
        [SerializeField] private bool m_BastheetRight;
        [SerializeField] private bool m_DinnerRight;
        [SerializeField] private bool m_SpammyRight;

        public float bastPos => m_BastheetPosition + transform.position.x;
        public float dinnerPos => m_DinnerPosition + transform.position.x;
        public float spamPos => m_SpammyPosition + transform.position.x;

        public bool ingoreBast => m_DontControlBastheet;
        public bool ignoreDinner => m_DontControlDinner;
        public bool ignoreSpammy => m_DontControlSpammy;

        public bool setFaceDir => m_AutoFaceDirection;
        public bool bastDir => m_BastheetRight;
        public bool dinnerDir => m_DinnerRight;
        public bool spamDir => m_SpammyRight;

        protected override void BindEvents() {
        }

        protected override void UnbindEvents() {
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            const float sphereRadius = 0.25f;
            var bast = FindObjectOfType<Characters.BastheetCharacterController>();
            float y = bast ? bast.transform.position.y : 0.0f;

            Gizmos.color = Color.gray;
            if (!ingoreBast)
                Gizmos.DrawSphere(new Vector2(bastPos, y), sphereRadius);

            Gizmos.color = Color.green;
            if (!ignoreDinner)
                Gizmos.DrawSphere(new Vector2(dinnerPos, y), sphereRadius);

            Gizmos.color = Color.yellow;
            if (!ignoreSpammy)
                Gizmos.DrawSphere(new Vector2(spamPos, y), sphereRadius);
        }
#endif
    }
}