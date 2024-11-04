using Articy.Unity;
using Cinemachine;
using DG.Tweening;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Battle;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.DialogueSystem.GameTriggers.Triggers;
using NFHGame.HaloManager;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.Interaction.Behaviours;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame.SceneManagement.SceneState {
    public class Level4StateController : SceneStateController {
        [Serializable]
        public struct Wormhole {
            public Collider2D collider;
            public float pos;
            public bool facingDirection;
        }

        public const string ThrowAlphaReefKey = "throwAlphaReef";
        public const string LitLakeKey = "litLake";
        public const string DestroyedDragonKey = "destroyedDragon";
        public const string DragonAliveKey = "dragonAlive";

        private const string k_AfterRessurrectDragonDialogueKey = "afterAliveDragonDialogue";
        private const string k_DragonEnterGameKey = "enteredDragon";
        public const string InBattleID = "inBattle";

        [Serializable]
        public struct GlobalLight {
            public float lakeOnHaloOnIntensity;
            public float lakeOnHaloOffIntensity;
            public float lakeOffHaloOnIntensity;
            public float lakeOffHaloOffIntensity;
            public HaloLightIntensityListener listener;

            public void Init(bool isLakeAwaken) {
                listener.intensityWhenDisabled = isLakeAwaken ? lakeOnHaloOffIntensity : lakeOffHaloOffIntensity;
                listener.intensityWhenEnabled = isLakeAwaken ? lakeOnHaloOnIntensity : lakeOffHaloOnIntensity;
            }
        }

        [Serializable]
        public struct DragonBodySprite {
            public SpriteRenderer renderer;
            public Sprite destroyedSprite;
        }

        [Header("Entering The Ponds")]
        [SerializeField] private ArticyRef m_EnteringThePondsDialogue;
        [SerializeField] private float m_EnteringThePondsFinalPositionX, m_EnteringThePondsStartPositionX;
        [SerializeField] private SceneLoadAnchorWalkIn m_EnteringPondsAnchor;
        [SerializeField] private SceneLoadTriggerWalkOut m_ExitingPondsTrigger;
        [SerializeField] private ArticyRef m_HaloExitDialogue; 
        [SerializeField] private GameObject m_SulfurLakeTrigger;

        [Header("Spammy Background")]
        [SerializeField] private GameObject m_SpammyBackgroundObject;
        [SerializeField] private InteractionPlayDialogue m_SpmmayBackgroundInteraction;
        [SerializeField] private ArticyRef m_SpammyBackgroundDialogueRef, m_SpammyWitnessDialogueRef;

        [Header("Soundtrack")]
        [SerializeField] private ChangeSoundtrackOnStart m_SoundtrackController;
        [SerializeField] private AudioMusicObject m_IntoxicatingSoundtrack, m_DragonSoundtrack, m_AliveSoundtrack;

        [Header("In Battle")]
        [SerializeField] private float m_InBattleBastheetPositionX;
        [SerializeField] private float m_InBattleDinnerPositionX;

        [Header("GlobalLights")]
        [SerializeField] private GlobalLight m_GlobalLight;
        [SerializeField] private GlobalLight m_ForegroundLight;
        [SerializeField] private Light2D m_HorizonLight;
        [SerializeField] private float m_HorizonLightOn, m_HorizonLightOff;

        [Header("Objects")]
        [SerializeField] private GameObject[] m_AwakenObjects;
        [SerializeField] private GameObject[] m_SleepObjects;

        [Header("Colors")]
        [SerializeField] private SpriteRenderer[] m_Croaks;
        [SerializeField] private Color m_LakeAwakeCroakColor;
        [SerializeField] private Color m_LakeSleepCroakColor;

        [Header("Bubbles")]
        [SerializeField] private SpriteRenderer[] m_Bubbles, m_BubblesSulfur;
        [SerializeField] private Color m_LakeAwakeBubbleColor, m_LakeAwakeBubbleSulfurColor;
        [SerializeField] private Color m_LakeSleepBubbleColor, m_LakeSleepBubbleSulfurColor;

        [Header("Dragon Battle")]
        [SerializeField] private ComposedDragonBattle m_ComposedDragonBattle;
        [SerializeField] private GameObject[] m_DestroyOnDragonDefeated;
        [SerializeField] private SpriteRenderer m_DragonTeethRenderer;
        [SerializeField] private Sprite m_DragonTeethOpen;
        [SerializeField] private DragonBodySprite[] m_DragonBodyParts;

        [Header("Dragon Interaction")]
        [SerializeField] private InteractionObject m_DragonLoadSceneInteraction;
        [SerializeField] private InteractionObject m_DragonDialogueInteraction;
        [SerializeField] private ArticyRef m_AfterRessurrectDragonDialogueRef;

        [Header("Wormhole")]
        [SerializeField] private Wormhole[] m_Wormholes;
        [SerializeField] private float m_MiddleLevelPosition;

        private bool _inBattle;
        private bool _litLake;
        private bool _defeatedDragon;
        private bool _ressurrectedDialogue;

        protected override void Awake() {
            base.Awake();
            _litLake = GameKeysManager.instance.HaveGameKey(LitLakeKey);
            bool dragonAlive = GameKeysManager.instance.HaveGameKey(DragonAliveKey);
            m_SoundtrackController.soundtrack = dragonAlive ? m_AliveSoundtrack : (_litLake ? m_DragonSoundtrack : m_IntoxicatingSoundtrack);
        }

        private void Start() {
            m_GlobalLight.Init(_litLake);
            m_ForegroundLight.Init(_litLake);
            m_HorizonLight.intensity = _litLake ? m_HorizonLightOn : m_HorizonLightOff;

            foreach (var awakeObj in m_AwakenObjects)
                awakeObj.SetActive(_litLake);

            foreach (var sleepObj in m_SleepObjects)
                sleepObj.SetActive(!_litLake);

            var croakColor = _litLake ? m_LakeAwakeCroakColor : m_LakeSleepCroakColor;
            foreach (var croak in m_Croaks)
                croak.color = croakColor;

            var bubbleColor = _litLake ? m_LakeAwakeBubbleColor : m_LakeSleepBubbleColor;
            foreach (var bubble in m_Bubbles)
                bubble.color = bubbleColor;

            var sulfurColor = _litLake ? m_LakeAwakeBubbleSulfurColor : m_LakeSleepBubbleSulfurColor;
            foreach (var sulfur in m_BubblesSulfur)
                sulfur.color = sulfurColor;

            var destroyedDragon = GameKeysManager.instance.HaveGameKey(DestroyedDragonKey);
            var aliveDragon = GameKeysManager.instance.HaveGameKey(DragonAliveKey);
            if (destroyedDragon || aliveDragon) {
                if (destroyedDragon) DestroyDragon(!aliveDragon);
                if (aliveDragon) BattleManager.instance.shipCanon.SetMood(ShipCanon.ShipMoodIdleHash);
                DefeatDragon();
            }

            var articyVars = ArticyVariables.globalVariables;
            bool spammyBackground = true;
            if (articyVars.gameState.spamInParty || articyVars.gameState.spamByPond || articyVars.gameState.SpammyDidntJoin)
                spammyBackground = false;
            
            if (GameKeysManager.instance.HaveGameKey(ComposedDragonBattle.SpammyWitnessGameKey)) {
                spammyBackground = true;
                m_SpmmayBackgroundInteraction.dialogueReference = m_SpammyWitnessDialogueRef;
            } else if (spammyBackground) {
                m_SpmmayBackgroundInteraction.dialogueReference = m_SpammyBackgroundDialogueRef;
            }

            m_SpammyBackgroundObject.SetActive(spammyBackground);
            if (!GameKeysManager.instance.HaveGameKey("Checkpoint_2_4")) m_ExitingPondsTrigger.dialogue = m_HaloExitDialogue;
        }

        public override void BeforeAnchors(SceneLoader.SceneLoadingHandler handler, List<SceneLoadAnchor> allAnchors, ref SceneLoadAnchor anchor) {
            base.BeforeAnchors(handler, allAnchors, ref anchor);
            var gameState = ArticyVariables.globalVariables.gameState;

            if (gameState.firstTimePonding) {
                m_EnteringPondsAnchor.finalPositionX = m_EnteringThePondsFinalPositionX;
                m_EnteringPondsAnchor.onFinish.AddListener(() => DialogueManager.instance.PlayHandledDialogue(m_EnteringThePondsDialogue));
                anchor = m_EnteringPondsAnchor;
            } else if (GameKeysManager.instance.HaveGameKey(LitLakeKey) && gameState.FirstTimeLitLake) {
                anchor = m_EnteringPondsAnchor;
                m_EnteringPondsAnchor.startPositionX = m_EnteringThePondsStartPositionX;
                m_EnteringPondsAnchor.finalPositionX = m_EnteringThePondsFinalPositionX;
                m_EnteringPondsAnchor.onFinish.AddListener(() => DialogueManager.instance.PlayHandledDialogue(m_EnteringThePondsDialogue));
            } else if (MatchState(handler, InBattleID)) {
                _inBattle = true;
            }
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);

            if (_inBattle) {
                GameCharactersManager.instance.bastheet.SetPositionX(m_InBattleBastheetPositionX);
                GameCharactersManager.instance.dinner.SetPositionX(m_InBattleDinnerPositionX);
                BattleManager.instance.SkipBattleCutscene(true);
                handler.ResumeInput();
            }

            if (m_SulfurLakeTrigger) {
                if (ArticyVariables.globalVariables.gameState.spamByPond || _inBattle) {
                    m_SulfurLakeTrigger.SetActive(false);
                } else if (ArticyVariables.globalVariables.gameState.spamInParty) {
                    m_SulfurLakeTrigger.SetActive(true);
                }
            }

            if (GameKeysManager.instance.HaveGameKey(ComposedDragonBattle.SpammyInPondGameKey)) {
                m_ComposedDragonBattle.SetSpammyInPond(true);
                if (!_inBattle) {
                    var location = stateData.locations[1];
                    var offset = location.facingRight ? -1 : 1;

                    var bastheet = GameCharactersManager.instance.bastheet;
                    var dinner = GameCharactersManager.instance.dinner;

                    bastheet.SetPositionX(location.positionX, location.facingRight);
                    dinner.SetPositionX(location.positionX + bastheet.dinnerOffset * offset, location.facingRight);
                    handler.ResumeInput();
                }
            }

            if (_ressurrectedDialogue) {
                bool spam = GameManager.instance.spammyInParty;
                GameCharactersManager.instance.bastheet.SetFacingDirection(true);

                GameCharactersManager.instance.dinner.SetFacingDirection(1);
                GameCharactersManager.instance.dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.normal);

                if (spam) {
                    GameCharactersManager.instance.spammy.SetFacingDirection(1);
                    GameCharactersManager.instance.spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
                }

                var dHandler = DialogueManager.instance.PlayHandledDialogue(m_AfterRessurrectDragonDialogueRef);
                dHandler.onDialogueFinished += () => {
                    GameCharactersManager.instance.dinner.stateMachine.EnterDefaultState();
                    if (spam) GameCharactersManager.instance.spammy.stateMachine.EnterDefaultState();
                };
            }

            if (_defeatedDragon) {
                DoWormhole(GameCharactersManager.instance.bastheet.rb.position.x < m_MiddleLevelPosition, false);
            }
        }

        public void DefeatDragon() {
            _defeatedDragon = true;
            foreach (var obj in m_DestroyOnDragonDefeated)
                Destroy(obj);
            m_DragonTeethRenderer.sprite = m_DragonTeethOpen;
            SetupDragonInteraction();
            BattleManager.instance.FadeFog(false);
        }

        public void DestroyDragon(bool withAnim = true) {
            if (withAnim) BattleManager.instance.shipCanon.SetMood(ShipCanon.ShipMoodDeadHash);
            m_DragonTeethRenderer.gameObject.SetActive(false);
            foreach (var dragonPart in m_DragonBodyParts) {
                dragonPart.renderer.sprite = dragonPart.destroyedSprite;
            }
        }

        public void SulfurLakeDialogueFinished() {
            if (ArticyVariables.globalVariables.gameState.spamInParty) {
                m_SulfurLakeTrigger.SetActive(true);
            } else {
                m_SulfurLakeTrigger.SetActive(false);
            }
        }

        public void LoadedDragonMouth() {
            if (!GameKeysManager.instance.HaveGameKey(k_AfterRessurrectDragonDialogueKey) && GameKeysManager.instance.HaveGameKey(HeartGame.RessurrectDragonGameKey)) {
                GameKeysManager.instance.ToggleGameKey(k_AfterRessurrectDragonDialogueKey, true);
                _ressurrectedDialogue = true;
            }
        }

        public void FadeHorizonSpammy() {
            var renderer = m_SpammyBackgroundObject.GetComponent<SpriteRenderer>();
            renderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            m_SpammyBackgroundObject.SetActive(true);
            m_SpmmayBackgroundInteraction.dialogueReference = m_SpammyWitnessDialogueRef;
            renderer.DOFade(1.0f, 5.0f);
        }

        public void WormholeTo(bool left) {
            InputReader.instance.PushMap(InputReader.InputMap.None);
            var fadeHandler = FadeScreen.instance.FadeFor(0.5f);
            fadeHandler.onFinishFadeIn = () => {
                DoWormhole(left, true);
                fadeHandler.onFinishFadeOut += () => InputReader.instance.PopMap(InputReader.InputMap.None);
                fadeHandler.FadeOut();
            };
        }

        private void DoWormhole(bool left, bool positioning) {
            var toDisable = m_Wormholes[left ? 1 : 0];
            var toEnable = m_Wormholes[left ? 0 : 1];

            toDisable.collider.gameObject.SetActive(false);
            toEnable.collider.gameObject.SetActive(true);

            Helpers.vCam.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = toEnable.collider;
            if (positioning)
                GameCharactersManager.instance.SetPosition(toEnable.pos, toEnable.facingDirection);
        }

        private void SetupDragonInteraction() {
            if (GameKeysManager.instance.HaveGameKey(k_DragonEnterGameKey)) {
                m_DragonLoadSceneInteraction.gameObject.SetActive(true);
            } else {
                m_DragonDialogueInteraction.gameObject.SetActive(true);
                m_DragonDialogueInteraction.GetComponent<InteractionPlayDialogue>().callbacks.onDialogueProcessGameTrigger.AddListener((t) => {
                    if (t == "enterDragon") {
                        GameKeysManager.instance.ToggleGameKey(k_DragonEnterGameKey, true);
                        DialogueManager.instance.executionEngine.Finish();
                        m_DragonLoadSceneInteraction.GetComponent<InteractionLoadScene>().LoadScene();
                    }
                });
            }
        }
    }
}
