using System.Collections;
using UnityEngine;

namespace NFHGame.Graphics {
    public class ParallaxController : MonoBehaviour {
        private void LateUpdate() {
            var mainCam = Helpers.mainCamera;
            var camPos = mainCam.transform.position;
            Parallax.FlushBuffer(mainCam.farClipPlane, mainCam.nearClipPlane, camPos.z, camPos.x);
        }
    }
}