using UnityEngine;

namespace NFHGame.Configs {
    public class InitializerManager : ScriptableObject {
        [SerializeField] private Object[] m_LoadObjects;

        public Object[] loadObjects => m_LoadObjects;
    }
}