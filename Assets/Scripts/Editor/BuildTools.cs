using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using System.IO;
using NFHGame;

namespace NFHGameEditor {
    public class BuildTools : EditorWindow {
        private const string k_BuildsFolder = "Builds";

        [MenuItem("Tools/Build Tools")]
        public static void ShowBuildTools() {
            EditorWindow.GetWindow<BuildTools>();
        }

        private BuildTargetGroup GetTargetGroupForTarget(BuildTarget target) => target switch {
            BuildTarget.StandaloneOSX => BuildTargetGroup.Standalone,
            BuildTarget.StandaloneWindows => BuildTargetGroup.Standalone,
            BuildTarget.iOS => BuildTargetGroup.iOS,
            BuildTarget.Android => BuildTargetGroup.Android,
            BuildTarget.StandaloneWindows64 => BuildTargetGroup.Standalone,
            BuildTarget.WebGL => BuildTargetGroup.WebGL,
            BuildTarget.StandaloneLinux64 => BuildTargetGroup.Standalone,
            _ => BuildTargetGroup.Unknown
        };

        private Dictionary<BuildTarget, bool> _targetsToBuild = new Dictionary<BuildTarget, bool>();
        private List<BuildTarget> _availableTargets = new List<BuildTarget>();

        private void OnEnable() {
            _availableTargets.Clear();

            var buildTargets = System.Enum.GetValues(typeof(BuildTarget));
            foreach (var buildTargetValue in buildTargets) {
                BuildTarget target = (BuildTarget)buildTargetValue;

                if (!BuildPipeline.IsBuildTargetSupported(GetTargetGroupForTarget(target), target))
                    continue;

                _availableTargets.Add(target);

                if (!_targetsToBuild.ContainsKey(target))
                    _targetsToBuild[target] = false;
            }

            if (_targetsToBuild.Count > _availableTargets.Count) {
                List<BuildTarget> targetsToRemove = new List<BuildTarget>();
                foreach (var target in _targetsToBuild.Keys) {
                    if (!_availableTargets.Contains(target))
                        targetsToRemove.Add(target);
                }

                foreach (var target in targetsToRemove)
                    _availableTargets.Remove(target);
            }
        }

        private void OnGUI() {
            GUILayout.Label("Platforms to Build", EditorStyles.boldLabel);
            GUILayout.Space(2.0f);

            int numEnabled = 0;
            foreach (var target in _availableTargets) {
                _targetsToBuild[target] = EditorGUILayout.Toggle(target.ToString(), _targetsToBuild[target]);

                if (_targetsToBuild[target])
                    numEnabled++;
            }

            GUILayout.Space(2.0f);
            if (numEnabled > 0 && GUILayout.Button($"Start Build Process")) {
                List<BuildTarget> selectedTargets = new List<BuildTarget>();
                foreach (var target in _availableTargets) {
                    if (_targetsToBuild[target])
                        selectedTargets.Add(target);
                }

                EditorCoroutineUtility.StartCoroutine(PerformBuild(selectedTargets), this);
            }
        }

        IEnumerator PerformBuild(List<BuildTarget> targets) {
            const float DelayTime = 1.0f;

            BuildTarget originalTarget = EditorUserBuildSettings.activeBuildTarget;

            var buildProgressID = Progress.Start("Build Platforms", "Build all selected platforms", Progress.Options.Sticky);
            Progress.ShowDetails();
            yield return new EditorWaitForSeconds(DelayTime);

            for (int i = 0; i < targets.Count; i++) {
                BuildTarget target = targets[i];
                var buildTaskID = Progress.Start($"Build {target}", null, Progress.Options.Sticky, buildProgressID);
                yield return new EditorWaitForSeconds(DelayTime);

                if (!BuildIndividualTarget(target)) {
                    Progress.Finish(buildTaskID, Progress.Status.Failed);
                    Progress.Finish(buildProgressID, Progress.Status.Failed);
                    Leave();
                    yield break;
                }

                Progress.Finish(buildTaskID, Progress.Status.Succeeded);
                Progress.Report(buildProgressID, i + 1, targets.Count);
            }

            Progress.Finish(buildProgressID, Progress.Status.Succeeded);
            yield return new EditorWaitForSeconds(DelayTime);

            Leave();
            var logger = AssetDatabase.LoadAssetAtPath<GameLogger>("Assets/Data/Singletons/ScriptableSingletons/GameLogger.asset");
            var version = logger.currentGameVersion;
            var autoPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds/Automation");
            File.WriteAllText(Path.Combine(autoPath, "version.txt"), version.Remove(0, 1));
            EditorUtility.RevealInFinder(Path.Combine(autoPath, "publish_to_itch.bat"));

            yield return null;

            void Leave() {
                if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetTargetGroupForTarget(originalTarget), originalTarget);
            }
        }

        private bool BuildIndividualTarget(BuildTarget target) {
            List<string> scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                scenes.Add(scene.path);

            string locationPathName = GetBuildPath(target);
            BuildPlayerOptions options = new BuildPlayerOptions() {
                scenes = scenes.ToArray(),
                target = target,
                targetGroup = GetTargetGroupForTarget(target),
                locationPathName = locationPathName,
                options = BuildPipeline.BuildCanBeAppended(target, locationPathName) == CanAppendBuild.Yes ? BuildOptions.AcceptExternalModificationsToPlayer : BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded) {
                Debug.Log($"[Build Tools] {target} builded succesfully at {report.summary.totalTime.Seconds}s.\nsize = {report.summary.totalSize / 1e+6}MB\npath = {report.summary.outputPath}");
                return true;
            }

            Debug.LogError($"[Build Tools] Build for {target} failed.");
            return false;
        }

        private string GetBuildPath(BuildTarget target) {
            var logger = AssetDatabase.LoadAssetAtPath<GameLogger>("Assets/Data/Singletons/ScriptableSingletons/GameLogger.asset");
            var version = logger.currentGameVersion;

            return target switch {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => Path.Combine(k_BuildsFolder, version, target.ToString(), PlayerSettings.productName + ".exe"),
                BuildTarget.WebGL => Path.Combine(k_BuildsFolder, version, target.ToString()),
                BuildTarget.Android => Path.Combine(k_BuildsFolder, version, target.ToString(), PlayerSettings.productName + ".apk"),
                BuildTarget.StandaloneLinux64 => Path.Combine(k_BuildsFolder, version, target.ToString(), PlayerSettings.productName + ".x86_64"),
                _ => Path.Combine(k_BuildsFolder, version, target.ToString(), PlayerSettings.productName),
            };
        }
    }
}