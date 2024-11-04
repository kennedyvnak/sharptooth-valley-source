using UnityEngine;

namespace NFHGame.Graphics {
    public class ParallaxUnit : MonoBehaviour {
        private ParallaxObjectData _data;

        private void Start() {
            _data = new ParallaxObjectData(transform.position);
        }

        private void LateUpdate() {
            transform.position = Parallax.GetParallaxPosition(_data, transform.position);
        }
    }
}