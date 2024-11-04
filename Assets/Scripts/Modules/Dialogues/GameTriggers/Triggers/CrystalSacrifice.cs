using DG.Tweening;
using NFHGame.AchievementsManagement;
using NFHGame.Animations;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.SceneManagement.SceneState;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame {
    public class CrystalSacrifice : GameTriggerBase {
        public const string SawTyranxGameKey = "sawTyranx";

        [Header("Audio")]
        [SerializeField] private AudioMusicObject m_CrystalAnger0;
        [SerializeField] private AudioMusicObject m_CrystalAnger1;
        [SerializeField] private AudioMusicObject m_TyranxEnvironment;
        [SerializeField] private AudioSource m_NergatSound;

        [Header("Lights Control")]
        [SerializeField] private Light2D[] m_Lights;
        [SerializeField] private float m_DarkerSteps = 1.0f / 3.0f;
        [SerializeField] private float m_DarkerDuration;  
        [SerializeField] private Animator m_SacrificeLights;

        [SerializeField] private Renderer[] m_UnlitRenderers;
        [SerializeField] private Renderer[] m_LitRenderers;
        [SerializeField] private Material m_SpriteUnlitMaterial;
        [SerializeField] private Material m_SpriteLitMaterial;

        [Header("Nergath")]
        [SerializeField] private SpriteRenderer m_NergathEyesRenderer;
        [SerializeField] private float m_NergathEyesFadeDuration, m_NergathAvatarDelay, m_NergathFadeDelay;
        [SerializeField] private SpriteArrayAnimator m_NergathEyesArray;

        [Header("Sacrifice")]
        [SerializeField] private float m_CrouchDuration;
        [SerializeField] private float m_DropDuration;
        [SerializeField] private Sprite m_CrystalItemSprite;
        [SerializeField] private float m_BastheetGetUpDuration;
        [SerializeField] private Sprite m_BastheetSprite;

        [SerializeField] private AudioProviderObject m_DropSound;
        [SerializeField] private ParticleSystem m_SmokeParticle;

        [Header("Tyranx")]
        [SerializeField] private SpriteRenderer m_TyranxRenderer;
        [SerializeField] private float m_TyranxFadeDuration, m_TyranxDuration;
        [SerializeField] private TMP_Text m_TyranxText;

        [Header("Return")]
        [SerializeField] private float m_BastheetReturnTime;

        [Header("Achievements")]
        [SerializeField] private AchievementObject m_SacrificeCrystalAchievement;

        [Header("Bubbles")]
        [SerializeField] private SpriteRenderer[] m_Bubbles;
        [SerializeField] private float m_BubblesFadeDuration;

        [Header("Spammy Dinner Hug")]
        [SerializeField] private SpriteRenderer m_SpammyDinnerHugRenderer;
        [SerializeField] private float m_SpammyHugPosX;
        [SerializeField] private float m_DinnerHugPosX;

        private int _darkerLevel;
        private TextVertexAnimator _vertexAnimator;
        private GameTriggerProcessor.GameTriggerHandler _handler;
        private AudioMusicObject _gameSoundtrack;
        private bool _nergath;

        private int darkerLevel {
            get => _darkerLevel;
            set {
                DOVirtual.Float(1.0f - _darkerLevel * m_DarkerSteps, 1.0f - value * m_DarkerSteps, m_DarkerDuration, SetLights);
                _darkerLevel = value;
            }
        }

        private float[] _lightsCache;
        private bool _lightsCached;

        private void Awake() {
            _lightsCache = new float[m_Lights.Length];
        }

        private void Start() {
            _vertexAnimator = new TextVertexAnimator(m_TyranxText);
        }

        public override bool Match(string id) {
            return id switch {
                "darkerScene" => true,
                "bastheetThrowCrouch" => true,
                "cancelDarkerScene" => true,
                "nergathsEyes" => true,
                "alphaReefRevelation" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _handler = handler;
            if (!_gameSoundtrack)
                _gameSoundtrack = SoundtrackManager.instance.currentSoundtrack;

            switch (id) {
                case "darkerScene":
                    darkerLevel++;
                    UnlitRenderers();
                    handler.onReturnToDialogue.Invoke();
                    break;
                case "bastheetThrowCrouch":
                    GameCharactersManager.instance.bastheet.stateMachine.dropState.StartDrop(m_CrystalItemSprite);
                    FadeBubbles(0.0f, 1.0f);
                    DOVirtual.DelayedCall(m_CrouchDuration, () => handler.onReturnToDialogue.Invoke());
                    break;
                case "cancelDarkerScene":
                    LitRendereres();
                    SoundtrackManager.instance.SetSoundtrack(_gameSoundtrack);
                    handler.onReturnToDialogue.Invoke();
                    break;
                case "nergathsEyes":
                    StartCoroutine(ShowNergath());
                    break;
                case "alphaReefRevelation":
                    StartCoroutine(AlphaReefRevelation());
                    break;
            }

            return true;
        }

        private IEnumerator AlphaReefRevelation() {
            GameKeysManager.instance.ToggleGameKey(Level4StateController.ThrowAlphaReefKey, true);

            if (_nergath) yield break;
            GameCharactersManager.instance.bastheet.stateMachine.dropState.DropOnPool();
            AudioPool.instance.PlaySound(m_DropSound);
            m_SmokeParticle.Play();
            yield return Helpers.GetWaitForSeconds(m_DropDuration);
            AvatarState();
        }

        private IEnumerator ShowNergath() {
            _nergath = true;
            GameCharactersManager.instance.bastheet.stateMachine.dropState.DropOnPool();
            AudioPool.instance.PlaySound(m_DropSound);
            m_SmokeParticle.Play();
            yield return Helpers.GetWaitForSeconds(m_DropDuration);
            _handler.onReturnToDialogue.Invoke();
            darkerLevel = 3;
            m_NergatSound.Play();

            yield return m_NergathEyesRenderer.DOFade(1.0f, m_NergathEyesFadeDuration).WaitForCompletion();
            m_NergathEyesArray.Replay();

            yield return Helpers.GetWaitForSeconds(m_NergathAvatarDelay);
            AvatarState();

            yield return Helpers.GetWaitForSeconds(m_NergathFadeDelay);
            m_NergathEyesRenderer.DOFade(0.0f, m_NergathEyesFadeDuration);
        }

        private void AvatarState() {
            if (GameManager.instance.spammyInParty)
                StartCoroutine(SpammyDinnerHug());
            else
                GameCharactersManager.instance.dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.shit);

            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.avatarState.EnterAvatarState(2, 0, false, true, true);
            bastheet.stateMachine.avatarState.GetDown += AvatarState_GetDown;

            IEnumerator SpammyDinnerHug() {
                var spammy = GameCharactersManager.instance.spammy;
                var dinner = GameCharactersManager.instance.dinner;

                var spammyCoroutine = spammy.WalkOut(m_SpammyHugPosX, 1, true);
                var dinnerCoroutine = dinner.WalkOut(m_DinnerHugPosX, -1, true);

                yield return spammyCoroutine;
                yield return dinnerCoroutine;

                spammy.gameObject.SetActive(false);
                dinner.gameObject.SetActive(false);
                m_SpammyDinnerHugRenderer.gameObject.SetActive(true);
            }
        }

        private void AvatarState_GetDown() {
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.avatarState.GetDown -= AvatarState_GetDown;
            StartCoroutine(GetDown());

            IEnumerator GetDown() {
                foreach (var litSprite in m_LitRenderers)
                    litSprite.material = m_SpriteLitMaterial;
                foreach (var unlitSprite in m_UnlitRenderers)
                    unlitSprite.material = m_SpriteUnlitMaterial;

                SetLights(0.0f);
                _darkerLevel = 3;

                if (GameManager.instance.spammyInParty) {
                    GameCharactersManager.instance.spammy.gameObject.SetActive(true);
                    GameCharactersManager.instance.dinner.gameObject.SetActive(true);
                    GameCharactersManager.instance.dinner.SetFacingDirection(1);
                    GameCharactersManager.instance.spammy.SetFacingDirection(1);
                    m_SpammyDinnerHugRenderer.gameObject.SetActive(false);
                } else {
                    GameCharactersManager.instance.dinner.stateMachine.EnterDefaultState();
                }

                yield return Helpers.GetWaitForSeconds(m_BastheetGetUpDuration);

                bastheet.anim.enabled = false;
                bastheet.anim.GetComponent<SpriteRenderer>().sprite = m_BastheetSprite;
                SoundtrackManager.instance.SetSoundtrack(m_TyranxEnvironment);

                yield return m_TyranxRenderer.DOFade(1.0f, m_TyranxFadeDuration).WaitForCompletion();
                GameKeysManager.instance.ToggleGameKey(SawTyranxGameKey, true);
                m_TyranxText.alpha = 1.0f;
                var commands = DialogueUtility.ProcessInputString(m_TyranxText.text, out var message);
                var textCoroutine = StartCoroutine(_vertexAnimator.AnimateTextIn(commands, message, null));
                yield return m_TyranxRenderer.DOFade(0.0f, m_TyranxFadeDuration).SetDelay(m_TyranxDuration).WaitForCompletion();
                StopCoroutine(textCoroutine);
                m_TyranxText.DOFade(0.0f, m_BastheetReturnTime * 0.5f);

                yield return Helpers.GetWaitForSeconds(m_BastheetReturnTime);
                FadeBubbles(1.0f, 0.0f);
                LitRendereres();
                m_SacrificeLights.enabled = true;
                m_SacrificeLights.transform.GetChild(0).gameObject.SetActive(true);

                yield return Helpers.GetWaitForSeconds(m_BastheetReturnTime);
                bastheet.anim.enabled = true;
                bastheet.stateMachine.EnterState(bastheet.stateMachine.idleState);
                bastheet.SetFacingDirection(false);
                AchievementsManager.instance.UnlockAchievement(m_SacrificeCrystalAchievement);
                _handler.onReturnToDialogue.Invoke();
            }

        }

        private void SetLights(float valueNormalized) {
            for (int i = 0; i < m_Lights.Length; i++)
                m_Lights[i].intensity = _lightsCache[i] * valueNormalized;
        }

        private void LitRendereres() {
            foreach (var litSprite in m_LitRenderers)
                litSprite.material = m_SpriteUnlitMaterial;
            foreach (var unlitSprite in m_UnlitRenderers)
                unlitSprite.material = m_SpriteLitMaterial;
            foreach (var light in m_Lights)
                if (light.TryGetComponent<LightAsFire>(out var lightAsFire)) lightAsFire.enabled = true;

            HaloManager.HaloManager.instance.haloToggled.Invoke(HaloManager.HaloManager.instance.haloActive);
            _darkerLevel = 0;
            _lightsCached = false;
        }

        private void UnlitRenderers() {
            if (!_lightsCached) {
                UpdateLightsCache();
                _lightsCached = true;
            }

            foreach (var litSprite in m_LitRenderers)
                litSprite.material = m_SpriteLitMaterial;
            foreach (var unlitSprite in m_UnlitRenderers)
                unlitSprite.material = m_SpriteUnlitMaterial;
            foreach (var light in m_Lights)
                if (light.TryGetComponent<LightAsFire>(out var lightAsFire)) lightAsFire.enabled = false;

            if (darkerLevel == 1)
                SoundtrackManager.instance.SetSoundtrack(m_CrystalAnger0);
            else if (darkerLevel == 2)
                SoundtrackManager.instance.SetSoundtrack(m_CrystalAnger1);
        }

        private void UpdateLightsCache() {
            for (int i = 0; i < m_Lights.Length; i++) {
                _lightsCache[i] = m_Lights[i].intensity;
            }
        }

        private void FadeBubbles(float start, float end) {
            float[] alpha = new float[m_Bubbles.Length];
            for (int i = 0; i < m_Bubbles.Length; i++) {
                SpriteRenderer bubble = m_Bubbles[i];
                alpha[i] = bubble.color.a;
                bubble.transform.parent.gameObject.SetActive(true);
                bubble.color = new Color(1.0f, 1.0f, 1.0f, start * alpha[i]);
            }

            DOVirtual.Float(start, end, m_BubblesFadeDuration, (a) => {
                for (int i = 0; i < m_Bubbles.Length; i++) {
                    SpriteRenderer bubble = m_Bubbles[i];
                    bubble.color = new Color(1.0f, 1.0f, 1.0f, alpha[i] * a);
                }
            });
        }
    }
}
