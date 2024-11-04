using UnityEngine;

namespace NFHGame.RangedValues {
    [System.Serializable]
    public struct RangedInt {
        public int min;
        public int max;

        public int RandomRange() => UnityEngine.Random.Range(min, max + 1);

        public RangedInt(int min, int max) {
            this.min = min;
            this.max = max;
        }
        
        public float Lerp(float x) => Mathf.Lerp(min, max, x);

        public bool Contains(int time) => time >= min && time <= max;
        public bool Contains(float time) => time >= min && time <= max;
    }
}