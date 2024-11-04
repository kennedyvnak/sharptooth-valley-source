using NFHGame.Animations;
using System.Reflection;
using UnityEditor;

namespace NFHGameEditor {
    [CustomEditor(typeof(ValueArrayAnimator<>), true), CanEditMultipleObjects]
    public class ValueArrayAnimatorEditor : Editor {
        private MethodInfo _method;

        private void OnEnable() {
            _method = target.GetType().BaseType.GetMethod("UpdateFrameDuration", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            _method.Invoke(target, null);
        }
    }
}
