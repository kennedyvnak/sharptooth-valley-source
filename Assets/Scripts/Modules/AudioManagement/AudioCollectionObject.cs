using UnityEngine;

namespace NFHGame.AudioManagement {
    [CreateAssetMenu(fileName = "New Audio Collection Object", menuName = "Scriptable/Audio/Audio Collection Object")]
    public class AudioCollectionObject : AudioProviderObject {
        public AudioClip[] clips;

        public override void CloneToSource(AudioSource source) {
            if (!source)
                throw new System.ArgumentNullException(nameof(source));
            if (clips == null || clips.Length == 0)
                throw new System.Exception("Clips length cannot be 0.");

            source.clip = clips[Random.Range(0, clips.Length)];
            source.outputAudioMixerGroup = AudioManager.instance.GetAudioMixerGroup(group);
            source.volume = volume.RandomRange();
            source.pitch = pitch.RandomRange();
            source.priority = priority;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
        }
    }
}