using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NFHGameEditor {
    public static class MagicMaker {
        public static readonly float PixelScale = 32.0f;
        public static readonly float HalfPixelScale = 1.0f / (PixelScale * 2.0f);

        [MenuItem("Tools/Pixel Perfect Magic")]
        private static void DoMagic() {
            List<Transform> list = new List<Transform>();

            foreach (var transform in Selection.transforms) {
                GetChildTransformsRecursively(transform, list);
            }

            Undo.RecordObjects(list.ToArray(), "Pixel Perfect Magic");
            foreach (var parent in list) {
                var position = (Vector2)parent.position;
                position *= PixelScale;
                position.x = Mathf.RoundToInt(position.x) / PixelScale;
                position.y = Mathf.RoundToInt(position.y) / PixelScale;

                if (parent.TryGetComponent<SpriteRenderer>(out var renderer)) {
                    var sprite = renderer.sprite;
                    if (renderer.size.x * PixelScale % 2.0f != 0.0f) {
                        position.x += HalfPixelScale;
                    }
                    if (renderer.size.y * PixelScale % 2.0f != 0.0f) {
                        position.y += HalfPixelScale;
                    }
                }

                parent.transform.position = new Vector3(position.x, position.y, parent.transform.position.z);
            }
        }

        private static void GetChildTransformsRecursively(Transform parent, List<Transform> list) {
            if (parent.localScale.x != Mathf.Round(parent.localScale.x) || parent.localScale.y != Mathf.Round(parent.localScale.y))
                Debug.LogWarning("Scale cannot be decimal. Obj: " + parent.name, parent);
            list.Add(parent);

            foreach (Transform child in parent) {
                if (!PrefabUtility.IsPartOfAnyPrefab(parent))
                    GetChildTransformsRecursively(child, list);
            }
        }
    }
}
