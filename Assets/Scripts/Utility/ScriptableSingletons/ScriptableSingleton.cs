using UnityEngine;

namespace NFHGame.ScriptableSingletons {
    public interface ISingletonLoader {
        void LoadSingleton(ScriptableObject obj);
    }

    public abstract class ScriptableSingleton<T> : ScriptableObject, ISingletonLoader where T : ScriptableSingleton<T> {
        private static T s_Instance;

        public static T instance => s_Instance;

        void ISingletonLoader.LoadSingleton(ScriptableObject obj) {
            if (s_Instance) return;
            s_Instance = obj as T;
        }
    }
}