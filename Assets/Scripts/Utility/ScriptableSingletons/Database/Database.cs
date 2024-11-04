using NFHGame.ScriptableSingletons;
using UnityEngine;

namespace NFHGame.Data {
    public abstract class Database<TDatabase, TData> : ScriptableSingleton<TDatabase>
        where TDatabase : Database<TDatabase, TData> {
        [SerializeField] private SerializedDictionary<string, TData> m_Data;
        public SerializedDictionary<string, TData> data => m_Data;

        public TData this[string key] {
            get => m_Data[key];
            set => m_Data[key] = value;
        }

        public TData GetData(string key) => data[key];

        public bool TryGetData(string key, out TData data) {
            return this.data.TryGetValue(key, out data);
        }

        public string GetKeyForData(TData data, System.Collections.Generic.EqualityComparer<TData> comparer = null) {
            comparer ??= System.Collections.Generic.EqualityComparer<TData>.Default;

            foreach (var kvp in m_Data) {
                if (comparer.Equals(kvp.Value, data))
                    return kvp.Key;
            }

            return null;
        }

        public string GetKeyForData(System.Predicate<TData> predicate) {
            foreach (var kvp in m_Data) {
                if (predicate(kvp.Value))
                    return kvp.Key;
            }

            return null;
        }
    }
}
