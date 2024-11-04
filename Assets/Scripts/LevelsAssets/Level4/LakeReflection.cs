using UnityEngine;

namespace NFHGame {
    public class LakeReflection : MonoBehaviour {
        [SerializeField] private Transform m_Camera;

        private void LateUpdate() {
            var pos = transform.position;
            pos.x = m_Camera.position.x;
            transform.position = pos;
        }
    }
}
