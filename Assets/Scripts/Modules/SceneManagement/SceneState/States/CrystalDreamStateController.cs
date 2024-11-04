using System.Collections;
using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
using DG.Tweening;
using NFHGame.ArticyImpl;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.Actors;
using NFHGame.Input;
using NFHGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace NFHGame.SceneManagement.SceneState {
    public class CrystalDreamStateController : SceneStateController {
        public const string PostCreditsID = "postCredits";

        [SerializeField] private float m_FadeDuration;

        [SerializeField] private float m_StartCutsceneDelay;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private float m_TextFadeDuration;
        [SerializeField] private float m_TextFadeDelay;
        [SerializeField] private float m_TextMaxFadeDuration;

        [SerializeField] private PulseData m_CrystalPulseData;
        [SerializeField] private PulseData m_BackgroundPulseData;

        [SerializeField] private float m_BloomRange;

        [SerializeField] private BackgroundLightData m_BackgroundLightData;
        [SerializeField] private AppearCrystalData m_AppearCrystalData;
        [SerializeField] private ImKhatusData m_ImKhatusData;
        [SerializeField] private DisappearCrystalData m_DisappearCrystalData;

        [SerializeField] private DialogueLine[] m_Lines;
        [SerializeField] private SceneReference m_Level1SceneRef;

        [SerializeField] private Color m_BastheetColor;
        [SerializeField] private Color m_CrystalColor;
        [SerializeField] private TMP_FontAsset m_CrystalFont;

        [SerializeField] private Button m_StepButton;

        [SerializeField] private ArticyRef[] m_PostCreditsDialiogue;
        [SerializeField] private SceneReference m_PostCreditsScene;

        private List<SpriteRenderer> _pulseSprites;

        private int _currentLineIndex = 0;

        private float _currentCrystalPulse;
        private Tween _pulseCrystalTween;

        private bool _canPulseBg;
        private float _currentBgPulse;
        private Tween _pulseBgTween;

        private List<Tween> _pulseSparkles;

        private Bloom _bloomComponent;
        private bool _fixBloom = false;
        private bool _canStep;
        private bool _inputStep = false;

        private bool _isPostCredits;

        protected override void Awake() {
            base.Awake();
            _pulseSprites = new List<SpriteRenderer>();
            _pulseSparkles = new List<Tween>();
        }

        private void Start() {
            InputReader.instance.OnStepDialogue += INPUT_StepDialogue;
            m_StepButton.onClick.AddListener(INPUT_StepDialogue);

            m_DisappearCrystalData.volume.profile.TryGet(out _bloomComponent);

            DOVirtual.DelayedCall(m_StartCutsceneDelay, PlayText);

            _pulseCrystalTween = DOTween.To(() => m_CrystalPulseData.minValue, x => _currentCrystalPulse = x, m_CrystalPulseData.maxValue, m_CrystalPulseData.duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .OnUpdate(() => {
                        if (Random.value < m_CrystalPulseData.sparkleChance) {
                            foreach (SpriteRenderer pulseSprite in _pulseSprites) {
                                _pulseSparkles.Add(pulseSprite.DOFade(pulseSprite.color.a + Random.Range(-m_CrystalPulseData.sparkleStrength, m_CrystalPulseData.sparkleStrength), m_CrystalPulseData.sparkleDuration)
                                    .SetEase(Ease.OutSine));
                            }
                        }
                    });

            InputReader.instance.PushMap(InputReader.InputMap.Dialogue | InputReader.InputMap.UI);
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);
            if (handler.anchorID == PostCreditsID) {
                _isPostCredits = true;

                PulseBackground();
                foreach (var part in m_AppearCrystalData.crystalParts) {
                    part.enabled = true;
                    _pulseSprites.Add(part);
                }
            }
        }

        protected override void OnDestroy() {
            _pulseCrystalTween.Kill();
            _pulseBgTween.Kill();
            foreach (var pulseSparkle in _pulseSparkles) {
                if (pulseSparkle.IsActive()) {
                    pulseSparkle.Kill();
                }
            }
            base.OnDestroy();
        }

        private void Update() {
            foreach (var pulseSprite in _pulseSprites) {
                var color = pulseSprite.color;
                color.a = _currentCrystalPulse;
                pulseSprite.color = color;
            }

            if (_canPulseBg) {
                var color = m_BackgroundLightData.lightRenderer.color;
                color.a = _currentCrystalPulse;
                m_BackgroundLightData.lightRenderer.color = color;
            }

            if (!_fixBloom)
                _bloomComponent.intensity.value = _currentCrystalPulse * m_BloomRange;
        }

        private void EndCutscene() {
            if (!_isPostCredits) {
                _fixBloom = true;
                DOVirtual.Float(_bloomComponent.intensity.value, m_DisappearCrystalData.endBloomValue, m_DisappearCrystalData.bloomDuration, (x) => _bloomComponent.intensity.value = x);

                m_DisappearCrystalData.whiteSquare.DOFade(1.0f, m_FadeDuration).SetDelay(m_DisappearCrystalData.whiteSquareFadeDelay).OnComplete(() => {
                    InputReader.instance.PopMap(InputReader.InputMap.Dialogue | InputReader.InputMap.UI);
                    var handler = SceneLoader.instance.CreateHandler(m_Level1SceneRef, "newGame");
                    handler.blackScreen = true;
                    handler.saveGame = false;
                    SceneLoader.instance.LoadScene(handler);
                });
            } else {
                var fadeHandler = FadeScreen.instance.FadeFor(m_DisappearCrystalData.bloomDuration);
                fadeHandler.onFinishFadeIn += () => {
                    InputReader.instance.PopMap(InputReader.InputMap.Dialogue | InputReader.InputMap.UI);
                    var handler = SceneLoader.instance.CreateHandler(m_PostCreditsScene, "crystalDream");
                    handler.blackScreen = true;
                    handler.saveGame = false;
                    SceneLoader.instance.LoadScene(handler);
                };
            }
        }

        public void OnBackgroundLight() {
            m_BackgroundLightData.lightRenderer.DOFade(m_BackgroundPulseData.minValue, m_FadeDuration).OnComplete(PulseBackground);
        }

        public void OnAppearCrystal() {
            foreach (var part in m_AppearCrystalData.crystalParts) {
                var color = part.color;
                color.a = 0.0f;
                part.color = color;
                part.enabled = true;
                part.DOFade(_currentCrystalPulse, m_FadeDuration).OnComplete(() => _pulseSprites.Add(part));
            }
            DOVirtual.DelayedCall(m_FadeDuration, () => m_AppearCrystalData.silhouette.enabled = false);
        }

        public void OnImKhatus() {
            foreach (var part in m_ImKhatusData.crystalParts) {
                var color = part.color;
                var alpha = color.a;
                color.a = 0.0f;
                part.color = color;
                part.enabled = true;
                part.DOFade(alpha, m_FadeDuration).OnComplete(() => part.DOFade(0.0f, m_FadeDuration).SetDelay(m_ImKhatusData.lightFadeDelay));
            }

            var sequence = DOTween.Sequence();
            var lightTime = m_FadeDuration / m_ImKhatusData.lights.Length;
            foreach (var light in m_ImKhatusData.lights) {
                var scale = light.transform.localScale;
                light.transform.localScale = Vector3.zero;
                sequence.Append(light.transform.DOScale(scale, lightTime).OnStart(() => light.DOFade(0.0f, m_ImKhatusData.lightsAnimDuration).SetDelay(m_ImKhatusData.lightFadeDelay)));
                light.enabled = true;
            }
            sequence.Play();
        }

        private void PlayText() {
            SetCanStep(false);
            var textFadeTween = m_Text.DOFade(0.0f, m_TextFadeDuration);

            DialogueActor actor = null;
            string text = null;
            TweenCallback onStart = null;
            if (!_isPostCredits) {
                var line = m_Lines[_currentLineIndex];
                var lineObj = line.line.GetObject();
                if (lineObj is IObjectWithSpeaker speaker)
                    DialogueDatabase.instance.TryGetActor(speaker.Speaker.TechnicalName, out actor);
                text = lineObj.ExtractText();
                onStart = line.action.Invoke;
            } else {
                var line = m_PostCreditsDialiogue[_currentLineIndex];
                var lineObj = line.GetObject();
                if (lineObj is IObjectWithSpeaker speaker)
                    DialogueDatabase.instance.TryGetActor(speaker.Speaker.TechnicalName, out actor);
                text = lineObj.ExtractText();
            }

            textFadeTween.OnComplete(() => {
                if (actor.actor == DialogueActor.Actor.Bastheet) {
                    m_Text.color = m_BastheetColor;
                    m_Text.font = actor.dialogueFont;
                } else if (actor.actor == DialogueActor.Actor.Crystal) {
                    m_Text.color = m_CrystalColor;
                    m_Text.font = m_CrystalFont;
                }

                m_Text.text = text;
                m_Text.fontStyle = actor.actor == DialogueActor.Actor.Crystal ? FontStyles.UpperCase : FontStyles.Normal;
                textFadeTween = m_Text.DOFade(1.0f, m_TextFadeDuration).SetDelay(m_TextFadeDelay).OnStart(onStart).OnComplete(() => StartCoroutine(WaitToPlayNextText()));
            });

            _currentLineIndex++;
        }

        private void PulseBackground() {
            _canPulseBg = true;
            _pulseBgTween = DOTween.To(() => m_BackgroundPulseData.minValue, x => _currentBgPulse = x, m_BackgroundPulseData.maxValue, m_BackgroundPulseData.duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .OnUpdate(() => {
                    if (Random.value < m_BackgroundPulseData.sparkleChance) {
                        _pulseSparkles.Add(m_BackgroundLightData.lightRenderer.DOFade(m_BackgroundLightData.lightRenderer.color.a + Random.Range(-m_BackgroundPulseData.sparkleStrength, m_BackgroundPulseData.sparkleStrength), m_BackgroundPulseData.sparkleDuration)
                                .SetEase(Ease.OutSine));
                    }
                });
        }

        private void SetCanStep(bool canStep) {
            m_StepButton.enabled = canStep;
            _canStep = canStep;
        }

        private IEnumerator WaitToPlayNextText() {
            SetCanStep(true);
            while (!_inputStep) yield return null;
            if (_currentLineIndex < (_isPostCredits ? m_PostCreditsDialiogue.Length : m_Lines.Length)) {
                PlayText();
            } else {
                m_Text.DOFade(0.0f, m_TextFadeDuration).SetDelay(m_TextFadeDelay).OnComplete(EndCutscene);
            }
            _inputStep = false;
        }

        private void INPUT_StepDialogue() {
            if (_canStep) _inputStep = true;
        }

        [System.Serializable]
        public struct DialogueLine {
            public ArticyRef line;
            public UnityEvent action;
        }

        [System.Serializable]
        public struct BackgroundLightData {
            public SpriteRenderer lightRenderer;
        }

        [System.Serializable]
        public struct AppearCrystalData {

            public SpriteRenderer silhouette;
            public SpriteRenderer[] crystalParts;
        }

        [System.Serializable]
        public struct ImKhatusData {
            public float lightsAnimDuration;
            public float lightFadeDelay;
            public SpriteRenderer[] crystalParts;
            public SpriteRenderer[] lights;
        }

        [System.Serializable]
        public struct DisappearCrystalData {
            public float endBloomValue;
            public float bloomDuration;
            public float whiteSquareFadeDelay;
            public SpriteRenderer whiteSquare;
            public Volume volume;
        }

        [System.Serializable]
        public struct PulseData {
            public float minValue;
            public float maxValue;
            public float duration;
            public float sparkleChance;
            public float sparkleDuration;
            public float sparkleStrength;
        }
    }
}