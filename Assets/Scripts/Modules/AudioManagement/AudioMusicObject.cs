using UnityEngine;
using UnityEngine.Serialization;

namespace NFHGame.AudioManagement {
    [CreateAssetMenu(fileName = "New Audio Collection Object", menuName = "Scriptable/Audio/Audio Music Object")]
    public class AudioMusicObject : AudioProviderObject {
        [FormerlySerializedAs("clip")]
        public AudioClip musicLoop;
        public AudioClip musicStart;

        public override void CloneToSource(AudioSource source) {
            if (!source)
                throw new System.ArgumentNullException(nameof(source));
            if (!musicLoop)
                throw new System.Exception("Music Loop is null.");

            source.clip = musicLoop;
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