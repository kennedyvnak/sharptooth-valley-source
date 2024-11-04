using NFHGame.SceneManagement;

namespace NFHGame.Serialization.States {
    [System.Serializable]
    public class SerializationState {
        public SceneReference sceneRef;
        public float endPosition;
        public string id;

        public SerializationState(string scenePath, float endPosition, string id) {
            sceneRef = new SceneReference();
            sceneRef.scenePath = scenePath;
            this.endPosition = endPosition;
            this.id = id;
        }
    }
}