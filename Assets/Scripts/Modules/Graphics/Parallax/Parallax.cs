using UnityEngine;

namespace NFHGame.Graphics {
    public readonly struct ParallaxObjectData {
        public readonly float startX, startZ;
        public readonly float absStartZ;
        public readonly bool positiveZ;

        public ParallaxObjectData(Vector3 pos) {
            this.startX = pos.x;
            this.startZ = pos.z;
            this.absStartZ = Mathf.Abs(pos.z);
            this.positiveZ = pos.z > 0.0f;
        }
    }

    public static class Parallax {
        private static float _farClipPlane, _nearClipPlane, _camZ, _camX;

        public static void FlushBuffer(float farClipPlane, float nearClipPlane, float camZ, float camX) {
            _farClipPlane = farClipPlane;
            _nearClipPlane = nearClipPlane;
            _camZ = camZ;
            _camX = camX;
        }

        public static Vector3 GetParallaxPosition(ParallaxObjectData objData, Vector3 position) {
            float parallaxFactor = GetParallaxFactor(objData);

            float distance = _camX * parallaxFactor;

            return new Vector3(objData.startX + distance, position.y, position.z);
        }

        public static float GetParallaxFactor(ParallaxObjectData objData) {
            float clipPlane = objData.positiveZ ? _farClipPlane : _nearClipPlane;
            float clippingPlane = _camZ + clipPlane;
            return objData.absStartZ / clippingPlane;
        }
    }
}