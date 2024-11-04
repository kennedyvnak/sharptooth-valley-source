using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace NFHGameEditor {
    public class EditorInputDialog : EditorWindow {
        string description, inputText;
        bool multiline;
        string okButton, cancelButton;
        bool initializedPosition = false;
        Action onOKButton;

        bool shouldClose = false;
        Vector2 maxScreenPos;

        void OnGUI() {
            var e = Event.current;
            if (e.type == EventType.KeyDown) {
                switch (e.keyCode) {
                    case KeyCode.Escape:
                        shouldClose = true;
                        e.Use();
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        if (multiline)
                            break;
                        onOKButton?.Invoke();
                        shouldClose = true;
                        e.Use();
                        break;
                }
            }

            if (shouldClose) Close();

            var rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(description);

            EditorGUILayout.Space(8);
            GUI.SetNextControlName("inText");
            if (multiline)
                inputText = EditorGUILayout.TextArea(inputText);
            else
                inputText = EditorGUILayout.TextField(string.Empty, inputText);
            GUI.FocusControl("inText");
            EditorGUILayout.Space(12);

            var r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if (GUI.Button(r, okButton)) {
                onOKButton?.Invoke();
                shouldClose = true;
            }

            r.x += r.width;
            if (GUI.Button(r, cancelButton)) {
                inputText = null;
                shouldClose = true;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            if (rect.width != 0 && minSize != rect.size) {
                minSize = maxSize = rect.size;
            }

            if (!initializedPosition && e.type == EventType.Layout) {
                initializedPosition = true;

                var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                mousePos.x += 32;
                if (mousePos.x + position.width > maxScreenPos.x) mousePos.x -= position.width + 64; // Display on left side of mouse
                if (mousePos.y + position.height > maxScreenPos.y) mousePos.y = maxScreenPos.y - position.height;

                position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

                Focus();
            }
        }

        public static string Show(string title, string description, string inputText, bool multiline = false, string okButton = "OK", string cancelButton = "Cancel") {
            var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

            string ret = null;
            var window = CreateInstance<EditorInputDialog>();
            window.maxScreenPos = maxPos;
            window.titleContent = new GUIContent(title);
            window.description = description;
            window.inputText = inputText;
            window.multiline = multiline;
            window.okButton = okButton;
            window.cancelButton = cancelButton;
            window.onOKButton += () => ret = window.inputText;
            window.ShowModal();

            return ret;
        }
    }
}
#endif