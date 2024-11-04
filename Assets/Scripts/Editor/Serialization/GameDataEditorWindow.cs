using NFHGame.Serialization;
using NFHGame.Serialization.Handlers;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NFHGameEditor.Serialization {
    public class GameDataEditorWindow : EditorWindow {
        private const string k_FileExtension = "." + DataHandler.FileExtension;

        [SerializeField] private GameDataAsset m_Asset;
        [SerializeField] private GameData m_EditingData;

        private SerializedObject so { get; set; }
        private SerializedProperty editingDataProp { get; set; }

        private Vector2 _scrollPosition;

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line) {
            string path = AssetDatabase.GetAssetPath(instanceId);
            if (!path.EndsWith(k_FileExtension, StringComparison.InvariantCultureIgnoreCase))
                return false;

            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is not GameDataAsset asset)
                return false;
            OpenEditor(asset);
            return true;
        }

        public static void OpenEditor(GameDataAsset asset) {
            GameDataEditorWindow window = GetWindow<GameDataEditorWindow>();
            window.titleContent = new GUIContent(asset.name + " (Game Data)");
            window.minSize = new Vector2(450, 600);
            window.m_Asset = asset;
            window.m_EditingData = window.m_Asset.gameData.Clone();
            window.Show();
        }

        private void OnEnable() {
            so = new SerializedObject(this);
            editingDataProp = so.FindProperty(nameof(m_EditingData));
        }

        private void OnDisable() {
            so.Dispose();
        }

        private void OnGUI() {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            so.Update();
            DrawToolbar();

            SerializedProperty iterator = editingDataProp.Copy();
            iterator.Next(true);
            EditorGUILayout.PropertyField(iterator);
            while (iterator.Next(false))
                EditorGUILayout.PropertyField(iterator);

            if (so.ApplyModifiedPropertiesWithoutUndo())
                hasUnsavedChanges = true;
            
            GUILayout.EndScrollView();
        }

        private void DrawToolbar() {
            GUILayout.BeginHorizontal("toolbar");
            using (new EditorGUI.DisabledScope(!hasUnsavedChanges)) {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                    SaveChanges();
            }
            GUILayout.EndHorizontal();
        }

        public override void SaveChanges() {
            base.SaveChanges();
            string path = AssetDatabase.GetAssetPath(m_Asset);
            string json = JsonUtility.ToJson(m_EditingData);
            File.WriteAllText(path, DataHandler.EncryptDecrypt(json));
            m_Asset.LoadFromJson(json);
        }

        public override void DiscardChanges() {
            base.DiscardChanges();
        }
    }
}