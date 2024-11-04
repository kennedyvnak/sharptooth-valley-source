using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class BigRock : MonoBehaviour {
        [SerializeField] private LayerMask m_GroundLayer;
        [SerializeField] private AudioSource m_Source;

        private void OnCollisionEnter2D(Collision2D other) {
            var layer = 1 << other.gameObject.layer;

            if ((layer & m_GroundLayer.value) != 0) {
                m_Source.Play();
            }
        }
    }
}
