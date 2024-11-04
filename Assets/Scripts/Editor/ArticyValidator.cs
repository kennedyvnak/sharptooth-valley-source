using Articy.SharptoothValley;
using Articy.Unity;
using Articy.Unity.Interfaces;
using NFHGame;
using NFHGame.ArticyImpl;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.Actors;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NFHGameEditor {
    public static class ArticyValidator {
        [MenuItem("Tools/Articy Valitor/Validate Articy")]
        private static void ValidateArticy() {
            var database = ArticyDatabase.Objects;
            foreach (var obj in database) {
                if (obj is not IDialogueFragment frag) continue;
                var speaker = ((IObjectWithSpeaker)obj).Speaker;
                var speech = obj.ExtractText();

                var id = ((ArticyObject)frag).TechnicalName;
                if (speaker == null) Debug.LogWarning($"Fragment {id} with no speaker");
                if (string.IsNullOrWhiteSpace(speech) && !Helpers.StringHelpers.StartsWith(((IObjectWithMenuText)obj).MenuText, "<sprite")) Debug.LogWarning($"Fragment {id} with no text");
            }
        }

        [MenuItem("Tools/Articy Valitor/Validate Articy Text Animations")]
        private static void ValidateArticyText() {
            var database = ArticyDatabase.Objects;
            foreach (var obj in database) {
                if (obj is not IDialogueFragment frag) continue;

                var speaker = ((IObjectWithSpeaker)obj).Speaker;
                var speech = obj.ExtractText();
                var menuText = ((IObjectWithMenuText)obj).MenuText;

                var id = ((ArticyObject)frag).TechnicalName;
                var speechAnim = DialogueUtility.ProcessInputString(speech, out _);
                var menuTextAnim = DialogueUtility.ProcessInputString(menuText, out _);

                if (menuTextAnim.Count > 0 || (string.IsNullOrWhiteSpace(menuText) && speechAnim.Count > 0 && speaker != null && speaker.TechnicalName == "Ntt_42AACF90")) Debug.LogWarning($"Fragment {id} with menu text zoado");

                int animStartCommands = 0;
                int animEndCommands = 0;
                for (int i = 0; i < speechAnim.Count; i++) {
                    DialogueCommand command = speechAnim[i];
                    if (command.type == DialogueCommandType.AnimStart) {
                        animStartCommands++;
                    } else if (command.type == DialogueCommandType.AnimEnd) {
                        animEndCommands++;
                    }
                }

                if (animStartCommands != animEndCommands) {
                    Debug.LogError($"Fragment {id} with text animations quebrado");
                }
            }
        }

        [MenuItem("Tools/Articy Valitor/Validate Articy Expressions")]
        private static void ValidateArticyExpressions() {
            var portraitsDB = AssetDatabase.LoadAssetAtPath<DialogueDatabase>("Assets/Data/Singletons/ScriptableSingletons/DialogueDatabase.asset");

            var database = ArticyDatabase.Objects;

            StringBuilder builder = new StringBuilder();
            foreach (var obj in database) {
                if (obj is not IDialogueFragment) continue;
                var speaker = ((IObjectWithSpeaker)obj).Speaker;

                if (!portraitsDB.TryGetActor(speaker.TechnicalName, out var dActor)) {
                    Debug.LogError($"{obj.TechnicalName} don't have a valid actor.");
                    continue;
                }

                if (!dActor.portraitCollection || dActor.portraitCollection.partsCollection.Length == 0) {
                    continue;
                }

                if (obj is not IObjectWithStageDirections stageDirections) {
                    Debug.LogError($"{obj.TechnicalName} don't have a stage directions. ???");
                    continue;
                }

                var portraitCollection = dActor.portraitCollection;

                if (!portraitCollection) continue;

                try {
                    GetPortrait(stageDirections.StageDirections, dActor.actor);
                } catch (NullReferenceException e) {
                    LogError(e.ToString());
                } catch (IndexOutOfRangeException e) {
                    LogError(e.ToString());
                }

                void GetPortrait(string id, DialogueActor.Actor actor) {
                    if (Helpers.StringHelpers.StartsWith(id, ":"))
                        GetFromIndex(id.Remove(0, 1), actor);
                    else
                        GetFromName(id, actor);
                }

                void GetFromIndex(string rawIndex, DialogueActor.Actor actor) {
                    try {
                        var indexes = System.Array.ConvertAll(rawIndex.Split(','), i => int.Parse(i));
                        GetFromIndexes(indexes, actor);
                    } catch (System.FormatException e) {
                        LogWarning(e.ToString());
                    }
                }

                void GetFromName(string name, DialogueActor.Actor actor) {
                    if (portraitCollection.prefabs.TryGetValue(name, out var v)) {
                        GetFromIndexes(v.indexes, actor);
                    } else {
                        LogWarning($"Can't find portrait '{name}'");
                    }
                }

                void GetFromIndexes(int[] indexes, DialogueActor.Actor actor) {
                    if (indexes.Length != portraitCollection.partsCollection.Length) {
                        LogWarning("Indexes length doesn't match portrait parts length");
                    }

                    for (int i = 0; i < indexes.Length; i++) {
                        int index = indexes[i];
                        var collection = portraitCollection.partsCollection[i];
                        if (index < 0 || index >= collection.parts.Count) {
                            LogError("Index invalid at PortraitCollection.GetFromIndexes()");
                            index = 0;
                        }
                    }
                }

                void LogWarning(string t) {
                    Debug.LogWarning($"[{obj.TechnicalName}] {t}");
                    builder.AppendLine($"[{obj.TechnicalName}] {t}");
                }

                void LogError(string t) {
                    Debug.LogError($"[{obj.TechnicalName}] {t}");
                    builder.AppendLine($"[{obj.TechnicalName}] {t}");
                }
            }
            Debug.Log(builder);
        }

        [MenuItem("Tools/Articy Valitor/Validate Pins")]
        private static void ValidateArticyPins() {
            var database = ArticyDatabase.Objects;
            foreach (var obj in database) {
                if (obj is Condition) continue;
                
                if (obj is IInputPinsOwner iPinOwner) {
                    if (iPinOwner.GetInputPins().Count > 1) {
                        Debug.LogError($"[{obj.TechnicalName}] more than 1 input pin.");
                    }
                }

                if (obj is IOutputPinsOwner iOutputPinOwner) {
                    if (iOutputPinOwner.GetOutputPins().Count > 1) {
                        Debug.LogError($"[{obj.TechnicalName}] more than 1 output pin.");
                    } 
                }
            }
        }
    }
}
