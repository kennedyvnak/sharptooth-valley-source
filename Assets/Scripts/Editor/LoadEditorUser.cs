using NFHGame.SceneManagement;
using NFHGame.Serialization.States;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace NFHGameEditor {
    public static class LoadEditorUser {
        private const string k_MenuName = "Tools/Start With Editor User";

        private static bool s_Enabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad() {
            if (EditorPrefs.GetBool(k_MenuName, false) || SceneManager.GetActiveScene().name == "LoadEditorUser") {
                DataManager.instance.userManager.SetUser(0);
                var state = DataManager.instance.gameData.state;
                var handler = SceneLoader.instance.CreateHandler(state.sceneRef, SceneStatesData.StateAnchorID);
                handler.blackScreen = true;
                handler.saveGame = false;
                SceneLoader.instance.LoadScene(handler);
            }
        }

        static LoadEditorUser() {
            s_Enabled = EditorPrefs.GetBool(k_MenuName, false);

            EditorApplication.delayCall += () => {
                PerformAction(s_Enabled);
            };
        }

        [MenuItem(k_MenuName)]
        private static void ToggleAction() {
            PerformAction(!s_Enabled);
        }

        public static void PerformAction(bool enabled) {
            Menu.SetChecked(k_MenuName, enabled);
            EditorPrefs.SetBool(k_MenuName, enabled);
            s_Enabled = enabled;
        }
    }
}
