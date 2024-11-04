using Articy.Unity;
using Cinemachine;
using DG.Tweening;
using NFHGame.AchievementsManagement;
using NFHGame.Animations;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.Characters.StateMachines;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers.Triggers;
using NFHGame.External;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.Interaction.Behaviours;
using NFHGame.Inventory.UI;
using NFHGame.RangedValues;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.SceneManagement.SceneState;
using NFHGame.Screens;
using NFHGame.Serialization;
using NFHGame.UI;
using NFHGame.UI.Input;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.Battle {
    public class BattleManager : Singleton<BattleManager> {
        public class ShotCondition {
            public Func<bool> condition;
            public Action<Color> colorTween;

            public ShotCondition(Func<bool> condition, Action<Color> colorTween) {
                this.condition = condition;
                this.colorTween = colorTween;
            }
        }

        [SerializeField] private RockSpawner m_RockSpawner;
        [SerializeField] private ShipCanon m_ShipCanon;

        [SerializeField] private TimerRNG m_RockTimer;
        [SerializeField] private TimerRNG m_MiniLaserTimer;

        [SerializeField] private FloatingGround[] m_DestroyBattleGrounds;
        [SerializeField] private FloatingGround[] m_Grounds;

        [SerializeField] private AudioMusicObject m_BattleMusic;

        [Header("Waves")]
        [SerializeField, RangedValue(0.0f, 100.0f)] private RangedFloat m_RockWaveDuration;
        [SerializeField, RangedValue(0.0f, 100.0f)] private RangedFloat m_LargeLaserDelay;
        [SerializeField, RangedValue(0.0f, 100.0f)] private RangedFloat m_MiniLaserComboDelay;
        [SerializeField, RangedValue(0, 100)] private RangedInt m_MiniLaserComboCount;

        [Header("Bar")]
        [SerializeField] private SlicedFilledImage m_ForceBarImage;
        [SerializeField] private float m_ForceBarFadeDuration = 1.0f / 3.0f;

        [SerializeReference] private BattleOffFieldDinnerArrow m_DinnerArrow;

        [SerializeField] private ComposedDragonBattle m_ComposedBattle;
        [SerializeField] private InteractionPlayDialogue m_BattleDialogueTrigger;
        [SerializeField] private ArticyRef m_SkipStartDialogue, m_DefaultBattleDialogue;
        [SerializeField] private InteractionObject m_BackgroundInteraction;

        [Header("End Battle")]
        [SerializeField] private GameObject m_SpecialInputDescription;
        [SerializeField] private float m_BlastActionTime;
        [SerializeField] private Image m_BlastActionFill;
        [SerializeField] private float m_BastheetEndBlast, m_BlastVelocity;
        [SerializeField] private float m_DinnerRunOffset;
        [SerializeField] private float m_RedFlashDuration;
        [SerializeField] private Ease m_RedFlashEase;
        [SerializeField] private Graphic m_DinnerInRangeGraphic;
        [SerializeField] private SpriteRenderer m_DinnerInRangeRenderer, m_BastheetCanShotRenderer, m_BastheetTailCanShotRenderer;
        [SerializeField, RangedValue(-50.0f, 50.0f)] private RangedFloat m_DinnerBlastDistance;
        [SerializeField] private float m_FadeDuration;
        [SerializeField] private float m_FadePause;
        [SerializeField] private ArticyRef m_AfterBattle;
        [SerializeField] private ArticyRef m_AfterBattleSpammy;
        [SerializeField] private float m_AfterBastheetPos, m_AfterDinnerPos, m_AfterSpammyPos;
        [SerializeField] private Transform m_GroundsParent;
        [SerializeField] private AudioMusicObject m_AfterBattleMusic;
        [SerializeField] private Transform m_AnimCamera;
        [SerializeField] private AchievementObject m_KilledDragonAchievement;

        [Header("Fogs")]
        [SerializeField] private SpriteRenderer[] m_DragonFogRenderers;
        [SerializeField] private float m_DragonFogFadeDuration;

        [Header("Explosion")]
        [SerializeField] private float m_ExplosionDuration;
        [SerializeField] private ParticleSystem m_ExplosionParticles;
        [SerializeField] private SpriteArrayAnimator m_EletrtixFXAnimator;
        [SerializeField] private AudioObject m_DragonExplosionSound;

        private ShotCondition[] _conditions;

        private float _rockWaveTime;
        private float _canonShootDelay;
        private bool _performShoot;

        public bool gameOver { get; set; }

        public bool battleStarted { get; private set; } = false;

        public RockSpawner rockSpawner { get => m_RockSpawner; set => m_RockSpawner = value; }
        public ShipCanon shipCanon { get => m_ShipCanon; set => m_ShipCanon = value; }
        public float afterSpammyPos { get => m_AfterSpammyPos; set => m_AfterSpammyPos = value; }

        private bool _skipCutscene;
        private bool _canFinish;
        private bool _blast;
        private float _currentBlastActionTime;

        protected override void Awake() {
            base.Awake();
            m_RockTimer.execute = m_RockSpawner.SpawnRandomRock;
            m_RockTimer.Reset();
            m_MiniLaserTimer.execute = () => m_ShipCanon.PerformMiniLasers(m_MiniLaserComboCount.RandomRange(), m_MiniLaserComboDelay);
            m_MiniLaserTimer.Reset();

            _canonShootDelay = m_LargeLaserDelay.RandomRange();
            _rockWaveTime = m_RockWaveDuration.RandomRange();
        }

        private void Start() {
            _conditions = new ShotCondition[] {
                new ShotCondition(DinnerInRange, DinnerInRangeGraphic),
                new ShotCondition(BastheetCanShot, BastheetInRangeGraphic),
            };
        }

        private void Update() {
            if (rockSpawner.onlyFakeRocks && !battleStarted) {
                _rockWaveTime -= Time.deltaTime;
                if (_rockWaveTime > 0.0f) {
                    m_RockTimer.Step(Time.deltaTime);
                    _performShoot = true;
                }
            }

            if (!battleStarted || _blast) return;

            float delta = Time.deltaTime;
            _rockWaveTime -= delta;
            if (_rockWaveTime > 0.0f) {
                m_RockTimer.Step(delta);
                _performShoot = true;
            } else {
                _canonShootDelay -= delta;
                if (_canonShootDelay <= 0.0f && _performShoot) {
                    m_ShipCanon.PerformShoot();
                    _canonShootDelay = m_LargeLaserDelay.RandomRange();
                    _performShoot = false;
                }
            }

            var bastheet = GameCharactersManager.instance.bastheet;
            _canFinish = bastheet.forceField.currentForce >= bastheet.forceField.maxForce && !gameOver;
            m_SpecialInputDescription.SetActive(_canFinish);

            if (!_canFinish)
                _currentBlastActionTime = 0.0f;

            _currentBlastActionTime = Mathf.Clamp(_currentBlastActionTime + (bastheet.forceField.fieldActive ? Time.deltaTime : -Time.deltaTime), 0.0f, m_BlastActionTime);
            m_BlastActionFill.fillAmount = _currentBlastActionTime / m_BlastActionTime;

            if (_currentBlastActionTime >= m_BlastActionTime) {
                TweenCallback<Color> tweenCalls = null;
                Tween tween = null;
                bool canBlast = true;
                foreach (var cond in _conditions) {
                    if (!cond.condition()) {
                        if (cond.colorTween != null)
                            tweenCalls += (col) => cond.colorTween(col);
                        canBlast = false;
                    }
                }

                if (canBlast) {
                    StartBastheetBlast();
                    m_SpecialInputDescription.SetActive(false);
                } else {
                    tween = DOVirtual.Color(Color.white, Color.red, m_RedFlashDuration, tweenCalls).SetEase(m_RedFlashEase).SetLoops(2, LoopType.Yoyo);
                    _currentBlastActionTime = 0.0f;
                    m_BlastActionFill.fillAmount = 0.0f;
                }
            }

            m_MiniLaserTimer.Step(delta);
        }

        public void GameOver(string label) {
            GameManager.instance.GameOver(label);
        }

        [ContextMenu("Battle")]
        public void StartBattle() {
            PauseScreen.instance.canSave = false;
            InventoryManager.instance.enabled = false;
            m_BackgroundInteraction.enabled = false;
            m_DinnerArrow.gameObject.SetActive(true);

            GameCharactersManager.instance.dinner.ToggleBattleMode(true);
            foreach (var ground in m_DestroyBattleGrounds)
                ground.Crack(int.MaxValue);
            foreach (var ground in m_Grounds)
                ground.enabled = true;

            m_ForceBarImage.transform.parent.SetParent(UserInterfaceInput.instance.transform.transform);
            m_ForceBarImage.transform.parent.SetSiblingIndex(0);
            GameCharactersManager.instance.bastheet.forceField.AssignFilledBar(m_ForceBarImage);
            m_ForceBarImage.transform.parent.GetComponent<CanvasGroup>().ToggleGroupAnimated(true, m_ForceBarFadeDuration);
            battleStarted = true;
        }

        [ContextMenu("Blast")]
        public void StartBastheetBlast() {
            if (_blast) return;

            m_RockTimer.paused = true;
            m_MiniLaserTimer.paused = true;
            _performShoot = false;

            var bastheet = GameCharactersManager.instance.bastheet;
            m_DinnerArrow.gameObject.SetActive(false);

            bool spammyInPond = GameKeysManager.instance.HaveGameKey(ComposedDragonBattle.SpammyInPondGameKey);

            m_RockSpawner.EndBattle();
            m_ShipCanon.EndBattle();
            var dinner = GameCharactersManager.instance.dinner;
            Helpers.vCam.GetCinemachineComponent<CinemachineHardLockToTarget>().m_Damping = 0.45f;
            Helpers.vCam.Follow = bastheet.transform.Find("BlastTile");
            StartCoroutine(dinner.WalkOut(bastheet.transform.position.x - m_DinnerRunOffset, 1, true));
            shipCanon.OpenTeeth();

            m_ForceBarImage.transform.parent.GetComponent<CanvasGroup>().ToggleGroupAnimated(false, m_ForceBarFadeDuration);

            var blastTile = bastheet.transform.Find("BlastTile").GetComponent<TrailRenderer>();
            StartBlast();

            _blast = true;
            InputReader.instance.PushMap(InputReader.InputMap.None);

            void StartBlast() {
                bastheet.stateMachine.animState.Animate(BastheetCharacterController.BastheetCannonShotAnimationHash);
                blastTile.gameObject.SetActive(true);
                blastTile.transform.DOMoveX(m_BastheetEndBlast, m_BlastVelocity).SetEase(Ease.Linear).SetDelay(45.0f / 60.0f).SetSpeedBased(true)
                    .OnStart(() => AudioPool.instance.PlayResourcedAudio("Audio/Bastheet BLAST")).OnComplete(BlastHitted);
            }

            void BlastHitted() {
                m_ExplosionParticles.gameObject.SetActive(true);
                m_EletrtixFXAnimator.enabled = true;
                m_ExplosionParticles.Play();
                AudioPool.instance.PlaySound(m_DragonExplosionSound);
                shipCanon.SetMood(ShipCanon.ShipMoodDeadHash);

                if (!spammyInPond)
                    UnlockBattleAchievement();

                DOVirtual.DelayedCall(m_ExplosionDuration, () => {
                    var fade = FadeScreen.instance.FadeFor(m_FadeDuration);
                    fade.onFinishFadeIn += () => ExplosionEnd(fade);
                });
            }

            void ExplosionEnd(FadeScreen.FadeHandler fade) {
                var dinner = GameCharactersManager.instance.dinner;

                foreach (Transform child in m_GroundsParent) {
                    if (!child.TryGetComponent<FloatingGround>(out var ground)) continue;
                    ground.Regen();
                }

                GameKeysManager.instance.ToggleGameKey(Level4StateController.DestroyedDragonKey, true);
                ArticyVariables.globalVariables.gameState.deadDragon = true;
                (Level4StateController.instance as Level4StateController).DestroyDragon();

                bastheet.SetForceField(false);
                dinner.ToggleBattleMode(false);
                battleStarted = false;

                bastheet.stateMachine.EnterDefaultState();
                blastTile.gameObject.SetActive(false);
                bastheet.transform.Find("BLAST").GetComponent<SpriteRenderer>().enabled = false;

                bastheet.SetPositionX(m_AfterBastheetPos, true);
                dinner.SetPositionX(m_AfterDinnerPos, true);

                Helpers.vCam.GetCinemachineComponent<CinemachineHardLockToTarget>().m_Damping = 0.0f;
                Helpers.vCam.Follow = bastheet.transform;

                DOVirtual.DelayedCall(m_FadePause, () => {
                    fade.FadeOut();

                    fade.onFinishFadeOut += BackToGameplay;
                });
            };

            void BackToGameplay() {
                if (spammyInPond) {
                    var spammy = GameCharactersManager.instance.spammy;
                    spammy.SetPositionX(m_AfterSpammyPos, true);
                }

                PlayAfterBattleMusic();
                PauseScreen.instance.canSave = true;
                InventoryManager.instance.enabled = true;
                m_BackgroundInteraction.enabled = true;

                InputReader.instance.PopMap(InputReader.InputMap.None);
                var handler = DialogueManager.instance.PlayHandledDialogue(spammyInPond ? m_AfterBattleSpammy : m_AfterBattle);
                handler.onDialogueFinished += () => {
                    (Level4StateController.instance as Level4StateController).DefeatDragon();
                    DataManager.instance.ClearSave();
                };
            }
        }

        public void ExitBattle() {
            if (DataManager.instance.gameData.state?.id == Level4StateController.InBattleID) {
                DataManager.instance.gameData.state.id = string.Empty;
                SkipBattleCutscene(false);
            }
        }

        public void SkipBattleCutscene(bool skip) {
            if (!_skipCutscene && skip) {
                m_BattleDialogueTrigger.dialogueReference = m_SkipStartDialogue;
                m_BattleDialogueTrigger.interactionObject.onInteract.AddListener(Event_Interact);
            } else if (_skipCutscene && !skip) {
                m_BattleDialogueTrigger.dialogueReference = m_DefaultBattleDialogue;
                m_BattleDialogueTrigger.interactionObject.onInteract.RemoveListener(Event_Interact);
            }

            _skipCutscene = skip;

            void Event_Interact(Interactor i) {
                m_ComposedBattle.SkipInitialCutscene();
            }
        }

        public void PlayBattleMusic() {
            SoundtrackManager.instance.SetSoundtrack(m_BattleMusic);
        }

        public void PlayAfterBattleMusic() {
            SoundtrackManager.instance.SetSoundtrack(m_AfterBattleMusic);
        }

        public void ResetRockWave() {
            _rockWaveTime = m_RockWaveDuration.RandomRange();
        }

        public void FadeFog(bool animate) {
            if (animate) {
                float[] alpha = new float[m_DragonFogRenderers.Length];
                for (int i = 0; i < alpha.Length; i++) {
                    alpha[i] = m_DragonFogRenderers[i].color.a;
                }

                DOVirtual.Float(1.0f, 0.0f, m_DragonFogFadeDuration, (x) => {
                    for (int i = 0; i < alpha.Length; i++) {
                        var renderer = m_DragonFogRenderers[i];
                        var col = renderer.color;
                        col.a = alpha[i] * x;
                        renderer.color = col;
                    }
                });
            } else {
                for (int i = 0; i < m_DragonFogRenderers.Length; i++) {
                    var renderer = m_DragonFogRenderers[i];
                    var col = renderer.color;
                    col.a = 0.0f;
                    renderer.color = col;
                }
            }
        }

        public static void GetBastheetCollisionData(Collider2D collider, out BastheetCharacterController bastheet, out BastheetForceField forceField, out bool fieldActive, out bool fieldValid) {
            if (collider.TryGetComponent<BastheetCharacterController>(out bastheet)) {
                forceField = bastheet.forceField;
                fieldActive = forceField.fieldActive;
                fieldValid = forceField.fieldValid;
            } else if (collider.TryGetComponent<BastheetForceField>(out forceField)) {
                fieldActive = forceField.fieldActive;
                fieldValid = forceField.fieldValid;
            } else {
                fieldActive = false;
                fieldValid = false;
            }
        }

        private bool DinnerInRange() {
            float dinnerPos = GameCharactersManager.instance.dinner.transform.position.x;
            float distance = dinnerPos - GameCharactersManager.instance.bastheet.transform.position.x;
            if (distance < 0.0f && distance > m_DinnerBlastDistance.min)
                return true;
            if (distance > 0.0f && distance < m_DinnerBlastDistance.max)
                return true;
            return false;
        }

        private void DinnerInRangeGraphic(Color col) {
            m_DinnerInRangeGraphic.color = col;
            m_DinnerInRangeRenderer.color = col;
        }

        public void UnlockBattleAchievement() {
            AchievementsManager.instance.UnlockAchievement(m_KilledDragonAchievement);
        }

        public bool BastheetCanShot() {
            return GameCharactersManager.instance.bastheet.stateMachine.currentState is IBastheetInputState;
        }

        private void BastheetInRangeGraphic(Color col) {
            m_BastheetCanShotRenderer.color = col;
            m_BastheetTailCanShotRenderer.color = col;
        }
    }
}
