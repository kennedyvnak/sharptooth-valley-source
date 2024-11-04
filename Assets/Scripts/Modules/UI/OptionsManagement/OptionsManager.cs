using NFHGame.Configs;
using NFHGame.ScriptableSingletons;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.Options {
    public class OptionsManager : ScriptableSingleton<OptionsManager>, IBootableSingleton {
        [System.Serializable]
        public class Options {
            public SerializedDictionary<string, float> floatOptions = new SerializedDictionary<string, float>() {
                { "gamma", 0.5f },
                { "masterVolume", 1.0f },
                { "musicsVolume", 1.0f },
                { "soundsVolume", 1.0f }
            };

            public float GetFloat(string key) => floatOptions[key];
            public bool TryGetFloat(string key, out float value) => floatOptions.TryGetValue(key, out value);
        }

        [SerializeField] private Options m_DefaultOptions;

        [Space]
        [SerializeField] private UnityEvent<string, float> m_OnFloatOptionChanged;

        public Options currentOptions { get; private set; }
        public UnityEvent<string, float> onFloatOptionChanged => m_OnFloatOptionChanged;

        public void Initialize() {
            currentOptions = new Options();
            foreach (var floatOption in m_DefaultOptions.floatOptions) {
                currentOptions.floatOptions[floatOption.Key] = GetPreferredFloat(floatOption.Key, floatOption.Value);
            }
        }

        public void SetFloat(string key, float value) {
            currentOptions.floatOptions[key] = value;
            PlayerPrefs.SetFloat(key, value);
            SendFloatEvent(key, value);
        }

        public void SendFloatEvent(string key, float value) => m_OnFloatOptionChanged?.Invoke(key, value);

        public void ResetOptions() {
            foreach (var floatOption in m_DefaultOptions.floatOptions) {
                currentOptions.floatOptions[floatOption.Key] = floatOption.Value;
                PlayerPrefs.DeleteKey(floatOption.Key);
                SendFloatEvent(floatOption.Key, floatOption.Value);
            }
        }

        private float GetPreferredFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);
    }
}