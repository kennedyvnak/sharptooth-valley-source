using UnityEngine;

namespace NFHGame {
    public static class PlatformManager {
        public enum Platform { Invalid = -1, Windows, WebGL, MacOS, Editor = 1 << 8 }

        public static Platform currentPlatform { get; private set; }

        public static void Initialize() {
            switch (Application.platform) {
                case RuntimePlatform.OSXPlayer:
                    currentPlatform = Platform.MacOS;
                    break;
                case RuntimePlatform.WebGLPlayer:
                    currentPlatform = Platform.WebGL;
                    break;
                case RuntimePlatform.WindowsPlayer:
                    currentPlatform = Platform.Windows;
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    currentPlatform = Platform.Editor;
                    break;
                default:
                    GameLogger.LogError("Not Expected Platform!");
                    currentPlatform = Platform.Invalid;
                    break;
            }
        }
    }
}
