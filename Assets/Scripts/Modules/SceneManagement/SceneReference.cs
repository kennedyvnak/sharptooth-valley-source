using System;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace NFHGame.SceneManagement {
    /// <summary>
    /// A wrapper that provides the means to safely serialize Scene Asset References.
    /// </summary>
    [Serializable]
    public class SceneReference : ISerializationCallbackReceiver {
#if UNITY_EDITOR
        // What we use in editor to select the scene
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("sceneAsset")] private Object m_SceneAsset;
        private bool isValidSceneAsset {
            get {
                if (!m_SceneAsset) return false;

                return m_SceneAsset is SceneAsset;
            }
        }
#endif

        // This should only ever be set during serialization/deserialization!
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("scenePath")]
        private string m_ScenePath = string.Empty;

        // Use this when you want to actually have the scene path
        public string scenePath {
            get {
                return m_ScenePath;
            }
            set {
                m_ScenePath = value;
#if UNITY_EDITOR
                m_SceneAsset = GetSceneAssetFromPath();
#endif
            }
        }

        public static implicit operator string(SceneReference sceneReference) {
            return sceneReference.scenePath;
        }

        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize() {
#if UNITY_EDITOR
            // We sadly cannot touch assetdatabase during serialization, so defer by a bit.
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }



#if UNITY_EDITOR
        private SceneAsset GetSceneAssetFromPath() {
            return string.IsNullOrEmpty(m_ScenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(m_ScenePath);
        }

        private string GetScenePathFromAsset() {
            return m_SceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(m_SceneAsset);
        }

        private void HandleBeforeSerialize() {
            // Asset is invalid but have Path to try and recover from
            if (isValidSceneAsset == false && string.IsNullOrEmpty(m_ScenePath) == false) {
                m_SceneAsset = GetSceneAssetFromPath();
                if (m_SceneAsset == null) m_ScenePath = string.Empty;

                EditorSceneManager.MarkAllScenesDirty();
            }
            // Asset takes precendence and overwrites Path
            else {
                m_ScenePath = GetScenePathFromAsset();
            }
        }

        private void HandleAfterDeserialize() {
            EditorApplication.update -= HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (isValidSceneAsset) return;

            // Asset is invalid but have path to try and recover from
            if (string.IsNullOrEmpty(m_ScenePath)) return;

            m_SceneAsset = GetSceneAssetFromPath();
            // No asset found, path was invalid. Make sure we don't carry over the old invalid path
            if (!m_SceneAsset) m_ScenePath = string.Empty;

            if (!Application.isPlaying) EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}