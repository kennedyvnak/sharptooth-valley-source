using UnityEngine;

namespace NFHGame.Serialization.States {
    [CreateAssetMenu(menuName = "Scriptable/Serialization/Scene States Data")]
    public class SceneStatesData : ScriptableObject {
        public const string StateAnchorID = "INTERNAL_State_Auto";

        [System.Serializable]
        public class LocationData {
            public float positionX;
            public bool facingRight;
            public Expression condition;
        }

        public LocationData[] locations;
        public string sceneKey;
        public string articyVariable;
        public Vector2 bastheetHeadMapPosition;

        public LocationData GetNearestLocation(float position) {
            LocationData nearestLocation = default;
            float nearestDistanceSqr = Mathf.Infinity;

            foreach (LocationData location in locations) {
                if (!location.condition.Get()) continue;

                float distanceSqr = Mathf.Abs(position - location.positionX);

                if (distanceSqr < nearestDistanceSqr) {
                    nearestDistanceSqr = distanceSqr;
                    nearestLocation = location;
                }
            }

            return nearestLocation;
        }
    }
}