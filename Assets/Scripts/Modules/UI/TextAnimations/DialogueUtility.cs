using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NFHGame.DialogueSystem {
    public static class DialogueUtility {
        private const string k_RemainderRegex = "(.*?((?=>)|(/|$)))";
        private const string k_PauseRegexString = "<p:(?<pause>" + k_RemainderRegex + ")>";
        private static readonly Regex s_PauseRegex = new Regex(k_PauseRegexString);
        private const string k_SpeedRegexString = "<sp:(?<speed>" + k_RemainderRegex + ")>";
        private static readonly Regex s_SpeedRegex = new Regex(k_SpeedRegexString);
        private const string k_AnimStartRegexString = "<anim:(?<anim>" + k_RemainderRegex + ")>";
        private static readonly Regex s_AnimStartRegex = new Regex(k_AnimStartRegexString);
        private const string k_AnimEndRegexString = "</anim>";
        private static readonly Regex s_AnimEndRegex = new Regex(k_AnimEndRegexString);

        public const float DefaultScrollSpeed = 50F;

        private static readonly Dictionary<string, float> s_PauseDictionary = new Dictionary<string, float>{
            { "tiny", .1f },
            { "short", .25f },
            { "normal", .666f },
            { "long", 1f },
            { "read", 2f },
        };

        public static readonly Regex variableRegex = new Regex("(?<={)(.*?)(?=})");
        public static readonly Dictionary<string, string> variablesDictionary = new Dictionary<string, string> { };

        public static string ProcessSmartDialogue(string label) {
            MatchCollection matches = variableRegex.Matches(label);
            for (int i = 0; i < matches.Count; i++) {
                Match match = matches[i];
                if (variablesDictionary.TryGetValue(match.ToString(), out string variable))
                    label = label.Replace(new StringBuilder().AppendFormat("{{{0}}}", match).ToString(), variable);
            }

            return label;
        }

        public static List<DialogueCommand> ProcessInputString(string message, out string processedMessage) {
            List<DialogueCommand> result = new List<DialogueCommand>();
            processedMessage = message;

            processedMessage = HandlePauseTags(processedMessage, result);
            processedMessage = HandleSpeedTags(processedMessage, result);
            processedMessage = HandleAnimStartTags(processedMessage, result);
            processedMessage = HandleAnimEndTags(processedMessage, result);

            return result;
        }

        private static string HandleAnimEndTags(string processedMessage, List<DialogueCommand> result) {
            MatchCollection animEndMatches = s_AnimEndRegex.Matches(processedMessage);
            for (int i = 0; i < animEndMatches.Count; i++) {
                Match match = animEndMatches[i];
                result.Add(new DialogueCommand {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.AnimEnd,
                });
            }
            processedMessage = Regex.Replace(processedMessage, k_AnimEndRegexString, string.Empty);
            return processedMessage;
        }

        private static string HandleAnimStartTags(string processedMessage, List<DialogueCommand> result) {
            MatchCollection animStartMatches = s_AnimStartRegex.Matches(processedMessage);
            for (int i = 0; i < animStartMatches.Count; i++) {
                Match match = animStartMatches[i];
                string stringVal = match.Groups["anim"].Value;
                result.Add(new DialogueCommand {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.AnimStart,
                    textAnimValue = GetTextAnimationType(stringVal)
                });
            }
            processedMessage = Regex.Replace(processedMessage, k_AnimStartRegexString, string.Empty);
            return processedMessage;
        }

        private static string HandleSpeedTags(string processedMessage, List<DialogueCommand> result) {
            MatchCollection speedMatches = s_SpeedRegex.Matches(processedMessage);
            for (int i = 0; i < speedMatches.Count; i++) {
                Match match = speedMatches[i];
                string stringVal = match.Groups["speed"].Value;
                if (!float.TryParse(stringVal, out float val))
                    val = DefaultScrollSpeed;
                result.Add(new DialogueCommand {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.TextSpeedChange,
                    floatValue = val
                });
            }
            processedMessage = Regex.Replace(processedMessage, k_SpeedRegexString, string.Empty);
            return processedMessage;
        }

        private static string HandlePauseTags(string processedMessage, List<DialogueCommand> result) {
            MatchCollection pauseMatches = s_PauseRegex.Matches(processedMessage);
            for (int i = 0; i < pauseMatches.Count; i++) {
                Match match = pauseMatches[i];
                string val = match.Groups["pause"].Value;
                string pauseName = val;
                Debug.Assert(s_PauseDictionary.ContainsKey(pauseName), $"No pause registered for '{pauseName}'.");
                result.Add(new DialogueCommand {
                    position = VisibleCharactersUpToIndex(processedMessage, match.Index),
                    type = DialogueCommandType.Pause,
                    floatValue = s_PauseDictionary[pauseName]
                });
            }
            processedMessage = Regex.Replace(processedMessage, k_PauseRegexString, string.Empty);
            return processedMessage;
        }

        private static TextAnimationType GetTextAnimationType(string stringVal) {
            TextAnimationType result;
            try {
                result = (TextAnimationType)Enum.Parse(typeof(TextAnimationType), stringVal, true);
            } catch (ArgumentException) {
                GameLogger.LogError($"Invalid Text Animation Type: {stringVal}");
                result = TextAnimationType.none;
            }
            return result;
        }

        private static int VisibleCharactersUpToIndex(string message, int index) {
            int result = 0;
            bool insideBrackets = false;
            for (int i = 0; i < index; i++) {
                if (message[i] == '<')
                    insideBrackets = true;
                else if (message[i] == '>') {
                    insideBrackets = false;
                    result--;
                }

                if (!insideBrackets)
                    result++;
                else if (i + 6 < index && message.Substring(i, 6) == "sprite")
                    result++;
            }
            return result;
        }
    }

    public struct DialogueCommand {
        public int position;
        public DialogueCommandType type;
        public float floatValue;
        public string stringValue;
        public TextAnimationType textAnimValue;
    }

    public enum DialogueCommandType {
        Pause,
        TextSpeedChange,
        AnimStart,
        AnimEnd
    }

    public enum TextAnimationType {
        none,
        shake,
        wave
    }
}