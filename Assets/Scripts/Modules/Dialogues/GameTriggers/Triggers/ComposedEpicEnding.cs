using UnityEngine;
using DG.Tweening;
using NFHGame.Characters;
using NFHGame.LevelAssets.Level5;
using System.Collections;
using NFHGame.UI;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using NFHGame.ArticyImpl;
using NFHGame.Serialization;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedEpicEnding : GameTriggerBase {
        [Header("Tyranx Reveal")]
        [SerializeField] private Transform m_AnimCamera;
        [SerializeField] private float m_TyranxRevealPosition, m_TyranxRevealCameraSpeed;

        [Header("Scene Fade Out")]
        [SerializeField] private float m_SceneFadeDuration;
        [SerializeField] private SpriteRenderer[] m_UnlitRenderers;
        [SerializeField] private Material m_SpriteUnlitMaterial;
        [SerializeField] private Light2D[] m_DisableLights;

        [Header("Memories Ritual")]
        [SerializeField, ColorUsage(true, true)] private Color m_BastEyesColor;
        [SerializeField] private TyranxControl m_Tyranx;
        [SerializeField] private float m_TyranxY, m_TyranxUpDuration;
        [SerializeField] private Vector2 m_BastheetAvatarPos;
        [SerializeField] private NoiseLineDrawer m_LightBeam;
        [SerializeField] private SpriteRenderer m_LightBeamBg;
        [SerializeField] private float m_LightBeamFadeDuration;
        [SerializeField] private float m_StartRitualReturnDelay;
        [SerializeField] private SpriteRenderer m_SpammyDinnerHugRenderer;
        [SerializeField] private float m_SpammyHugPosX, m_DinnerHugPosX;
        [SerializeField] private Transform m_MemoriesRitualFocus;

        [Header("After the ritual")]
        [SerializeField] private float m_TyranxIdleAnimDelay;
        [SerializeField] private float m_DinnerPosX, m_SpammyPosX;
        [SerializeField] private Transform m_BastheetAura, m_DinnerAura, m_SpammyAura;
        [SerializeField] private float m_AfterTheRitualReturnDelay;

        [Header("Fly Time")]
        [SerializeField] private Transform m_FlyCamera;
        [SerializeField] private float m_FlyDuration, m_FlyEndY, m_FlyCameraEndX, m_FlyCameraMoveDuration, m_DinnerFlyDelay, m_SpammyFlyDelay;
        [SerializeField] private Ease m_FlyCameraEase;
        [SerializeField] private AnimationCurve m_FlyEase;

        private float _tyranxGroundPosY;
        private Color _beamColor;
        private Material[] _unlitMaterials;

        private GameTriggerProcessor.GameTriggerHandler _handler;

        public override bool Match(string id) {
            return id switch {
                "tyranxReveal" => true,
                "returnCamera" => true,
                "memoriesRitualStart" => true,
                "bastheetAura" => true,
                "afterTheRitual" => true,
                "bastGodMode" => true,
                "bastTurnsToTyranx" => true,
                "flyTime" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _handler = handler;
            switch (id) {
                case "tyranxReveal":
                    TyranxReveal();
                    return true;
                case "returnCamera":
                    ReturnCamera();
                    return true;
                case "getPlayTime":
                    var playTime = System.TimeSpan.FromSeconds(DataManager.instance.gameData.playTime);
                    ArticyManager.instance.SetVariable("gameState.playTime", playTime.Hours);
                    _handler.onReturnToDialogue.Invoke();
                    return true;
                case "memoriesRitualStart":
                    MemoriesRitualStart();
                    return true;
                case "bastheetAura":
                    BastheetAura();
                    return true;
                case "afterTheRitual":
                    AfterTheRitual();
                    return true;
                case "bastGodMode":
                    BastGodMode();
                    return true;
                case "bastTurnsToTyranx":
                    BastTurnsToTyranx();
                    return true;
                case "flyTime":
                    FlyTime();
                    return true;
                default:
                    return false;
            };
        }

        private void TyranxReveal() {
            m_AnimCamera.transform.position = GameCharactersManager.instance.bastheet.transform.position;
            m_AnimCamera.DOMoveX(m_TyranxRevealPosition, m_TyranxRevealCameraSpeed)
                .SetEase(Helpers.CameraInEase).SetSpeedBased(true)
                .OnComplete(_handler.onReturnToDialogue.Invoke);
            Helpers.vCam.Follow = m_AnimCamera;
        }

        private void ReturnCamera() {
            m_AnimCamera.DOMoveX(GameCharactersManager.instance.bastheet.transform.position.x, m_TyranxRevealCameraSpeed)
                .SetEase(Helpers.CameraOutEase).SetSpeedBased(true)
                .OnComplete(() => {
                    _handler.onReturnToDialogue.Invoke();
                    Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                });
        }

        private void MemoriesRitualStart() {
            var fade = FadeScreen.instance.FadeFor(m_SceneFadeDuration);
            fade.onFinishFadeIn += () => {
                _unlitMaterials = new Material[m_UnlitRenderers.Length];
                for (int i = 0; i < m_UnlitRenderers.Length; i++) {
                    _unlitMaterials[i] = m_UnlitRenderers[i].material;
                    m_UnlitRenderers[i].material = m_SpriteUnlitMaterial;
                }
                foreach (var light in m_DisableLights)
                    light.intensity = 0.0f;
                Helpers.vCam.GetComponent<CinemachineConfiner2D>().enabled = false;
                Helpers.vCam.Follow = m_MemoriesRitualFocus;
                Helpers.vCam.PreviousStateIsValid = false;
                fade.FadeOut();
            };

            _tyranxGroundPosY = m_Tyranx.transform.position.y;
            m_Tyranx.animator.Play("TyranxMeditationEnter");
            m_Tyranx.StartFloating();
            m_Tyranx.transform.DOMoveY(m_TyranxY, m_TyranxUpDuration);

            var renderer = GameCharactersManager.instance.bastheet.GetComponent<SpriteRenderer>();
            renderer.material.SetColor("_EmissionColor", m_BastEyesColor);

            var bastheet = GameCharactersManager.instance.bastheet;
            HaloManager.HaloManager.instance.ForceToggle(false);
            bastheet.stateMachine.avatarState.EnterAvatarState(1, 1, m_BastheetAvatarPos, false, false, false);
            bastheet.stateMachine.avatarState.StartLoop += AVATARSTATE_StartLoop;

            DOVirtual.DelayedCall(m_StartRitualReturnDelay, () => _handler.onReturnToDialogue.Invoke());

            StartCoroutine(ScaryDinner());

            void AVATARSTATE_StartLoop() {
                var bastheet = GameCharactersManager.instance.bastheet;
                bastheet.stateMachine.avatarState.StartLoop -= AVATARSTATE_StartLoop;

                DOTween.Kill(bastheet.stateMachine.avatarState);

                _beamColor = m_LightBeamBg.color;
                SetLightBeamIntensity(0.0f, 1.0f, Ease.InSine);
                m_LightBeam.gameObject.SetActive(true);
            }

            IEnumerator ScaryDinner() {
                var dinner = GameCharactersManager.instance.dinner;
                dinner.shitDinner = true;
                if (GameManager.instance.spammyInParty) {
                    var spammy = GameCharactersManager.instance.spammy;

                    var dinnerCoroutine = StartCoroutine(WalkOut(dinner, m_DinnerHugPosX, -1, flipX: false));
                    var spammyCoroutine = StartCoroutine(WalkOut(spammy, m_SpammyHugPosX, -1, flipX: false));

                    yield return dinnerCoroutine;
                    yield return spammyCoroutine;

                    spammy.gameObject.SetActive(false);
                    dinner.gameObject.SetActive(false);
                    m_SpammyDinnerHugRenderer.gameObject.SetActive(true);

                    IEnumerator WalkOut(FollowerCharacterController follower, float pos, int dir, bool flipX) {
                        yield return follower.WalkOut(pos, dir, flipX: flipX);
                        follower.stateMachine.animState.Animate(follower.idleAnimationHash);
                    }
                } else {
                    yield return StartCoroutine(dinner.WalkOut(m_DinnerHugPosX, -1, flipX: false));
                    dinner.stateMachine.animState.Animate(dinner.idleAnimationHash);
                }
            }
        }

        private void BastheetAura() {
            CreateAura(GameCharactersManager.instance.bastheet.transform, m_BastheetAura);
            var dinner = GameCharactersManager.instance.dinner;
            if (GameManager.instance.spammyInParty) {
                var spammy = GameCharactersManager.instance.spammy;
                m_SpammyDinnerHugRenderer.gameObject.SetActive(false);
                spammy.SetPositionX(m_DinnerPosX, false);
                dinner.SetPositionX(m_SpammyPosX, false);
                spammy.gameObject.SetActive(true);
                dinner.gameObject.SetActive(true);
                spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
                Helpers.vCam.PreviousStateIsValid = false;
            }
            dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.normal);

            for (int i = 0; i < m_UnlitRenderers.Length; i++)
                m_UnlitRenderers[i].material = _unlitMaterials[i];
            Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
            Helpers.vCam.GetComponent<CinemachineConfiner2D>().enabled = true;

            HaloManager.HaloManager.instance.ForceToggle(false);
            HaloManager.HaloManager.instance.ForceToggle(true);

            _handler.onReturnToDialogue.Invoke();
        }

        private void AfterTheRitual() {
            var bastheet = GameCharactersManager.instance.bastheet;
            SetLightBeamIntensity(1.0f, 0.0f, Ease.OutSine).OnComplete(() => {
                m_Tyranx.transform.DOMoveY(_tyranxGroundPosY, m_TyranxUpDuration);
                DOVirtual.DelayedCall(m_TyranxIdleAnimDelay, () => {
                    m_Tyranx.EndFloating();
                    m_Tyranx.animator.Play("TyranxMeditationOut");
                });

                bastheet.stateMachine.avatarState.GoDownLikeEnter().OnComplete(() => {
                    bastheet.stateMachine.animState.Animate(BastheetCharacterController.IdleEyeClosedAnimationHash);
                });
            });

            DOVirtual.DelayedCall(m_AfterTheRitualReturnDelay, () => _handler.onReturnToDialogue.Invoke());
        }

        private void BastGodMode() {
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.SetFacingDirection(true);
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.IdleEpicAnimationHashes);
            _handler.onReturnToDialogue.Invoke();
        }

        private void BastTurnsToTyranx() {
            GameCharactersManager.instance.bastheet.SetFacingDirection(false);
            _handler.onReturnToDialogue.Invoke();
        }

        private void FlyTime() {
            var bastheet = GameCharactersManager.instance.bastheet;
            var dinner = GameCharactersManager.instance.dinner;

            m_FlyCamera.transform.position = bastheet.transform.position;
            Helpers.vCam.Follow = m_FlyCamera;
            m_FlyCamera.DOMoveX(m_FlyCameraEndX, m_FlyCameraMoveDuration).SetEase(m_FlyCameraEase);

            bastheet.SetFacingDirection(true);
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.AvatarStateEnterAnimationHashes);
            Fly(bastheet.rb).OnComplete(() => _handler.onReturnToDialogue.Invoke());

            CreateAura(dinner.transform, m_DinnerAura);
            dinner.stateMachine.animState.Animate(DinnerCharacterController.EpicEnterAnimationHash);
            Fly(dinner.rb).SetDelay(m_DinnerFlyDelay);

            if (GameManager.instance.spammyInParty) {
                var spammy = GameCharactersManager.instance.spammy;
                CreateAura(spammy.transform, m_SpammyAura);
                spammy.stateMachine.animState.Animate(SpammyCharacterController.EpicEnterAnimationHash);
                Fly(spammy.rb).SetDelay(m_SpammyFlyDelay);
            }

            Tweener Fly(Rigidbody2D rb) {
                var shadow = rb.transform.Find("Shadow").GetComponent<SpriteRenderer>();
                return FlyUp(rb, rb.position.y, shadow, shadow.color, shadow.transform.position);
            }

            m_Tyranx.animator.Play("TyranxBye1");
        }

        private Tweener SetLightBeamIntensity(float a, float b, Ease ease) {
            return DOVirtual.Float(a, b, m_LightBeamFadeDuration, (x) => {
                m_LightBeam.renderer.endColor = new Color(1.0f, 1.0f, 1.0f, x);
                var color = _beamColor;
                color.a *= x;
                m_LightBeamBg.color = color;
            }).SetEase(ease);
        }

        private Transform CreateAura(Transform t, Transform aura) {
            aura.SetParent(t);
            aura.localPosition = Vector2.zero;
            aura.gameObject.SetActive(true);
            return aura;
        }

        private Tweener FlyUp(Rigidbody2D rb, float posY, SpriteRenderer shadow, Color color, Vector3 shadowPos) {
            rb.gravityScale = 0.0f;
            return rb.DOMoveY(m_FlyEndY, m_FlyDuration).SetEase(m_FlyEase).OnUpdate(() => {
                shadow.transform.position = shadowPos;
                var col = color;
                col.a *= Mathf.InverseLerp(m_FlyEndY, posY, rb.position.y);
                shadow.color = col;
            });
        }
    }
}
