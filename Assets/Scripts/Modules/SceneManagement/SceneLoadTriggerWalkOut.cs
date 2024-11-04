using System.Collections;
using Articy.Unity;
using NFHGame.ArticyImpl;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using UnityEngine;

namespace NFHGame.SceneManagement {
    public class SceneLoadTriggerWalkOut : SceneLoadTrigger {
        [SerializeField] private float m_FinalPositionX;
        [SerializeField] private ArticyRef m_Dialogue;

        public ArticyRef dialogue { get => m_Dialogue; set => m_Dialogue = value; }

        private void Awake() {
            onTrigger?.AddListener(OnTrigger);
        }

        private void OnTrigger() {
            StartCoroutine(OnTriggerCoroutine());
        }

        private IEnumerator OnTriggerCoroutine() {
            if (m_Dialogue.ValidStart()) {
                bool ended = false;
                DialogueManager.instance.PlayHandledDialogue(m_Dialogue).onDialogueFinished += () => ended = true;
                yield return new WaitUntil(() => ended);
            }

            var handler = SceneLoader.instance.CreateHandler(sceneReference, anchorID);
            handler.StopInput();
            var bastheet = GameCharactersManager.instance.bastheet;

            var bastheetCoroutine = StartCoroutine(bastheet.WalkToPosition(m_FinalPositionX));
            var dinnerCoroutine = StartCoroutine(GameCharactersManager.instance.dinner.WalkOut(m_FinalPositionX));

            if (GameManager.instance.spammyInParty) yield return StartCoroutine(GameCharactersManager.instance.spammy.WalkOut(m_FinalPositionX));
            yield return bastheetCoroutine;
            yield return dinnerCoroutine;

            LoadScene(handler);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            const float radius = 0.5f;
            Gizmos.DrawSphere(new Vector2(m_FinalPositionX, transform.position.y), radius);
        }
#endif
    }
}