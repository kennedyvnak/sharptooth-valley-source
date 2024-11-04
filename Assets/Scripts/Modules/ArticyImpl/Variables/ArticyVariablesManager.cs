using System.Collections.Generic;
using System.Text;
using NFHGame.Serialization;
using UnityEngine;
using static NFHGame.ArticyImpl.ArticyManager;

namespace NFHGame.ArticyImpl.Variables {
    public class ArticyVariablesManager : ScriptableObject {
        private const char k_PairSeparator = ';';
        private const char k_KeyValueSeparator = ':';

        public void SerializeArticyVariablesToGameData(GameData data) {
            GetVariablesDictionaries(out var numberVariables, out var stringVariables);

            data.articyNumberVariables = BuildDictionary<int>(numberVariables);
            data.articyStringVariables = BuildDictionary<string>(stringVariables);
        }

        public void DeserializeGameDataToArticyVariables(GameData data) {
            var numberVariables = DeserializeDictionary(data.articyNumberVariables, ArticyManager.instance.defaultNumberVariables, (x) => int.Parse(x));
            var stringVariables = DeserializeDictionary(data.articyStringVariables, ArticyManager.instance.defaultStringVariables, (x) => x);
            SetVariables(numberVariables, stringVariables);
        }

        public static void GetVariablesDictionaries(out SerializedDictionary<string, int> numberVariables, out SerializedDictionary<string, string> stringVariables) {
            numberVariables = new SerializedDictionary<string, int>();
            stringVariables = new SerializedDictionary<string, string>();
            var globalVariables = ArticyVariables.globalVariables;

            foreach (var variable in globalVariables.Variables) {
                if (globalVariables.IsVariableOfTypeBoolean(variable.Key))
                    numberVariables.Add(variable.Key, (bool)variable.Value ? 1 : 0);
                else if (globalVariables.IsVariableOfTypeInteger(variable.Key))
                    numberVariables.Add(variable.Key, (int)variable.Value);
                else
                    stringVariables.Add(variable.Key, variable.Value.ToString());
            }
        }

        public static void SetVariables(IDictionary<string, int> numberVariables, IDictionary<string, string> stringVariables) {
            using (new ToggleRollbackScope(false)) {
                var globalVariables = ArticyVariables.globalVariables;

                foreach (var variable in numberVariables) {
                    if (globalVariables.Variables.ContainsKey(variable.Key)) {
                        if (globalVariables.IsVariableOfTypeBoolean(variable.Key))
                            globalVariables.SetVariableByString(variable.Key, variable.Value == 1);
                        else
                            globalVariables.SetVariableByString(variable.Key, variable.Value);
                    }
                }

                foreach (var variable in stringVariables) {
                    if (globalVariables.Variables.ContainsKey(variable.Key)) {
                        globalVariables.SetVariableByString(variable.Key, variable.Value);
                    }
                }
            }
        }

        public static string BuildDictionary<T>(IDictionary<string, T> dict) {
            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (var kvp in dict) {
                if (!first) builder.Append(k_PairSeparator);
                else first = false;

                builder.Append(kvp.Key);
                builder.Append(k_KeyValueSeparator);
                builder.Append(kvp.Value.ToString());
            }

            return builder.ToString();
        }

        public static IDictionary<string, T> DeserializeDictionary<T>(string str, SerializedDictionary<string, T> samples, System.Func<string, T> getValue) {
            Dictionary<string, T> dict = new Dictionary<string, T>(samples);
            var values = str.Split(k_PairSeparator);
            foreach (var value in values) {
                if (!value.Contains(k_KeyValueSeparator)) continue;
                var split = value.Split(k_KeyValueSeparator);
                dict[split[0]] = getValue(split[1]);
            }
            return dict;
        }
    }
}