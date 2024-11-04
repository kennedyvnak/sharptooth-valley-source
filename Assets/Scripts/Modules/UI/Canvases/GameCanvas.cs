using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.UI {
    [RequireComponent(typeof(Canvas))]
    public class GameCanvas<T> : Singleton<T> where T : GameCanvas<T> {
        private static readonly List<Canvas> s_AllCanvases = new List<Canvas>();
        private static readonly List<RaycastResult> s_RaycastResults = new List<RaycastResult>();

        public Canvas canvas { get; private set; }
        public RectTransform canvasTransform { get; private set; }

        protected override void Awake() {
            base.Awake();
            canvas = GetComponent<Canvas>();
            canvasTransform = canvas.GetComponent<RectTransform>();
            s_AllCanvases.Add(canvas);
            canvas.worldCamera = Helpers.mainCamera;
        }

        protected override void OnDestroy() {
            s_AllCanvases.Remove(canvas);
            base.OnDestroy();
        }

        public static bool Raycast(Vector2 screenPosition) {
            if (s_AllCanvases.Count == 0) return false;

            PointerEventData clickData = new PointerEventData(EventSystem.current) {
                position = screenPosition
            };

            foreach (var canvas in s_AllCanvases) {
                if (canvas.TryGetComponent<GraphicRaycaster>(out var raycaster)) {
                    s_RaycastResults.Clear();
                    raycaster.Raycast(clickData, s_RaycastResults);
                    if (s_RaycastResults.Count > 0)
                        return true;
                }
            }

            return false;
        }
    }
}
