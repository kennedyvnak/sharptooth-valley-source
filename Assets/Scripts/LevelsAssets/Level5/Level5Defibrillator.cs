using DG.Tweening;
using NFHGame.Animations;
using NFHGame.External;
using NFHGame.RangedValues;
using NFHGame.UI.Input;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.LevelAssets.Level5 {
    public class Level5Defibrillator : Singleton<Level5Defibrillator> {
        private static readonly int k_BastLineColorID = Shader.PropertyToID("_HDRColor");

        [Serializable]
        public class Defibri {
            [Serializable]
            public class Module {
                public bool use;
                public float factor;
                public float desired;
                public float steps;
                public int index;
                public int start;
            }

            public int points;
            public int amplitudeFrames;
            public Module speed, amplitude, frequency;

            public bool useWave;
            public int desiredWave;
            public int startWave;

            public float lifeTimerRegen;
        }

        [Header("Lines")]
        [SerializeField] private NoiseLineDrawer m_DragonNoise;
        [SerializeField] private NoiseLineDrawer m_BastNoise;
        [SerializeField, ColorUsage(true, true)] private Color m_BastLineColorStart, m_BastLineColorFinal;
        [SerializeField] private AnimationCurve m_BastLineColorCurve;
        [SerializeField, RangedValue(0.0f, 1.0f)] private RangedFloat m_BastLineSizeProgress;

        [Header("Defibris")]
        [SerializeField] private float m_DefibriStartDelay;
        [SerializeField] private float m_LifeDrain;
        [SerializeField] private Defibri[] m_Defibris;

        [Header("UI")]
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private RadialSlider m_SpeedSlider, m_AmplitudeSlider, m_FrequencySlider;
        [SerializeField] private SlicedFilledImage m_LifeTimerImage;
        [SerializeField] private SpriteArrayAnimator m_AmplitudeCurveAnim;

        [Header("Amplitude Slider")]
        [SerializeField] private Sprite m_AmplitudeEmptyState;
        [SerializeField] private Sprite[] m_AmplitudeStates;
        [SerializeField] SpriteArrayAnimator m_AmplitudeAnimatorA;
        [SerializeField] SpriteArrayAnimator m_AmplitudeAnimatorB;

        [Header("Frequency Slider")]
        [SerializeField] private RectTransform m_FrequencyIcon;
        [SerializeField] private RangedFloat m_FrequencyIconRange;
        [SerializeField] private float m_FrequencyIconDelta;

        [Header("Speed Slider")]
        [SerializeField] private SpriteArrayAnimator m_SpeedSliderIconAnim;
        [SerializeField] private RangedFloat m_SpeedSliderRange;

        [Header("Wave Button")]
        [SerializeField] private Button m_WaveButton;
        [SerializeField] private GameObject[] m_WaveButtonInactiveObjects;
        [SerializeField] private Image m_WaveButtonBackground;
        [SerializeField] private Color m_WaveButtonBackgroundActiveColor, m_WaveButtonBackgroundDeactiveColor;
        [SerializeField] private Image m_WaveFaceImage;
        [SerializeField] private Sprite[] m_WaveFaceSprites;
        [SerializeField] private GameObject[] m_WaveIconObjects;

        [Header("Anim")]
        [SerializeField] private float m_FadeDuration;
        [SerializeField] private float m_UIFadeDuration;
        [SerializeField] private float m_TutorialFadeDuration;
        [SerializeField] private CanvasGroup[] m_Tutorial;

        private float _currentLife = 1.0f;
        private int _defibriIndex;
        private System.Action<bool> _onEnd;
        private System.Action<bool> _onTutorialEnd;
        private bool _defibring;
        private float _frequencyDelta;
        private bool _listeningAmplitude;

        private void Start() {
            m_SpeedSlider.normalizedValueChanged.AddListener(EVENT_SpeedChanged);
            m_FrequencySlider.normalizedValueChanged.AddListener(EVENT_FrequencyChanged);
            m_AmplitudeSlider.normalizedValueChanged.AddListener(EVENT_AmplitudeChanged);
            m_WaveButton.onClick.AddListener(EVENT_WaveClicked);

            transform.SetSiblingIndex(UserInterfaceInput.instance.transform.GetSiblingIndex() + 1);
        }

        private void Update() {
            if (_defibring) {
                _currentLife -= m_LifeDrain * Time.deltaTime;
                m_LifeTimerImage.fillAmount = _currentLife;
                if (_currentLife <= 0.0f) {
                    _defibring = false;
                    SetInteraction(false);
                    FadeOut(() => {
                        _onEnd?.Invoke(false);
                    });
                }
                _frequencyDelta += Time.deltaTime;
                if (_frequencyDelta >= m_FrequencyIconDelta) {
                    _frequencyDelta = 0.0f;
                    m_FrequencyIcon.anchoredPosition = new Vector2(m_FrequencyIconRange.RandomRange(), 0.0f);
                }
            }
        }

        public void ScheduleDifibri(System.Action<bool> onEnd) {
            _defibriIndex = 0;
            _onEnd = null;

            foreach(var tutorial in m_Tutorial) {
                tutorial.DOKill();
                tutorial.gameObject.SetActive(false);
            }

            _onTutorialEnd = onEnd;
            _currentLife = 1.0f;
            m_LifeTimerImage.fillAmount = _currentLife;
            m_CanvasGroup.gameObject.SetActive(true);
            m_CanvasGroup.ToggleGroupAnimated(true, m_UIFadeDuration);

            SetDefibri(m_Defibris[0], false);
            PlayTutorial(0);
        }

        public void EndDefibri() {
            m_CanvasGroup.ToggleGroupAnimated(false, m_UIFadeDuration).onComplete += () => {
                m_CanvasGroup.gameObject.SetActive(false);
            };
        }

        public void PlayDefibri(int defibri, System.Action<bool> onEnd) {
            _defibriIndex = defibri;
            _onEnd = onEnd;
            SetDefibri(m_Defibris[defibri], true);
            if (defibri == 1)
                PlayTutorial(defibri);
        }

        public void WaveIconAnimLoopFinished() {
            for (int i = 0; i < m_WaveIconObjects.Length; i++) {
                m_WaveIconObjects[i].SetActive(i == m_BastNoise.wave);
            }
        }

        private void FadeIn(System.Action finishFade) {
            var points = m_BastNoise.points;
            var speed = m_BastNoise.speed;
            m_BastNoise.points = 0;
            m_BastNoise.speed = 0.0f;
            DOVirtual.Float(0.0f, 1.0f, m_FadeDuration, (x) => {
                SetLineAlpha(m_BastNoise.renderer, x);
                SetLineAlpha(m_DragonNoise.renderer, x);
                m_BastNoise.points = Mathf.Clamp(Mathf.CeilToInt(x * points), 2, points);
            }).OnComplete(() => {
                m_BastNoise.points = points;
                m_BastNoise.speed = speed;
                finishFade();
            });
        }

        private void FadeOut(System.Action finishFade) {
            DOVirtual.Float(1.0f, 0.0f, m_FadeDuration, (x) => {
                SetLineAlpha(m_BastNoise.renderer, x);
                SetLineAlpha(m_DragonNoise.renderer, x);
            }).OnComplete(finishFade.Invoke);
        }

        private void SetDefibri(Defibri defibri, bool drawLine) {
            if (drawLine) {
                SetLine(m_DragonNoise, defibri.points, defibri.amplitude.desired, defibri.frequency.desired, defibri.speed.desired, defibri.desiredWave);
                SetLine(m_BastNoise, defibri.points, defibri.amplitude.start, defibri.frequency.start, defibri.speed.start, defibri.startWave);
            }

            m_SpeedSlider.active = defibri.speed.use;
            m_AmplitudeSlider.active = defibri.amplitude.use;
            if (defibri.amplitude.use)
                UpdateAmplitudeAnim();
            m_FrequencySlider.active = defibri.frequency.use;

            m_WaveFaceImage.sprite = m_WaveFaceSprites[m_BastNoise.wave];
            m_WaveButton.interactable = defibri.useWave;
            foreach (var obj in m_WaveButtonInactiveObjects)
                obj.SetActive(defibri.useWave);
            m_WaveButtonBackground.color = defibri.useWave ? m_WaveButtonBackgroundActiveColor : m_WaveButtonBackgroundDeactiveColor;

            m_SpeedSlider.steps = defibri.speed.steps;
            m_AmplitudeSlider.steps = defibri.amplitude.steps;
            m_FrequencySlider.steps = defibri.frequency.steps;

            m_SpeedSlider.normalizedValue = defibri.speed.start / defibri.speed.steps;
            m_AmplitudeSlider.normalizedValue = defibri.amplitude.start / defibri.amplitude.steps;
            m_FrequencySlider.normalizedValue = defibri.frequency.start / defibri.frequency.steps;

            SetInteraction(!drawLine);
            if (drawLine) {
                FadeIn(() => {
                    _defibring = true;
                    SetInteraction(true);
                });
            }
        }

        private void PlayTutorial(int tutorial) {
            var group = m_Tutorial[tutorial];
            group.DOFade(1.0f, m_TutorialFadeDuration);
            group.gameObject.SetActive(true);

            if (tutorial == 0) {
                m_AmplitudeSlider.normalizedValueChanged.AddListener(EndTutorial0);
                m_FrequencySlider.normalizedValueChanged.AddListener(EndTutorial0);
            } else if (tutorial == 1) {
                m_WaveButton.onClick.AddListener(EndTutorial1);
            }

            void EndTutorial0(float t) {
                m_AmplitudeSlider.normalizedValueChanged.RemoveListener(EndTutorial0);
                m_FrequencySlider.normalizedValueChanged.RemoveListener(EndTutorial0);
                group.DOFade(0.0f, m_TutorialFadeDuration).SetDelay(m_DefibriStartDelay).OnComplete(() => {
                    group.gameObject.SetActive(false);

                    var defibri = m_Defibris[0];

                    m_SpeedSlider.steps = defibri.speed.steps;
                    m_AmplitudeSlider.steps = defibri.amplitude.steps;
                    m_FrequencySlider.steps = defibri.frequency.steps;

                    m_SpeedSlider.normalizedValue = defibri.speed.start / defibri.speed.steps;
                    m_AmplitudeSlider.normalizedValue = defibri.amplitude.start / defibri.amplitude.steps;
                    m_FrequencySlider.normalizedValue = defibri.frequency.start / defibri.frequency.steps;

                    PlayDefibri(0, _onTutorialEnd);
                });
            }

            void EndTutorial1() {
                m_WaveButton.onClick.RemoveListener(EndTutorial1);
                group.DOFade(0.0f, m_TutorialFadeDuration).OnComplete(() => group.gameObject.SetActive(false));
            }
        }

        private float SliderCalc(float val, Defibri.Module module) {
            int value = (int)(val * module.steps);
            int dif = module.index - value;
            return (dif / module.steps) * module.factor + module.desired;
        }

        private void SetInteraction(bool interactable) {
            m_AmplitudeSlider.interact = interactable;
            m_FrequencySlider.interact = interactable;
            m_SpeedSlider.interact = interactable;
            m_WaveButton.interactable = interactable;
        }

        private void SetLine(NoiseLineDrawer noise, int points, float amplitude, float frequency, float speed, int wave) {
            noise.points = points;
            noise.amplitude = amplitude;
            noise.frequency = frequency;
            noise.speed = speed;
            noise.wave = wave;
        }

        private void CheckValues() {
            var defibri = m_Defibris[_defibriIndex];
            int count = 0;
            float speed = Calc(defibri.speed, m_SpeedSlider.normalizedValue);
            float amplitude = Calc(defibri.amplitude, m_AmplitudeSlider.normalizedValue);
            float frequency = Calc(defibri.frequency, m_FrequencySlider.normalizedValue);
            if (defibri.useWave) count++;
            float wave = !defibri.useWave ? 0.0f : m_BastNoise.wave == defibri.desiredWave ? 1.0f : (m_BastNoise.wave == 2 && defibri.desiredWave == 0) || (m_BastNoise.wave == 1 && defibri.desiredWave == 2) || (m_BastNoise.wave == 0 && defibri.desiredWave == 1) ? 0.5f : 0.0f;

            float progress = (speed + amplitude + frequency + wave) / count;

            m_BastNoise.renderer.material.SetColor(k_BastLineColorID, Color.LerpUnclamped(m_BastLineColorStart, m_BastLineColorFinal, m_BastLineColorCurve.Evaluate(progress)));
            m_BastNoise.renderer.widthMultiplier = Mathf.Lerp(m_BastLineSizeProgress.min, m_BastLineSizeProgress.max, progress);

            if (progress >= 0.99f && _onEnd != null) {
                _defibring = false;
                _currentLife = Mathf.Clamp01(_currentLife + defibri.lifeTimerRegen);
                m_LifeTimerImage.fillAmount = _currentLife;
                SetInteraction(false);
                FadeOut(() => _onEnd.Invoke(true));
            }

            float Calc(Defibri.Module module, float val) {
                if (module.use) count++;
                else return 0.0f;
                return 1.0f - Mathf.Abs(val * module.steps - module.index) / module.steps;
            }
        }

        private void EVENT_WaveClicked() {
            m_BastNoise.wave = m_BastNoise.wave > 1 ? 0 : m_BastNoise.wave + 1;
            m_WaveFaceImage.sprite = m_WaveFaceSprites[m_BastNoise.wave];
            CheckValues();
        }

        private void EVENT_AmplitudeChanged(float arg) {
            m_BastNoise.amplitude = SliderCalc(arg, m_Defibris[_defibriIndex].amplitude);
            CheckValues();
            if (!_listeningAmplitude) {
                m_AmplitudeAnimatorA.indexChanged.AddListener(EVENT_AmplitudeAnimationFinished);
                _listeningAmplitude = true;
            }
        }

        private void EVENT_FrequencyChanged(float arg) {
            m_BastNoise.frequency = SliderCalc(arg, m_Defibris[_defibriIndex].frequency);
            CheckValues();
        }

        private void EVENT_SpeedChanged(float arg) {
            m_BastNoise.speed = SliderCalc(arg, m_Defibris[_defibriIndex].speed);
            m_SpeedSliderIconAnim.duration = Mathf.Lerp(m_SpeedSliderRange.min, m_SpeedSliderRange.max, arg);
            CheckValues();
        }

        private void EVENT_AmplitudeAnimationFinished(int index) {
            if (index != m_AmplitudeAnimatorA.values.Length - (m_AmplitudeAnimatorA.values.Length + 1) / 4)
                return;

            m_AmplitudeAnimatorA.indexChanged.RemoveListener(EVENT_AmplitudeAnimationFinished);
            _listeningAmplitude = false;

            UpdateAmplitudeAnim();
        }

        private void UpdateAmplitudeAnim() {
            var defibri = m_Defibris[_defibriIndex];
            int pos = (int)(m_AmplitudeSlider.normalizedValue * defibri.amplitude.steps);
            int frames = defibri.amplitudeFrames + pos;

            int count = frames * 4 - 1;
            int startIndex = count - frames;
            Sprite[] spritesA = new Sprite[count];
            for (int i = 0; i < frames; i++)
                spritesA[i] = m_AmplitudeEmptyState;
            for (int i = 0; i < frames - 1; i++)
                spritesA[i + frames] = m_AmplitudeStates[i];
            spritesA[frames * 2 - 1] = m_AmplitudeStates[frames - 1];
            for (int i = 0; i < frames - 1; i++)
                spritesA[frames * 2 + i] = m_AmplitudeStates[frames - i - 2];
            for (int i = 0; i < frames; i++)
                spritesA[frames * 3 - 1 + i] = m_AmplitudeEmptyState;
            m_AmplitudeAnimatorA.SetValuesAndSetIndex(spritesA, startIndex);

            Sprite[] spritesB = new Sprite[count];
            for (int i = 0; i < frames; i++)
                spritesB[i] = m_AmplitudeStates[frames - 1 - i];
            for (int i = 0; i < frames * 2; i++)
                spritesB[i + frames] = m_AmplitudeEmptyState;
            for (int i = 0; i < frames - 1; i++)
                spritesB[i + frames * 3] = m_AmplitudeStates[i];
            m_AmplitudeAnimatorB.SetValuesAndSetIndex(spritesB, startIndex);
        }

        private void SetLineAlpha(LineRenderer line, float alpha) {
            var col = line.startColor;
            col.a = alpha;
            line.startColor = col;

            col = line.endColor;
            col.a = alpha;
            line.endColor = col;
        }
    }
}
