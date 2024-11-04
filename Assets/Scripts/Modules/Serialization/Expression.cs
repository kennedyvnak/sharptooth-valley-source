using NFHGame.ArticyImpl.Variables;
using NFHGame.SceneManagement.GameKeys;
using System;
using UnityEngine;

namespace NFHGame.Serialization {
    [Serializable]
    public class Expression {
        [Serializable]
        public class Condition {
            public enum Operator { And, Or }

            public bool not;
            public string text;
            public Operator op;
        }

        public Condition[] conditions;

        public bool Get(GameData data) {
            bool result = true;
            Condition.Operator lastOp = Condition.Operator.And;

            for (int i = 0; i < conditions.Length; i++) {
                var condition = conditions[i];
                bool val = condition.not ? !GetCondition(condition.text, data) : GetCondition(condition.text, data);
                result = i != 0 ? Compute(lastOp, result, val) : val;
                lastOp = condition.op;
            }
            return result;
        }

        public bool Get() {
            bool result = true;
            Condition.Operator lastOp = Condition.Operator.And;

            for (int i = 0; i < conditions.Length; i++) {
                var condition = conditions[i];
                bool val = condition.not ? !GetCondition(condition.text) : GetCondition(condition.text);
                result = i != 0 ? Compute(lastOp, result, val) : val;
                lastOp = condition.op;
            }
            return result;
        }

        /*public bool Get(ProgressMap.OverrideExpresion[] data) {
            bool result = true;
            Condition.Operator lastOp = Condition.Operator.And;

            for (int i = 0; i < conditions.Length; i++) {
                var condition = conditions[i];
                bool val = condition.not ? !Get(condition.text) : Get(condition.text);
                result = i != 0 ? Compute(lastOp, result, val) : val;
                lastOp = condition.op;
            }
            return result;

            bool Get(string text) {
                var over = Array.Find(data, (x) => x.key == text);
                return over != null && over.value;
            }
        }*/

        private static bool Compute(Condition.Operator op, bool result, bool val) {
            return op == Condition.Operator.And ? result && val : result || val;
        }

        private static bool GetCondition(string condition, GameData data) {
            if (condition.Contains(".")) {
                return GetArticy(condition, data);
            } else {
                return GetKey(condition, data);
            }
        }

        private static bool GetCondition(string condition) {
            if (condition.Contains(".")) {
                return ArticyVariables.globalVariables.GetVariableByString<bool>(condition);
            } else {
                return GameKeysManager.instance.HaveGameKey(condition);
            }
        }

        private static bool GetArticy(string variable, GameData data) {
            string vars = data.articyNumberVariables;
            int index = vars.IndexOf(variable);
            if (index == -1) {
                GameLogger.articy.LogError($"Can't find var {variable} in {data.articyNumberVariables}");
                return false;
            }

            return vars[index + variable.Length + 1] == '1';
        }

        private static bool GetKey(string key, GameData data) {
            return data.gameKeys.Contains(key);
        }
    }
}
