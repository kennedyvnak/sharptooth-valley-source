using System.IO;
using NFHGame;
using NFHGame.Serialization;
using NFHGame.Serialization.Handlers;
using UnityEditor;
using UnityEngine;

namespace NFHGameEditor.Serialization {
    public class DataEditorReaderWindow : EditorWindow {
        private string _path;
        private string _fileText;
        private string _json;
        private string _jsonFormatted;

        private Vector2 _scrollPosition;
        private static GUIStyle s_Style;

        [MenuItem("Tools/Data Editor Reader Window")]
        private static void ShowWindow() {
            string path = EditorUtility.OpenFilePanel("Load Save File", Application.persistentDataPath, "data;*.save;");
            string fileText = null;
            string jsonFormatted = null;
            string json = null;

            if (File.Exists(path)) {
                using FileStream stream = new FileStream(path, FileMode.Open);
                using StreamReader reader = new StreamReader(stream);
                fileText = DataHandler.EncryptDecrypt(reader.ReadToEnd());
                if (Helpers.StringHelpers.EndsWith(path, ".data")) {
                    var data = JsonUtility.FromJson<GlobalGameData>(fileText);
                    jsonFormatted = JsonUtility.ToJson(data, true);
                    json = JsonUtility.ToJson(data, false);
                } else if (Helpers.StringHelpers.EndsWith(path, ".save")) {
                    var data = JsonUtility.FromJson<GameData>(fileText);
                    jsonFormatted = JsonUtility.ToJson(data, true);
                    json = JsonUtility.ToJson(data, false);
                }
            } else {
                return;
            }

            var window = GetWindow<DataEditorReaderWindow>();
            window.titleContent = new GUIContent("Data Editor Reader Window");
            window.minSize = new Vector2(400.0f, 300.0f);
            window._path = path;
            window._fileText = fileText;
            window._jsonFormatted = jsonFormatted;
            window._json = json;
            window.Show();
        }

        private void OnGUI() {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = position.width - 200.0f;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            Label("Path", _path);
            Label("Json Formatted", _jsonFormatted);
            Label("Text", _fileText);
            Label("Json", _json);

            EditorGUILayout.EndScrollView();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void Label(string header, string content) {
            EditorGUILayout.Space(8.0f);
            EditorGUILayout.SelectableLabel(header, EditorStyles.boldLabel);

            Vector2 size = GetStyle().CalcSize(EditorGUIUtility.TrTempContent(content));
            Rect viewRect = EditorGUILayout.GetControlRect(hasLabel: false, size.y, GetStyle(), null);
            EditorGUI.SelectableLabel(viewRect, content, GetStyle());
        }

        private GUIStyle GetStyle() {
            if (s_Style != null) return s_Style;

            s_Style = new GUIStyle(EditorStyles.whiteLabel);
            s_Style.wordWrap = true;
            return s_Style;
        }
    }
}