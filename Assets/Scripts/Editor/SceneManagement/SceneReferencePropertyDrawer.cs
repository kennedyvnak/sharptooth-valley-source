using System.Linq;
using NFHGame.SceneManagement;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace NFHGameEditor {
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer {
        private const string k_SceneAssetPropertyString = "m_SceneAsset";
        private const string k_ScenePathPropertyString = "m_ScenePath";

        private static readonly RectOffset k_BoxPadding = EditorStyles.helpBox.padding;

        private const float k_PadSize = 2f;
        private const float k_FooterHeight = 10f;

        private static readonly float s_LineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float s_PaddedLine = s_LineHeight + k_PadSize;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, s_LineHeight), property.isExpanded, label, true);

            if (property.isExpanded) {
                position.height -= s_LineHeight;
                position.y += s_LineHeight;

                var sceneAssetProperty = GetSceneAssetProperty(property);

                position.height -= k_FooterHeight;
                GUI.Box(EditorGUI.IndentedRect(position), GUIContent.none, EditorStyles.helpBox);
                position = k_BoxPadding.Remove(position);
                position.height = s_LineHeight;

                label.tooltip = "The actual Scene Asset reference.\nOn serialize this is also stored as the asset's path.";


                var sceneControlID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUI.BeginChangeCheck();
                {
                    sceneAssetProperty.objectReferenceValue = EditorGUI.ObjectField(position, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);
                }
                var buildScene = BuildUtils.GetBuildScene(sceneAssetProperty.objectReferenceValue);
                if (EditorGUI.EndChangeCheck()) {
                    if (buildScene.scene == null) GetScenePathProperty(property).stringValue = string.Empty;
                }

                position.y += s_PaddedLine;

                if (!buildScene.assetGUID.Empty()) {
                    DrawSceneInfoGUI(position, buildScene, sceneControlID + 1);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var sceneAssetProperty = GetSceneAssetProperty(property);
            if (property.isExpanded) {
                int lines = sceneAssetProperty.objectReferenceValue ? 3 : 2;
                return k_BoxPadding.vertical + s_LineHeight * lines + k_PadSize * (lines - 1) + k_FooterHeight;
            }

            return s_LineHeight + k_PadSize;
        }

        private void DrawSceneInfoGUI(Rect position, BuildUtils.BuildScene buildScene, int sceneControlID) {
            var readOnly = BuildUtils.IsReadOnly();
            var readOnlyWarning = readOnly ? "\n\nWARNING: Build Settings is not checked out and so cannot be modified." : "";

            GUIContent iconContent;
            var labelContent = new GUIContent();

            if (buildScene.buildIndex == -1) {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close");
                labelContent.text = "NOT In Build";
                labelContent.tooltip = "This scene is NOT in build settings.\nIt will be NOT included in builds.";
            } else if (buildScene.scene.enabled) {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_max");
                labelContent.text = "BuildIndex: " + buildScene.buildIndex;
                labelContent.tooltip = "This scene is in build settings and ENABLED.\nIt will be included in builds." + readOnlyWarning;
            } else {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_min");
                labelContent.text = "BuildIndex: " + buildScene.buildIndex;
                labelContent.tooltip = "This scene is in build settings and DISABLED.\nIt will be NOT included in builds.";
            }

            using (new EditorGUI.DisabledScope(readOnly)) {
                var labelRect = DrawUtils.GetLabelRect(position);
                var iconRect = labelRect;
                iconRect.width = iconContent.image.width + k_PadSize;
                labelRect.width -= iconRect.width;
                labelRect.x += iconRect.width;
                EditorGUI.PrefixLabel(iconRect, sceneControlID, iconContent);
                EditorGUI.PrefixLabel(labelRect, sceneControlID, labelContent);
            }

            var buttonRect = DrawUtils.GetFieldRect(position);
            buttonRect.width = (buttonRect.width) / 3;

            string tooltipMsg;
            using (new EditorGUI.DisabledScope(readOnly)) {
                if (buildScene.buildIndex == -1) {
                    buttonRect.width *= 2;
                    var addIndex = EditorBuildSettings.scenes.Length;
                    tooltipMsg = "Add this scene to build settings. It will be appended to the end of the build scenes as buildIndex: " + addIndex + "." + readOnlyWarning;
                    if (DrawUtils.ButtonHelper(buttonRect, "Add...", "Add (buildIndex " + addIndex + ")", EditorStyles.miniButtonLeft, tooltipMsg))
                        BuildUtils.AddBuildScene(buildScene);
                    buttonRect.width /= 2;
                    buttonRect.x += buttonRect.width;
                } else {
                    var isEnabled = buildScene.scene.enabled;
                    var stateString = isEnabled ? "Disable" : "Enable";
                    tooltipMsg = stateString + " this scene in build settings.\n" + (isEnabled ? "It will no longer be included in builds" : "It will be included in builds") + "." + readOnlyWarning;

                    if (DrawUtils.ButtonHelper(buttonRect, stateString, stateString + " In Build", EditorStyles.miniButtonLeft, tooltipMsg))
                        BuildUtils.SetBuildSceneState(buildScene, !isEnabled);
                    buttonRect.x += buttonRect.width;

                    tooltipMsg = "Completely remove this scene from build settings.\nYou will need to add it again for it to be included in builds!" + readOnlyWarning;
                    if (DrawUtils.ButtonHelper(buttonRect, "Remove...", "Remove from Build", EditorStyles.miniButtonMid, tooltipMsg))
                        BuildUtils.RemoveBuildScene(buildScene);
                }
            }

            buttonRect.x += buttonRect.width;

            tooltipMsg = "Open the 'Build Settings' Window for managing scenes." + readOnlyWarning;
            if (DrawUtils.ButtonHelper(buttonRect, "Settings", "Build Settings", EditorStyles.miniButtonRight, tooltipMsg)) {
                BuildUtils.OpenBuildSettings();
            }

        }

        private static SerializedProperty GetSceneAssetProperty(SerializedProperty property) {
            return property.FindPropertyRelative(k_SceneAssetPropertyString);
        }

        private static SerializedProperty GetScenePathProperty(SerializedProperty property) {
            return property.FindPropertyRelative(k_ScenePathPropertyString);
        }

        private static class DrawUtils {
            public static bool ButtonHelper(Rect position, string msgShort, string msgLong, GUIStyle style, string tooltip = null) {
                var content = new GUIContent(msgLong) { tooltip = tooltip };

                var longWidth = style.CalcSize(content).x;
                if (longWidth > position.width) content.text = msgShort;

                return GUI.Button(position, content, style);
            }

            public static Rect GetFieldRect(Rect position) {
                position.width -= EditorGUIUtility.labelWidth;
                position.x += EditorGUIUtility.labelWidth;
                return position;
            }

            public static Rect GetLabelRect(Rect position) {
                position.width = EditorGUIUtility.labelWidth - k_PadSize;
                return position;
            }
        }

        private static class BuildUtils {
            public static float minCheckWait = 3;

            private static float lastTimeChecked;
            private static bool cachedReadonlyVal = true;

            public struct BuildScene {
                public int buildIndex;
                public GUID assetGUID;
                public string assetPath;
                public EditorBuildSettingsScene scene;
            }

            /// </summary>
            public static bool IsReadOnly() {
                var curTime = Time.realtimeSinceStartup;
                var timeSinceLastCheck = curTime - lastTimeChecked;

                if (!(timeSinceLastCheck > minCheckWait)) return cachedReadonlyVal;

                lastTimeChecked = curTime;
                cachedReadonlyVal = QueryBuildSettingsStatus();

                return cachedReadonlyVal;
            }

            private static bool QueryBuildSettingsStatus() {
                if (!Provider.enabled) return false;

                if (!Provider.hasCheckoutSupport) return false;

                var status = Provider.Status("ProjectSettings/EditorBuildSettings.asset", false);
                status.Wait();

                if (status.assetList == null || status.assetList.Count != 1) return true;

                return !status.assetList[0].IsState(Asset.States.CheckedOutLocal);
            }

            public static BuildScene GetBuildScene(Object sceneObject) {
                var entry = new BuildScene {
                    buildIndex = -1,
                    assetGUID = new GUID(string.Empty)
                };

                if (sceneObject as SceneAsset == null) return entry;

                entry.assetPath = AssetDatabase.GetAssetPath(sceneObject);
                entry.assetGUID = new GUID(AssetDatabase.AssetPathToGUID(entry.assetPath));

                var scenes = EditorBuildSettings.scenes;
                for (var index = 0; index < scenes.Length; ++index) {
                    if (!entry.assetGUID.Equals(scenes[index].guid)) continue;

                    entry.scene = scenes[index];
                    entry.buildIndex = index;
                    return entry;
                }

                return entry;
            }

            public static void SetBuildSceneState(BuildScene buildScene, bool enabled) {
                var modified = false;
                var scenesToModify = EditorBuildSettings.scenes;
                foreach (var curScene in scenesToModify.Where(curScene => curScene.guid.Equals(buildScene.assetGUID))) {
                    curScene.enabled = enabled;
                    modified = true;
                    break;
                }
                if (modified) EditorBuildSettings.scenes = scenesToModify;
            }

            public static void AddBuildScene(BuildScene buildScene, bool force = false, bool enabled = true) {
                if (force == false) {
                    var selection = EditorUtility.DisplayDialogComplex(
                        "Add Scene To Build",
                        "You are about to add scene at " + buildScene.assetPath + " To the Build Settings.",
                        "Add as Enabled",       // option 0
                        "Add as Disabled",      // option 1
                        "Cancel (do nothing)"); // option 2

                    switch (selection) {
                        case 0: // enabled
                            enabled = true;
                            break;
                        case 1: // disabled
                            enabled = false;
                            break;
                        default:
                            //case 2: // cancel
                            return;
                    }
                }

                var newScene = new EditorBuildSettingsScene(buildScene.assetGUID, enabled);
                var tempScenes = EditorBuildSettings.scenes.ToList();
                tempScenes.Add(newScene);
                EditorBuildSettings.scenes = tempScenes.ToArray();
            }

            public static void RemoveBuildScene(BuildScene buildScene, bool force = false) {
                var onlyDisable = false;
                if (force == false) {
                    var selection = -1;

                    var title = "Remove Scene From Build";
                    var details = $"You are about to remove the following scene from build settings:\n    {buildScene.assetPath}\n    buildIndex: {buildScene.buildIndex}\n\nThis will modify build settings, but the scene asset will remain untouched.";
                    var confirm = "Remove From Build";
                    var alt = "Just Disable";
                    var cancel = "Cancel (do nothing)";

                    if (buildScene.scene.enabled) {
                        details += "\n\nIf you want, you can also just disable it instead.";
                        selection = EditorUtility.DisplayDialogComplex(title, details, confirm, alt, cancel);
                    } else {
                        selection = EditorUtility.DisplayDialog(title, details, confirm, cancel) ? 0 : 2;
                    }

                    switch (selection) {
                        case 0: // remove
                            break;
                        case 1: // disable
                            onlyDisable = true;
                            break;
                        default:
                            //case 2: // cancel
                            return;
                    }
                }

                if (onlyDisable) {
                    SetBuildSceneState(buildScene, false);
                } else {
                    var tempScenes = EditorBuildSettings.scenes.ToList();
                    tempScenes.RemoveAll(scene => scene.guid.Equals(buildScene.assetGUID));
                    EditorBuildSettings.scenes = tempScenes.ToArray();
                }
            }

            public static void OpenBuildSettings() {
                EditorWindow.GetWindow(typeof(BuildPlayerWindow));
            }
        }
    }
}