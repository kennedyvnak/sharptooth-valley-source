using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NFHGame {
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver {
        // Internal
        [SerializeField] private List<KeyValuePair> m_Data = new List<KeyValuePair>();
        private readonly Dictionary<TKey, int> m_IndexByKey = new Dictionary<TKey, int>();
        private readonly Dictionary<TKey, TValue> m_Dict = new Dictionary<TKey, TValue>();

#pragma warning disable 0414
        [SerializeField, HideInInspector]
        private bool _keyCollision;
#pragma warning restore 0414

        // Serializable KeyValuePair struct
        [Serializable]
        struct KeyValuePair {
            public TKey key;
            public TValue value;

            public KeyValuePair(TKey key, TValue value) {
                this.key = key;
                this.value = value;
            }
        }

        // Since lists can be serialized natively by unity no custom implementation is needed
        public void OnBeforeSerialize() { }

        // Fill dictionary with list pairs and flag key-collisions.
        public void OnAfterDeserialize() {
            m_Dict.Clear();
            m_IndexByKey.Clear();
            _keyCollision = false;

            for (int i = 0; i < m_Data.Count; i++) {
                var key = m_Data[i].key;
                if (key != null && !ContainsKey(key)) {
                    m_Dict.Add(key, m_Data[i].value);
                    m_IndexByKey.Add(key, i);
                } else {
                    _keyCollision = true;
                }
            }
        }

        // IDictionary
        public TValue this[TKey key] {
            get => m_Dict[key];
            set {
                m_Dict[key] = value;

                if (m_IndexByKey.ContainsKey(key)) {
                    var index = m_IndexByKey[key];
                    m_Data[index] = new KeyValuePair(key, value);
                } else {
                    m_Data.Add(new KeyValuePair(key, value));
                    m_IndexByKey.Add(key, m_Data.Count - 1);
                }
            }
        }

        public ICollection<TKey> Keys => m_Dict.Keys;
        public ICollection<TValue> Values => m_Dict.Values;

        public void Add(TKey key, TValue value) {
            m_Dict.Add(key, value);
            m_Data.Add(new KeyValuePair(key, value));
            m_IndexByKey.Add(key, m_Data.Count - 1);
        }

        public bool ContainsKey(TKey key) => m_Dict.ContainsKey(key);

        public bool Remove(TKey key) {
            if (m_Dict.Remove(key)) {
                var index = m_IndexByKey[key];
                m_Data.RemoveAt(index);
                UpdateIndexes(index);
                m_IndexByKey.Remove(key);
                return true;
            } else {
                return false;
            }
        }

        void UpdateIndexes(int removedIndex) {
            for (int i = removedIndex; i < m_Data.Count; i++) {
                var key = m_Data[i].key;
                m_IndexByKey[key]--;
            }
        }

        public bool TryGetValue(TKey key, out TValue value) => m_Dict.TryGetValue(key, out value);

        // ICollection
        public int Count => m_Dict.Count;
        public bool IsReadOnly { get; set; }

        public void Add(KeyValuePair<TKey, TValue> pair) {
            Add(pair.Key, pair.Value);
        }

        public void Clear() {
            m_Dict.Clear();
            m_Data.Clear();
            m_IndexByKey.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair) {
            if (m_Dict.TryGetValue(pair.Key, out TValue value)) {
                return EqualityComparer<TValue>.Default.Equals(value, pair.Value);
            } else {
                return false;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (array == null)
                throw new ArgumentException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (array.Length - arrayIndex < m_Dict.Count)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            foreach (var pair in m_Dict) {
                array[arrayIndex] = pair;
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> pair) {
            if (m_Dict.TryGetValue(pair.Key, out TValue value)) {
                bool valueMatch = EqualityComparer<TValue>.Default.Equals(value, pair.Value);
                if (valueMatch) {
                    return Remove(pair.Key);
                }
            }
            return false;
        }

        // IEnumerable
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_Dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_Dict.GetEnumerator();
    }
}