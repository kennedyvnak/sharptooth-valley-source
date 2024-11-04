using NFHGame.Characters;
using UnityEngine;

namespace NFHGame.SceneManagement {
    public class SceneLoadAnchorFixedPosition : SceneLoadAnchor {
        [System.Serializable]
        public struct CharacterData {
            public float positionX;
            public bool facingRight;
        }

        [SerializeField] private CharacterData m_Bastheet, m_Dinner, m_Spammy;

        private void Awake() {
            onLoad.AddListener(OnLoad);
        }

        private void OnLoad(SceneLoader.SceneLoadingHandler handler) {
            if (!GameCharactersManager.instance) return;

            GameCharactersManager.instance.bastheet.SetPositionX(m_Bastheet.positionX, m_Bastheet.facingRight);
            GameCharactersManager.instance.dinner.SetPositionX(m_Dinner.positionX, m_Dinner.facingRight);
            GameCharactersManager.instance.spammy.SetPositionX(m_Spammy.positionX, m_Spammy.facingRight);
            handler.ResumeInput();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            const float sphereRadius = 0.5f;
            Gizmos.color = new Color(0.70f, 0.60f, 0.10f, 1.0f);
            Gizmos.DrawSphere(new Vector3(m_Bastheet.positionX, 0.0f, 0.0f), sphereRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(m_Dinner.positionX, 0.0f, 0.0f), sphereRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(m_Spammy.positionX, 0.0f, 0.0f), sphereRadius);
        }
#endif
    }
}