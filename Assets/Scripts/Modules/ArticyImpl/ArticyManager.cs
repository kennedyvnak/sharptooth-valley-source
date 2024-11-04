using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Articy.Unity;
using Articy.Unity.Utils;
using NFHGame.AchievementsManagement;
using NFHGame.ArticyImpl.Variables;
using NFHGame.DialogueSystem;
using NFHGame.Inventory;
using NFHGame.Inventory.UI;
using NFHGame.Serialization;
using UnityEngine;

namespace NFHGame.ArticyImpl {
    public class ArticyManager : SingletonPersistent<ArticyManager> {
        public struct ToggleRollbackScope : IDisposable {
            public readonly bool enable;
            private bool _rollbackEnabled;

            public ToggleRollbackScope(bool enable) {
                this.enable = enable;
                _rollbackEnabled = ArticyManager.instance.rollback;
                ArticyManager.instance.rollback = enable;
            }

            public void Dispose() {
                ArticyManager.instance.rollback = _rollbackEnabled;
            }
        }

        [SerializeField] private ArticyVariablesManager m_VariablesManager;

        private SerializedDictionary<string, int> _defaultNumberVariables;
        private SerializedDictionary<string, string> _defaultStringVariables;

        private HashSet<ulong> _references;
        private Dictionary<string, object> _variables;

        public SerializedDictionary<string, int> defaultNumberVariables => _defaultNumberVariables;
        public SerializedDictionary<string, string> defaultStringVariables => _defaultStringVariables;

        private static NotificationManager _notifications;
        public static NotificationManager notifications => _notifications;

        public bool rollback { get; set; } = true;
        public bool inSacrifice { get; set; } = false;

        public bool notify { get; set; } = true;

        public bool isRollbackVariable { get; private set; } = false;

        protected override void Awake() {
            base.Awake();
            _notifications = new NotificationManager();
            ArticyVariablesManager.GetVariablesDictionaries(out _defaultNumberVariables, out _defaultStringVariables);

            DataManager.instance.afterDeserializeGameData.AddListener(ReloadVariables);
            DataManager.instance.beforeSerializeGameData.AddListener(SerializeReferences);

            ArticyVariables.globalVariables.Notifications.AddListener("*.*", VariableChanged);

            notifications.AddListener("gameState.rollbackVariables", (_, rollbackV) => rollback = (bool)rollbackV);
            notifications.AddListener("items.*", ItemVariableChanged);
        }

        private void SerializeReferences(GameData data) {
            data.articyVariablesDialoguesLock = new List<ulong>(_references);
            m_VariablesManager.SerializeArticyVariablesToGameData(data);
            GameLogger.articy.Log("Serialize variables", LogLevel.FunctionScope);
        }

        private void ReloadVariables(GameData gameData) {
            _references = new HashSet<ulong>(gameData.articyVariablesDialoguesLock);
            m_VariablesManager.DeserializeGameDataToArticyVariables(gameData);
            _variables = new Dictionary<string, object>(ArticyVariables.globalVariables.Variables);
            GameManager.instance.ReloadSpammyInParty(ArticyVariables.globalVariables.gameState.spamInParty);
            GameLogger.articy.Log("Reset variables", LogLevel.FunctionScope);
        }

        public void SetVariable(string variable, object value) {
            using ToggleRollbackScope scope = new ToggleRollbackScope(false);
            ArticyVariables.globalVariables.SetVariableByString(variable, value);
            VariableChanged(variable, value);
        }

        private void VariableChanged(string name, object value) {
            if (isRollbackVariable) {
                isRollbackVariable = false;
                return;
            }

            ulong id = 0UL;
            if (DialogueManager.instance) {
                var objRef = DialogueManager.instance.executionEngine.flowPlayer.CurrentObject;
                if (objRef != null && objRef.HasReference) id = objRef.id;
            }

            if (id != 0UL && rollback && ArticyVariables.globalVariables.IsVariableOfTypeInteger(name) && _references.Contains(id)) {
                isRollbackVariable = true;
                ArticyVariables.globalVariables.SetVariableByString(name, _variables[name]);
                GameLogger.articy.Log($"Rollback variable {name} at obj {id} from {value} to to value {_variables[name]}", LogLevel.Verbose);
            } else {
                if (ArticyVariables.globalVariables.IsVariableOfTypeInteger(name) && id != 0UL && rollback) {
                    _references.Add(id);
                    _variables[name] = value;
                    GameLogger.articy.Log($"Add rollback variable {name} at obj {id} with value {value}", LogLevel.Verbose);
                }
                GameLogger.articy.Log($"Variable {name} at obj {id} changed to {value}", LogLevel.FunctionScope);
                if (notify)
                    notifications.NotifyPropertyChanged(name, value);
            }
        }

        private void ItemVariableChanged(string name, object value) {
            if (!InventoryManager.instance) return;

            if (value is not int iValue) {
                GameLogger.articy.LogWarning("Item variable isn`t a int value");
                return;
            }

            foreach (var item in InventoryDatabase.instance.data.Values) {
                if (item.articyVariable == name) {
                    if (iValue == 1) {
                        InventoryManager.instance.AddItem(item);
                        if (item.achievement)
                            AchievementsManager.instance.UnlockAchievement(item.achievement);
                    } else {
                        InventoryManager.instance.RemoveItem(item);
                    }
                    return;
                }
                continue;
            }
        }
    }

    public sealed class NotificationManager {
        private sealed class WeakDelegate {
            private static readonly MethodInfo s_GetMethodInfoMethod;

            private static readonly MethodInfo s_CreateDelegateMethod;

            private static readonly Type s_ActionType;

            private static readonly Func<Delegate, MethodInfo> s_GetMethodInfoFunc;

            private static readonly Func<object, MethodInfo, Action<string, object>> s_CreateDelegateFunc;

            private readonly WeakReference _reference;

            private readonly Action<string, object> _method;

            private readonly MethodInfo _methodInfo;

            public bool isAlive => _reference == null || _reference.IsAlive;

            public WeakReference reference => _reference;

            static WeakDelegate() {
                s_ActionType = typeof(Action<string, object>);
                try {
                    Type type = Type.GetType("System.Reflection.RuntimeReflectionExtensions", throwOnError: false);
                    Type typeFromHandle = typeof(MethodInfo);
                    if (type is not null) {
                        s_GetMethodInfoMethod = type.GetMethod("GetMethodInfo", new Type[1] { typeof(Delegate) });
                    }

                    s_CreateDelegateMethod = typeFromHandle.GetMethod("CreateDelegate", new Type[2] {
                        typeof(Type),
                        typeof(object)
                    });
                    if (type is not null && s_CreateDelegateMethod is not null && s_GetMethodInfoMethod is not null) {
                        s_GetMethodInfoFunc = GetMethodInfoUwp;
                        s_CreateDelegateFunc = CreateDelegateUwp;
                    } else {
                        s_GetMethodInfoFunc = GetMethodInfoNormal;
                        s_CreateDelegateFunc = CreateDelegateNormal;
                    }
                } catch (Exception ex) {
                    GameLogger.LogWarning($"Failed to detect UWP usage. Exception\n{ex}");
                }
            }

            public override int GetHashCode() {
                if (_reference == null) {
                    return _method.GetHashCode();
                }

                return _methodInfo.GetHashCode() ^ (_reference.IsAlive ? _reference.Target.GetHashCode() : _reference.GetHashCode());
            }

            public override bool Equals(object other) {
                if (other is not WeakDelegate weakDelegate) {
                    return false;
                }

                return GetHashCode() == weakDelegate.GetHashCode();
            }

            public WeakDelegate(Action<string, object> strongReferenceAction) {
                object target = strongReferenceAction.Target;
                if (target != null) {
                    _reference = new WeakReference(target);
                    _methodInfo = GetMethodInfo(strongReferenceAction);
                } else {
                    _reference = null;
                    _method = strongReferenceAction;
                }
            }

            public bool Invoke(string variable, object value) {
                object obj = null;
                if (_reference != null) {
                    obj = _reference.Target;
                }

                if (!isAlive) {
                    return false;
                }

                if (obj != null) {
                    CreateDelegate(obj, _methodInfo)(variable, value);
                } else {
                    _method.DynamicInvoke(variable, value);
                }

                return true;
            }

            private static Action<string, object> CreateDelegate(object targetObject, MethodInfo method) {
                return s_CreateDelegateFunc(targetObject, method);
            }

            private static MethodInfo GetMethodInfo(Action<string, object> action) {
                return s_GetMethodInfoFunc(action);
            }

            private static Action<string, object> CreateDelegateNormal(object targetObject, MethodInfo method) {
                return (Action<string, object>)Delegate.CreateDelegate(s_ActionType, targetObject, method);
            }

            private static MethodInfo GetMethodInfoNormal(Delegate action) {
                return action.Method;
            }

            private static Action<string, object> CreateDelegateUwp(object targetObject, MethodInfo method) {
                return s_CreateDelegateMethod.Invoke(method, new object[2] { s_ActionType, targetObject }) as Action<string, object>;
            }

            private static MethodInfo GetMethodInfoUwp(Delegate action) {
                return s_GetMethodInfoMethod.Invoke(null, new object[1] { action }) as MethodInfo;
            }
        }

        private readonly Dictionary<string, HashSet<WeakDelegate>> _notificationMap = new Dictionary<string, HashSet<WeakDelegate>>();

        private readonly Type[] _removeAllBlackListedTypes = new Type[1] { typeof(ArticyText) };

        internal NotificationManager() { }

        public void AddListener(string variable, Action<string, object> notificationFunc) {
            WeakDelegate aWeakDelegate = new WeakDelegate(notificationFunc);
            bool flag = true;
            if (variable.Contains("?") || variable.Contains("*")) {
                Regex regex = StringUtils.CreateRegexFromWildcard(variable);
                foreach (string variableName in BaseGlobalVariables.VariableNames) {
                    if (regex.IsMatch(variableName)) {
                        AddRawListener(variableName, aWeakDelegate);
                        flag = false;
                    }
                }
            } else if (BaseGlobalVariables.VariableNames.Contains(variable)) {
                AddRawListener(variable, aWeakDelegate);
                flag = false;
            }

            if (flag) {
                GameLogger.LogWarning($"AddListener() didn't found any variables using \"{variable}\" as supplied name or search pattern.");
            }
        }

        public void RemoveListener(string variable, Action<string, object> notificationFunc) {
            if (variable.Contains("?") || variable.Contains("*")) {
                Regex regex = StringUtils.CreateRegexFromWildcard(variable);
                foreach (string variableName in BaseGlobalVariables.VariableNames) {
                    if (regex.IsMatch(variableName)) {
                        RemoveRawListener(variableName, notificationFunc);
                    }
                }
            } else {
                RemoveRawListener(variable, notificationFunc);
            }
        }

        private void AddRawListener(string variable, WeakDelegate weakDelegate) {
            if (!_notificationMap.TryGetValue(variable, out var value)) {
                value = new HashSet<WeakDelegate>();
                _notificationMap[variable] = value;
            }

            value.Add(weakDelegate);
        }

        private void RemoveRawListener(string variable, Action<string, object> notificationFunc) {
            if (_notificationMap.TryGetValue(variable, out var value)) {
                value.Remove(new WeakDelegate(notificationFunc));
            }
        }

        private void RemoveRawListener(WeakDelegate weakDelegate) {
            foreach (KeyValuePair<string, HashSet<WeakDelegate>> item in _notificationMap) {
                item.Value.Remove(weakDelegate);
            }
        }

        public void RemoveAllListeners() {
            foreach (KeyValuePair<string, HashSet<WeakDelegate>> item in _notificationMap) {
                HashSet<WeakDelegate> value = item.Value;
                foreach (WeakDelegate item2 in new HashSet<WeakDelegate>(value)) {
                    Type type = null;
                    if (item2.reference != null && item2.reference.Target != null) {
                        type = item2.reference.Target.GetType();
                    }

                    if (type is null || !_removeAllBlackListedTypes.Contains(type)) {
                        value.Remove(item2);
                    }
                }
            }

            foreach (KeyValuePair<string, HashSet<WeakDelegate>> item3 in new Dictionary<string, HashSet<WeakDelegate>>(_notificationMap)) {
                if (item3.Value.Count == 0) {
                    _notificationMap.Remove(item3.Key);
                }
            }
        }

        public void NotifyPropertyChanged(string variable, object varValue) {
            if (!_notificationMap.TryGetValue(variable, out var value)) {
                return;
            }

            HashSet<WeakDelegate> hashSet = new HashSet<WeakDelegate>();
            hashSet.UnionWith(value);
            foreach (WeakDelegate item in hashSet) {
                if (!item.Invoke(variable, varValue)) {
                    RemoveRawListener(item);
                }
            }
        }
    }
}
