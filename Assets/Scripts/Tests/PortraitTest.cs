using System.Linq;
using NFHGame.DialogueSystem.Actors;
using NFHGame.DialogueSystem.DialogueBoxes;
using NFHGame.DialogueSystem.Portraits;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGameTests {
    public class PortraitTest : MonoBehaviour {
        [SerializeField] private string[] m_PartsNames;
        [SerializeField] private Slider m_SliderController;
        [SerializeField] private PortraitDisplay m_Display;
        [SerializeField] private DialogueActor.Actor m_Actor;
        [SerializeField, Range(-1, 1)] private int m_Direction;
        [SerializeField] private PortraitCollection m_Portrait;
        [SerializeField] private TMP_Dropdown m_PrefabsDropdown;
        [SerializeField] private RectTransform m_SlidersParent;

        private int[] _idx;

        private void Start() {
            int partsLength = m_Portrait.partsCollection.Length;
            _idx = new int[partsLength];

            for (int i = 0; i < partsLength; i++) {
                var slider = Instantiate(m_SliderController.transform.parent, m_SlidersParent);
                var controller = slider.GetChild(1).GetComponent<Slider>();
                controller.maxValue = m_Portrait.partsCollection[i].parts.Count - 1;
                controller.transform.parent.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{m_PartsNames[i]} ({i})";
                var valueText = controller.transform.parent.GetChild(2).GetComponent<TextMeshProUGUI>();
                int pos = i;
                controller.onValueChanged.AddListener((f) => {
                    valueText.text = ((int)f).ToString();
                    _idx[pos] = (int)f;
                    UpdatePortrait();
                });

                slider.gameObject.SetActive(true);
            }

            UpdatePortrait();

            m_PrefabsDropdown.ClearOptions();
            m_PrefabsDropdown.AddOptions(m_Portrait.prefabs.Keys.ToList());
            m_PrefabsDropdown.onValueChanged.AddListener(OnSetPrefab);
        }

        private void OnSetPrefab(int index) {
            var prefab = m_Portrait.prefabs.Values.ElementAt(index);
            for (int i = 0; i < prefab.indexes.Length; i++) {
                int partIdx = prefab.indexes[i];
                _idx[i] = partIdx;
                m_SlidersParent.GetChild(i + 1).GetChild(1).GetComponent<Slider>().SetValueWithoutNotify(partIdx);
            }
            UpdatePortrait();
        }

        private void UpdatePortrait() {
            m_Display.SetPortrait(m_Portrait.GetFromIndexes(_idx, m_Actor), m_Direction);
        }
    }
}