namespace NFHGame {
    public enum LogLevel : byte {
        Default = 0,
        Verbose = 1,
        FunctionScope = 2,
        None = 255,
    }

    [System.Serializable]
    public class LogModule {
        public string moduleName;
        public bool overrideLog = false;
        public LogLevel currentLogLevel;

        public void Log(string message) {
            GameLogger.ModuleLog(this, message);
        }

        public void LogWarning(string message) {
            GameLogger.ModuleLogWarning(this, message);
        }

        public void LogError(string message) {
            GameLogger.ModuleLogError(this, message);
        }

        public void Log(string message, LogLevel level) {
            GameLogger.ModuleLog(this, message, level);
        }

        public void LogWarning(string message, LogLevel level) {
            GameLogger.ModuleLogWarning(this, message, level);
        }

        public void LogError(string message, LogLevel level) {
            GameLogger.ModuleLogError(this, message, level);
        }
    }
}
