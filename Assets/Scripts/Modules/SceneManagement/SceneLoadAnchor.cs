using UnityEngine;
using UnityEngine.Events;
using static NFHGame.SceneManagement.SceneLoader;

namespace NFHGame.SceneManagement {
    public class SceneLoadAnchor : MonoBehaviour {
        [SerializeField] private string m_AnchorID;
        [SerializeField] private UnityEvent<SceneLoadingHandler> m_OnLoad;

        public string anchorID => m_AnchorID;
        public UnityEvent<SceneLoadingHandler> onLoad => m_OnLoad;
    }
}