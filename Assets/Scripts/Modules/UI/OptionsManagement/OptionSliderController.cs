using NFHGame.UI;
using UnityEngine;

namespace NFHGame.Options {
    [RequireComponent(typeof(SliderController))]
    public class OptionSliderController : MonoBehaviour {
        [SerializeField] private string m_TargetKey;

        public SliderController sliderController { get; private set; }

        private void Awake() {
            sliderController = GetComponent<SliderController>();
        }

        private void Start() {
            if (OptionsManager.instance.currentOptions.TryGetFloat(m_TargetKey, out float value))
                SetValue(value);

            sliderController.slider.onValueChanged.AddListener((v) => OptionsManager.instance.SetFloat(m_TargetKey, v));

            OptionsManager.instance.onFloatOptionChanged.AddListener(EVENT_OptionChanged);
        }

        private void OnDestroy() {
            OptionsManager.instance.onFloatOptionChanged.RemoveListener(EVENT_OptionChanged);
        }

        private void SetValue(float value) {
            sliderController.slider.SetValueWithoutNotify(value);
            sliderController.RefreshSlider();
        }

        private void EVENT_OptionChanged(string key, float value) {
            if (key.Equals(m_TargetKey))
                SetValue(value);
        }
    }
}