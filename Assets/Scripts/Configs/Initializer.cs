using NFHGame.ScriptableSingletons;
using UnityEngine;

namespace NFHGame.Configs {
    public static class Initializer {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeScriptableSingletons() {
            var initializerManager = Resources.Load<InitializerManager>("InitializerManager");
            foreach (var loadObject in initializerManager.loadObjects) {
                if (loadObject is ISingletonLoader singletonLoader) {
                    singletonLoader.LoadSingleton(loadObject as ScriptableObject);
                }
                if (loadObject is GameObject gameObject) {
                    GameObject.Instantiate(gameObject);
                }
                if (loadObject is IBootableSingleton bootableSingleton) {
                    bootableSingleton.Initialize();
                }
            }
        }
    }

    public interface IBootableSingleton {
        void Initialize();
    }
}
