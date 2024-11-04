using NFHGame.Characters;
using System.Collections;
using UnityEngine;

namespace NFHGame.Battle {
    public class PoundTrigger : MonoBehaviour {
        [SerializeField] private float m_BastheetGameOverTime;
        [SerializeField] private float m_DinnerGameOverTime;

        [SerializeField, TextArea] private string m_BastheetGameOverText;
        [SerializeField, TextArea] private string m_DinnerGameOverText;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.TryGetComponent<BastheetCharacterController>(out var bastheet)) {
                bastheet.stateMachine.animState.Animate(BastheetCharacterController.DrowedAnimationHash);
                MakeTrigger(bastheet.rb);
                if (!BattleManager.instance.gameOver) {
                    BattleManager.instance.gameOver = true;
                    StartCoroutine(BastheetGameOver());
                }
            } else if (other.TryGetComponent<DinnerCharacterController>(out var dinner)) {
                dinner.stateMachine.animState.Animate(DinnerCharacterController.DrownedAnimationHash);
                MakeTrigger(dinner.rb);
                if (!BattleManager.instance.gameOver) {
                    BattleManager.instance.gameOver = true;
                    StartCoroutine(DinnerGameOver());
                }
            }
        }

        private void MakeTrigger(Rigidbody2D rb) {
            var colliders = new Collider2D[rb.attachedColliderCount];
            rb.GetAttachedColliders(colliders);
            foreach (var collider in colliders) {
                collider.enabled = false;
            }
            rb.gravityScale = 0.0f;
            rb.velocity = Vector2.zero;
        }

        private IEnumerator BastheetGameOver() {
            yield return Helpers.GetWaitForSeconds(m_BastheetGameOverTime);
            BattleManager.instance.GameOver(m_BastheetGameOverText);
        }

        private IEnumerator DinnerGameOver() {
            yield return Helpers.GetWaitForSeconds(m_DinnerGameOverTime);
            BattleManager.instance.GameOver(m_DinnerGameOverText);
        }
    }
}
