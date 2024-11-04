using UnityEngine;

namespace NFHGame.SceneManagement {
    public class SceneLoadHandler : MonoBehaviour {
        [SerializeField] private SceneReference m_SceneReference;
        [SerializeField] private string m_AnchorID;
        [SerializeField] private bool m_StopInputWhenStart = true;

        public void LoadScene() {
            var handler = SceneLoader.instance.LoadScene(m_SceneReference, m_AnchorID);
            if (m_StopInputWhenStart)
                handler.StopInput();
        }
    }
}
