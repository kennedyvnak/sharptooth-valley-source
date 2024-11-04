using UnityEngine;

namespace NFHGame.RangedValues {
    [System.Serializable]
    public struct RangedFloat {
        public float min;
        public float max;

        public float RandomRange() => UnityEngine.Random.Range(min, max);

        public RangedFloat(float min, float max) {
            this.min = System.Math.Min(min, max);
            this.max = max;
        }

        public float Lerp(float x) => Mathf.Lerp(min, max, x);

        public bool Contains(float t) => t >= min && t <= max;
    }
}