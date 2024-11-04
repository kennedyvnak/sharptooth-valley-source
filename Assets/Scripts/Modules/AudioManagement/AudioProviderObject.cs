using NFHGame.RangedValues;
using UnityEngine;

namespace NFHGame.AudioManagement {
    public abstract class AudioProviderObject : ScriptableObject {
        public AudioGroupType group = AudioGroupType.Sounds;

        [Space]
        [RangedValue(0, 1)] public RangedFloat volume = new RangedFloat(.75f, .9f);
        [RangedValue(0, 2)] public RangedFloat pitch = new RangedFloat(.9f, 1.1f);

        [Space]
        [Range(0, 256)] public int priority = 128;
        public bool loop;

        [Space]
        [Range(0, 1)] public float spatialBlend = 1;

        [Space]
        public float minDistance = 1;
        public float maxDistance = 500;

        public abstract void CloneToSource(AudioSource source);

#if UNITY_EDITOR
        private void OnValidate() {
            maxDistance = Mathf.Clamp(maxDistance, 0, float.MaxValue);
            minDistance = Mathf.Clamp(minDistance, 0, maxDistance);

            volume.min = Mathf.Clamp(volume.min, 0, volume.max);
            volume.max = Mathf.Clamp(volume.max, volume.min, 1);
        }
#endif
    }
}