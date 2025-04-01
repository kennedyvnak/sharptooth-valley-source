using NFHGame.Configs;
using NFHGame.ScriptableSingletons;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NFHGame {
    public class GameLogger : ScriptableSingleton<GameLogger>, IBootableSingleton {
        [System.Serializable]
        public struct LogContext {
            public long time;
            public string log, stack;
            public byte type, level;

            public LogContext(long time, string log, string stack, LogType type, LogLevel level) {
                this.time = time;
                this.log = log;
                this.stack = stack;
                this.type = (byte)type;
                this.level = (byte)level;
            }

            public override string ToString() {
                if (level == (byte)LogLevel.None)
                    return $"[{System.DateTime.FromBinary(time):H:mm:ss}, {(LogType)type}]: {log}{GetStackText()}";
                else
                    return $"[{System.DateTime.FromBinary(time):H:mm:ss}, {(LogLevel)level} {(LogType)type}]: {log}{GetStackText()}";

            }

            private string GetStackText() {
                return string.IsNullOrEmpty(stack) ? string.Empty : $"\n{stack}";
            }
        }

        public LogLevel currentLogLevel = LogLevel.FunctionScope;

        [SerializeField] private string m_GameVersion;
        [SerializeField] private bool m_StoreLogs = false;

        [SerializeField] private LogModule m_Configs;
        [SerializeField] private LogModule m_Input;
        [SerializeField] private LogModule m_Data;
        [SerializeField] private LogModule m_State;
        [SerializeField] private LogModule m_Loader;
        [SerializeField] private LogModule m_GameKeys;
        [SerializeField] private LogModule m_Achievements;
        [SerializeField] private LogModule m_Dialogue;
        [SerializeField] private LogModule m_Articy;

        public string currentGameVersion => m_GameVersion;

        public static bool storeLogs { get => instance.m_StoreLogs; set => instance.m_StoreLogs = value; }
        public static List<LogContext> logs;

        public static LogModule configs => instance.m_Configs;
        public static LogModule input => instance.m_Input;
        public static LogModule data => instance.m_Data;
        public static LogModule state => instance.m_State;
        public static LogModule loader => instance.m_Loader;
        public static LogModule gameKeys => instance.m_GameKeys;
        public static LogModule achievements => instance.m_Achievements;
        public static LogModule dialogue => instance.m_Dialogue;
        public static LogModule articy => instance.m_Articy;

        public static string gameVersion => instance.m_GameVersion;

        public static void ModuleLog(LogModule context, string log) {
            Log($"[{context.moduleName}] {log}");
        }

        public static void ModuleLogWarning(LogModule context, string log) {
            LogWarning($"[{context.moduleName}] {log}");
        }

        public static void ModuleLogError(LogModule context, string log) {
            LogError($"[{context.moduleName}] {log}");
        }

        public static void ModuleLog(LogModule context, string log, LogLevel level) {
            if (InLevel(context, level))
                Log($"[{context.moduleName}] {log}");
        }

        public static void ModuleLogWarning(LogModule context, string log, LogLevel level) {
            if (InLevel(context, level))
                LogWarning($"[{context.moduleName}] {log}");
        }

        public static void ModuleLogError(LogModule context, string log, LogLevel level) {
            if (InLevel(context, level))
                LogError($"[{context.moduleName}] {log}");
        }

        public static void Log(string log) {
            Debug.Log(log);
        }

        public static void LogWarning(string log) {
            Debug.LogWarning(log);
        }

        public static void LogError(string log) {
            Debug.LogError(log);
        }

        public static void Log(string log, Object context) {
            Debug.Log(log, context);
        }

        public static void LogWarning(string log, Object context) {
            Debug.LogWarning(log, context);
        }

        public static void LogError(string log, Object context) {
            Debug.LogError(log, context);
        }

        public static void Log(string log, LogLevel level) {
            if (InLevel(level)) Debug.Log(log);
            else if (storeLogs) logs.Add(new LogContext(System.DateTime.Now.ToBinary(), log, null, LogType.Log, level));
        }

        public static void LogWarning(string log, LogLevel level) {
            if (InLevel(level)) Debug.LogWarning(log);
            else if (storeLogs) logs.Add(new LogContext(System.DateTime.Now.ToBinary(), log, null, LogType.Warning, level));
        }

        public static void LogError(string log, LogLevel level) {
            if (InLevel(level)) Debug.LogError(log);
            else if (storeLogs) logs.Add(new LogContext(System.DateTime.Now.ToBinary(), log, null, LogType.Error, level));
        }

        public static bool InLevel(LogLevel level) => InLevel(instance.currentLogLevel, level);

        public static bool InLevel(LogModule module, LogLevel level) => module.overrideLog ? InLevel(module.currentLogLevel, level) : InLevel(level);

        public static bool InLevel(LogLevel current, LogLevel level) => level <= current;

        public void Initialize() {
            if (storeLogs) logs = new List<LogContext>();
            Application.logMessageReceived += EVENT_LogReceived;

            PlatformManager.Initialize();
            configs.Log($"Running version {gameVersion} in {PlatformManager.currentPlatform}");
        }

        private void EVENT_LogReceived(string condition, string stackTrace, LogType type) {
            if (storeLogs) logs.Add(new LogContext(System.DateTime.Now.ToBinary(), condition, stackTrace, type, LogLevel.None));
        }
    }
}
