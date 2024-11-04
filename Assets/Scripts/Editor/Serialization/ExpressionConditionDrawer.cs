using NFHGame.Serialization;
using UnityEditor;
using UnityEngine;

namespace NFHGameEditor {
    [CustomPropertyDrawer(typeof(Expression))]
    public class ExpressionDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.indentLevel--;
            var conditions = property.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(position, conditions, label);
            EditorGUI.indentLevel++;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"));
    }

    [CustomPropertyDrawer(typeof(Expression.Condition))]
    public class ExpressionConditionDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var text = property.FindPropertyRelative("text");
            var op = property.FindPropertyRelative("op");
            var not = property.FindPropertyRelative("not");

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, text, label);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.width *= 0.5f;
            EditorGUI.PropertyField(position, op, EditorGUIUtility.TrTextContent("Operation"));
            position.x += position.width + 4;
            position.width -= 4;
            EditorGUI.PropertyField(position, not);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
