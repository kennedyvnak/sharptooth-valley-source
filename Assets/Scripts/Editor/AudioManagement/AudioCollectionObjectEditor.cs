using NFHGame.AudioManagement;
using UnityEditor;
using UnityEngine;

namespace NFHGameEditor.AudioManagement {
    [CustomEditor(typeof(AudioCollectionObject), false), CanEditMultipleObjects]
    public class AudioCollectionObjectEditor : Editor {
        [SerializeField] private AudioSource _previewer;

        public void OnEnable() {
            _previewer = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        public void OnDisable() {
            DestroyImmediate(_previewer.gameObject);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            AudioCollectionObject audio = (AudioCollectionObject)target;
            if (!audio || audio.clips == null || audio.clips.Length == 0) return;

            for (int i = 0; i < audio.clips.Length; i++) {
                var clip = audio.clips[i];
                if (!clip) {
                    GUILayout.Button($"Null ({i})");
                    return;
                }
                if (GUILayout.Button($"Preview {clip.name} ({i})")) {
                    _previewer.clip = clip;

                    _previewer.volume = audio.volume.RandomRange();
                    _previewer.pitch = audio.pitch.RandomRange();

                    _previewer.priority = audio.priority;
                    _previewer.loop = audio.loop;

                    _previewer.spatialBlend = 0;

                    _previewer.minDistance = audio.minDistance;
                    _previewer.maxDistance = audio.maxDistance;
                    _previewer.Play();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}