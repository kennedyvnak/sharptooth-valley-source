using UnityEngine;

namespace NFHGame.AudioManagement {
    [CreateAssetMenu(fileName = "New Audio Object", menuName = "Scriptable/Audio/Audio Object")]
    public class AudioObject : AudioProviderObject {
        public AudioClip clip;

        public override void CloneToSource(AudioSource source) {
            if (!source)
                throw new System.ArgumentNullException(nameof(source));
            source.clip = clip;
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