using System.Collections;
using NFHGame.Characters;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.SceneManagement {
    public class SceneLoadAnchorWalkIn : SceneLoadAnchor {
        [SerializeField] private float m_StartPositionX, m_FinalPositionX;
        [SerializeField] private UnityEvent m_OnFinishWalkIn;

        public float startPositionX { get => m_StartPositionX; set => m_StartPositionX = value; }
        public float finalPositionX { get => m_FinalPositionX; set => m_FinalPositionX = value; }

        public UnityEvent onFinish => m_OnFinishWalkIn;

        private void Awake() {
            onLoad.AddListener(OnLoad);
        }

        private void OnLoad(SceneLoader.SceneLoadingHandler handler) {
            StartCoroutine(OnLoadCoroutine(handler));
        }

        private IEnumerator OnLoadCoroutine(SceneLoader.SceneLoadingHandler handler) {
            var bastheet = GameCharactersManager.instance.bastheet;

            bastheet.SetPositionX(startPositionX);
            GameCharactersManager.instance.dinner.SetPositionX(startPositionX);
            if (GameManager.instance.spammyInParty) GameCharactersManager.instance.spammy.SetPositionX(startPositionX);

            yield return bastheet.WalkToPosition(finalPositionX);

            handler.ResumeInput();
            m_OnFinishWalkIn?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            const float radius = 0.5f;
            Gizmos.DrawSphere(new Vector2(startPositionX, transform.position.y), radius);
            Gizmos.DrawSphere(new Vector2(finalPositionX, transform.position.y), radius);
        }
#endif
    }
}