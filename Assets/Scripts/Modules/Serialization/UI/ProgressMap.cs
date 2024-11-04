using UnityEngine;
using System;
using System.Linq;

namespace NFHGame.Serialization {
    public class ProgressMap : MonoBehaviour {
        [Serializable]
        public class MapField {
            public GameObject[] displays;
            public Expression expression;
        }

        [SerializeField] private MapField[] m_Fields;
        [SerializeField] private GameObject m_Overlay;
        
        /*[Serializable]
        public class OverrideExpresion {
            public string key;
            public bool value;
        }

        public OverrideExpresion[] m_Overrides;

        private void Start() {
            var conds = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < m_Fields.Length; i++) {
                MapField field = m_Fields[i];
                foreach (var cond in field.expression.conditions) {
                    conds.Add(cond.text);
                }
            }
            m_Overrides = new OverrideExpresion[conds.Count];
            var condsArr = conds.ToArray();
            for (int i = 0; i < conds.Count; i++) {
                m_Overrides[i] = new OverrideExpresion() { key = condsArr[i], value = false };
            }
        }

        private void Update() {
            foreach (var field in m_Fields) {
                foreach (var display in field.displays) {
                    display.SetActive(field.expression.Get(m_Overrides));
                }
            }
        }*/

        public void SetData(GameData data) {
            foreach (var field in m_Fields) {
                var fieldEnabled = field.expression.Get(data);
                foreach (var display in field.displays) {
                    display.SetActive(fieldEnabled);
                }
            }
            m_Overlay.SetActive(true);
        }

        public void ResetData() {
            m_Overlay.SetActive(false);
        }
    }
}
