using NFHGame.Options;
using NFHGame.RangedValues;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NFHGame.PostProcessing {
    [RequireComponent(typeof(Volume))]
    public class GammaController : Singleton<GammaController> {
        [SerializeField, RangedValue(-2.0f, 2.0f)] private RangedFloat m_GammaRange = new RangedFloat(-0.2f, 0.2f);

        public RangedFloat gammaRange { get => m_GammaRange; set => SetRange(value); }

        private Volume _volume;

        protected override void Awake() {
            base.Awake();
            _volume = GetComponent<Volume>();
        }

        private void Start() {
            SetValue(OptionsManager.instance.currentOptions.GetFloat("gamma"));
            OptionsManager.instance.onFloatOptionChanged.AddListener(OPTION_FloatChanged);
        }

        protected override void OnDestroy() {
            OptionsManager.instance.onFloatOptionChanged.RemoveListener(OPTION_FloatChanged);
            base.OnDestroy();
        }

        public void SetValue(float normalized) {
            var value = Mathf.Lerp(m_GammaRange.min, m_GammaRange.max, normalized);
            if (_volume.profile.TryGet<LiftGammaGain>(out var gammaEffect)) {
                var gamma = gammaEffect.gamma.value;
                gamma.w = value;
                gammaEffect.gamma.value = gamma;
            }
        }

        private void SetRange(RangedFloat range) {
            m_GammaRange = range;
            SetValue(OptionsManager.instance.currentOptions.GetFloat("gamma"));
        }

        private void OPTION_FloatChanged(string key, float value) {
            if (key == "gamma")
                SetValue(value);
        }
    }
}