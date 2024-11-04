using NFHGame.RangedValues;
using UnityEngine;

namespace NFHGame.Interaction.Input {
    public class InputClickController {
        public struct RaycastResult {
            public InputClickProcessResult flags;
            public Vector2 clickWorldPosition;
            public RaycastHit2D groundRaycast;
            public int collisionCount;
        }

        private LayerMask _ignoreClicksLayer;
        private RangedFloat _raycastRange;
        private RangedFloat _raycastHeightRange;
        private LayerMask _groundLayer;
        
        private readonly RaycastHit2D[] _raycastResult = new RaycastHit2D[16];
        public RaycastHit2D[] raycastResult => _raycastResult;

        public InputClickController(LayerMask ignoreClicksLayer, RangedFloat raycastRange, RangedFloat raycastHeightRange, LayerMask groundLayer) {
            _ignoreClicksLayer = ignoreClicksLayer;
            _raycastRange = raycastRange;
            _raycastHeightRange = raycastHeightRange;
            _groundLayer = groundLayer;
        }

        public RaycastResult Raycast(InteractorPoint point, Vector2 screenPosition) {
            RaycastResult result = new RaycastResult {
                clickWorldPosition = point.position
            };

            if (UI.ScreenCanvas.Raycast(point.screenPosition)) {
                result.flags |= InputClickProcessResult.ShouldIgnore;
                return result;
            }

            float rayRange = Mathf.Abs(_raycastRange.min) + Mathf.Abs(_raycastRange.max);
            result.collisionCount = Physics2D.RaycastNonAlloc(new Vector3(result.clickWorldPosition.x, result.clickWorldPosition.y, _raycastRange.min), Vector3.forward, _raycastResult, rayRange, _ignoreClicksLayer);
            for (int i = 0; i < result.collisionCount; i++) {
                var collision = raycastResult[i];
                if (collision.collider.TryGetComponent<InteractionObjectPointCollider>(out var pointCollider)) {
                    pointCollider.PointClick(point);
                    result.flags |= InputClickProcessResult.Interacted;
                    break;
                }
                result.flags |= InputClickProcessResult.ShouldIgnore;
            }

            if (result.flags == InputClickProcessResult.None) {
                result.groundRaycast = Physics2D.Raycast(new Vector2(result.clickWorldPosition.x, _raycastHeightRange.max), new Vector2(0, -1),
                Mathf.Abs(_raycastHeightRange.min) + Mathf.Abs(_raycastHeightRange.max), _groundLayer);
            }

            return result;
        }
    }
}