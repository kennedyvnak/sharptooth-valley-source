using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.UI {
    [RequireComponent(typeof(Slider))]
    public class SliderController : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_ValueText;

        public Slider slider { get; private set; }

        private void Awake() {
            slider = GetComponent<Slider>();
        }

        private void Start() {
            slider.onValueChanged.AddListener(EVENT_ValueChanged);
        }

        public void RefreshSlider() {
            m_ValueText.text = $"{Mathf.RoundToInt(slider.normalizedValue * 100.0f)}%";
        }

        private void EVENT_ValueChanged(float value) {
            RefreshSlider();
        }
    }
}
