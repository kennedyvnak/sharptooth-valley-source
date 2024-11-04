using DG.Tweening;
using NFHGame.Animations;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Battle;
using NFHGame.Characters;
using NFHGame.Input;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.SceneManagement.SceneState;
using NFHGame.Serialization;
using NFHGame.SpammyEvents;
using NFHGame.UI;
using System.Collections;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers.Triggers {
    public class ComposedDragonBattle : GameTriggerBase {
        public const string SpammyInPondGameKey = "spammyInPond";
        public const string SpammyWitnessGameKey = "spammyWitness";

        [Header("Spammy Leaves")]
        [SerializeField] private float m_SpammyPondPosition;
        [SerializeField] private float m_SpammyLeavesDuration;

        [Header("Dragon Shows")]
        [SerializeField] private Transform m_CameraTarget;
        [SerializeField] private float m_DragonShowsCamPosition;
        [SerializeField] private float m_DragonShowsCamDuration;
        [SerializeField] private Ease m_DragonShowsCamAnimEase;

        [Header("Dragon Encounter")]
        [SerializeField] private float m_DragonEncounterTime;

        [Header("Cannon Start")]
        [SerializeField] private float m_CannonStartDuration;

        [Header("Cannon Blast QTE")]
        [SerializeField] private float m_QTECamDuration;
        [SerializeField] private Ease m_QTECamAnimEase;
        [SerializeField] private AnimationCurve m_QTETimeCurve;
        [SerializeField] private float m_QTEDuration;
        [SerializeField] private GameObject m_QTEButtonImage;

        [Header("Alter Arrow Rescue")]
        [SerializeField] private float m_AfterBattleSpammyStepDuration;
        [SerializeField] private float m_AfterBattleSpammyStepSkipDuration;
        [SerializeField] private ArrowBehaviour m_ArrowBehaviour;
        [SerializeField] private Vector2 m_ArrowPosition;
        [SerializeField] private Vector2 m_ArrowForce;
        [SerializeField] private AnimationCurve m_AlterArrowTimeCurve;
        [SerializeField] private float m_AlterArrowRescueTime;
        [SerializeField] private float m_AlterArrowCamDuration;
        [SerializeField] private Ease m_AlterArrowCamEase;

        [Header("Explosion")]
        [SerializeField] private float m_ExplosionDuration;
        [SerializeField] private ParticleSystem m_ExplosionAlterParticles;
        [SerializeField] private SpriteArrayAnimator m_EletrtixFXAnimator;
        [SerializeField] private AudioObject m_DragonExplosionSound;

        [Header("Spammy Reveal")]
        [SerializeField] private float m_SpamRevealFadeTime;
        [SerializeField] private float m_SpamRevealFadeDelay;
        [SerializeField] private float m_SpammyRevealTime;
        [SerializeField] private float m_SpammyRevealFinalPosition;

        [Header("Defense Mode Off")]
        [SerializeField] private AudioMusicObject m_DragonTalkSoundtrack;
        [SerializeField] private Transform m_GolemHeadTransform;
        [SerializeField] private Vector2 m_GolemHeadHandOffset;
        [SerializeField] private float m_CameraReturnTime;
        [SerializeField] private AudioMusicObject m_AfterGolemHeadKeySountrack;

        private GameTriggerProcessor.GameTriggerHandler _currentHandler;
        private Tween _timeTween;
        private Tween _cameraTween;

        public override bool Match(string id) {
            return id switch {
                "SpammyLeaves" => true,
                "showsTheDragon" => true,
                "dragonReveal" => true,
                "cannonStart" => true,
                "cannonBlastQTE" => true,
                "battleBegin" => true,
                "startDragonTalk" => true,
                "bastheetShowsGolemHead" => true,
                "dragonCam" => true,
                "defenseModeOff" => true,
                "dragonExplodes" => true,
                "bastheetFlipsToDinner" => true,
                "dragonCharges" => true,
                "alterArrowRescue" => true,
                "arrowKillsDragon" => true,
                "spammyReveal" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _currentHandler = handler;
            switch (id) {
                case "SpammyLeaves":
                    StartCoroutine(SpammyLeaves());
                    break;
                case "showsTheDragon":
                    ShowsTheDragon();
                    break;
                case "dragonReveal":
                    DragonReveal();
                    break;
                case "cannonStart":
                    CannonStart();
                    break;
                case "cannonBlastQTE":
                    CannonBlackQTE();
                    break;
                case "battleBegin":
                    BattleBegin();
                    break;
                case "startDragonTalk":
                    StartDragonTalk();
                    break;
                case "bastheetShowsGolemHead":
                    BastheetShowsGolemHead();
                    break;
                case "dragonCam":
                    DragonCam();
                    break;
                case "defenseModeOff":
                    DefenseModeOff();
                    break;
                case "dragonExplodes":
                    DragonExplodes();
                    break;
                case "bastheetFlipsToDinner":
                    BastheetFlipsToDinner();
                    break;
                case "dragonCharges":
                    DragonCharges();
                    break;
                case "alterArrowRescue":
                    AlterArrowRescue();
                    break;
                case "arrowKillsDragon":
                    ArrowKillsDragon();
                    break;
                case "spammyReveal":
                    SpammyReveal();
                    break;
                default:
                    return false;
            }

            return true;
        }

        public void SetSpammyInPond(bool isInPond) {
            var spammy = GameCharactersManager.instance.spammy;
            if (isInPond) {
                if (!spammy.isActiveAndEnabled) {
                    GameCharactersManager.instance.bastheet.InitSpammy(spammy);
                    spammy.gameObject.SetActive(true);
                }
                spammy.SetPositionX(m_SpammyPondPosition);
                spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
                spammy.SetFacingDirection(1);
                GameKeysManager.instance.ToggleGameKey(SpammyInPondGameKey, true);
            } else {
                spammy.stateMachine.EnterState(spammy.stateMachine.followState);
                GameKeysManager.instance.ToggleGameKey(SpammyInPondGameKey, false);
            }
        }

        public void SkipInitialCutscene() {
            ArticyVariables.globalVariables.secrets.goldenKingdomPlumbing = true;
            ArticyVariables.globalVariables.gameState.seenTheDragon = true;
            BattleManager.instance.shipCanon.SetMood(ShipCanon.ShipMoodAngryHash);
            BattleManager.instance.PlayBattleMusic();
            BattleManager.instance.FadeFog(false);
            GameCharactersManager.instance.bastheet.SetForceField(true);
            GameCharactersManager.instance.dinner.shitDinner = true;

            if (ArticyVariables.globalVariables.items.golemHead == 1) {
                DialogueManager.instance.SetRoboticState(1);
                DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => {
                    DialogueManager.instance.SetRoboticState(0);
                };
            }
        }

        private void SpammyReveal() {
            ArticyManager.notifications.AddListener("gameState.spamInParty", SpamInPartyChanged);
            ArticyManager.instance.SetVariable("gameState.spamByPond", false);
            GameKeysManager.instance.ToggleGameKey(SpammyInPondGameKey, false);
            GameCharactersManager.instance.spammy.stateMachine.animState.Animate(SpammyCharacterController.ShootingReleasedAnimationHash);
            m_CameraTarget.DOMoveX(BattleManager.instance.afterSpammyPos, m_DragonShowsCamDuration).SetEase(m_DragonShowsCamAnimEase).OnComplete(() => {
                DOVirtual.DelayedCall(m_SpammyRevealTime, () => {
                    var fade = FadeScreen.instance.FadeFor(m_SpamRevealFadeTime);
                    fade.onFinishFadeIn += () => {
                        Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                        Helpers.vCam.PreviousStateIsValid = false;
                        GameCharactersManager.instance.spammy.SetPositionX(m_SpammyRevealFinalPosition, true);
                        GameCharactersManager.instance.spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
                        GameCharactersManager.instance.bastheet.SetFacingDirection(false);
                        GameCharactersManager.instance.dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.normal);
                        GameCharactersManager.instance.dinner.SetFacingDirection(-1);
                        DOVirtual.DelayedCall(m_SpamRevealFadeDelay, () => {
                            m_ArrowBehaviour.sRender.enabled = false;
                            m_ArrowBehaviour.boxCollider.enabled = false;
                            ReturnDialogue();
                            fade.FadeOut();
                        });
                    };
                });
            });
        }

        private void ArrowKillsDragon() {
            _timeTween.Kill();
            Time.timeScale = 1.0f;
            GameCharactersManager.instance.bastheet.SetFacingDirection(true);
        }

        private void AlterArrowRescue() {
            m_ArrowBehaviour.Shoot(m_ArrowForce, m_ArrowPosition, true, false, () => {
                BattleManager.instance.UnlockBattleAchievement();
                m_ExplosionAlterParticles.gameObject.SetActive(true);
                BattleManager.instance.shipCanon.largeLaser.CancelAttack();
                m_EletrtixFXAnimator.enabled = true;
                m_ExplosionAlterParticles.Play();
                AudioPool.instance.PlaySound(m_DragonExplosionSound);
                DOVirtual.DelayedCall(m_ExplosionDuration, ReturnDialogue);
            });

            _timeTween = DOVirtual.Float(0.0f, 1.0f, m_AlterArrowRescueTime, (x) => Time.timeScale = Mathf.Clamp01(m_AlterArrowTimeCurve.Evaluate(x))).SetUpdate(true);

            ReturnDialogue();
        }

        private void DragonExplodes() {
            m_CameraTarget.transform.position = GameCharactersManager.instance.bastheet.transform.position;
            Helpers.vCam.Follow = m_CameraTarget;
            _cameraTween = m_CameraTarget.DOMoveX(m_DragonShowsCamPosition, m_AfterBattleSpammyStepDuration);

            ReturnDialogue();
        }

        private void BastheetFlipsToDinner() {
            GameCharactersManager.instance.bastheet.SetFacingDirection(false);

            ReturnDialogue();
        }

        private void DragonCharges() {
            _cameraTween?.Kill();
            _cameraTween = m_CameraTarget.DOMoveX(m_DragonShowsCamPosition, m_AfterBattleSpammyStepSkipDuration).SetEase(m_AlterArrowCamEase);

            BattleManager.instance.shipCanon.largeLaser.ChargeAttack(false);

            ReturnDialogue();
        }

        private void DefenseModeOff() {
            GameKeysManager.instance.ToggleGameKey(Level4StateController.DragonAliveKey, true);

            Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
            Helpers.vCam.PreviousStateIsValid = false;
            
            // Spammy
            GameKeysManager.instance.ToggleGameKey(SpammyInPondGameKey, false);
            GameKeysManager.instance.ToggleGameKey(SpammyWitnessGameKey, true);
            (Level4StateController.instance as Level4StateController).FadeHorizonSpammy();
            GameCharactersManager.instance.spammy.gameObject.SetActive(false);

            BattleManager.instance.shipCanon.OpenTeeth();

            GameCharactersManager.instance.bastheet.SetForceField(false);
            GameCharactersManager.instance.dinner.ToggleBattleMode(false);

            BattleManager.instance.shipCanon.SetMood(ShipCanon.ShipMoodCalmHash);
            (Level4StateController.instance as Level4StateController).DefeatDragon();

            var handler = DialogueManager.instance.executionEngine.currentHandler;
            handler.onDialogueFinished += () => {
                GameCharactersManager.instance.bastheet.stateMachine.EnterDefaultState();
                Destroy(m_GolemHeadTransform.gameObject);
                SoundtrackManager.instance.SetSoundtrack(m_AfterGolemHeadKeySountrack);
                DataManager.instance.ClearSave();
            };

            ReturnDialogue();
        }

        public void DragonCam() {
            var p = m_CameraTarget.transform.position;
            p.x = m_DragonShowsCamPosition;
            m_CameraTarget.transform.position = p;
            
            Helpers.vCam.Follow = m_CameraTarget;
            Helpers.vCam.PreviousStateIsValid = false;

            ReturnDialogue();
        }

        private void BastheetShowsGolemHead() {
            Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
            Helpers.vCam.PreviousStateIsValid = false;
            
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.pickState.Pick(m_GolemHeadTransform, true);
            m_GolemHeadTransform.localPosition = m_GolemHeadHandOffset;
            m_GolemHeadTransform.gameObject.SetActive(true);
            
            ReturnDialogue();
        }

        private void StartDragonTalk() {
            SoundtrackManager.instance.SetSoundtrack(m_DragonTalkSoundtrack);
            ReturnDialogue();
        }

        private void BattleBegin() {
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.forceField.SetFieldActive(false);
            bastheet.forceField.ReduceForce(int.MaxValue);
            BattleManager.instance.rockSpawner.onlyFakeRocks = false;
            BattleManager.instance.StartBattle();
            ReturnDialogue();
        }

        private void CannonBlackQTE() {
            DialogueManager.instance.SetRoboticState(1);
            DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => {
                DialogueManager.instance.SetRoboticState(0);
            };

            GameCharactersManager.instance.dinner.shitDinner = true;
            GameCharactersManager.instance.bastheet.SetForceField(true);

            BattleManager.instance.shipCanon.largeLaser.Blast(() => {
                Helpers.vCam.Follow = m_CameraTarget;
                Helpers.vCam.PreviousStateIsValid = false;

                m_CameraTarget.DOMoveX(GameCharactersManager.instance.bastheet.transform.position.x, m_QTECamDuration).SetEase(m_QTECamAnimEase).OnComplete(() => {
                    Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                    Helpers.vCam.PreviousStateIsValid = false;

                    InputReader.instance.PushMap(InputReader.InputMap.QuickTimeEvents);
                    InputReader.instance.QTE_ForceField += QTE_ForceFieldEvent;
                    m_QTEButtonImage.SetActive(true);
                }).SetUpdate(true);

                BattleManager.instance.FadeFog(false);

                _timeTween = DOVirtual.Float(0.0f, 1.0f, m_QTEDuration, (t) => {
                    Time.timeScale = m_QTETimeCurve.Evaluate(t);
                });
            }, () => {
                InputReader.instance.PopMap(InputReader.InputMap.QuickTimeEvents);
                DialogueManager.instance.executionEngine.Finish();
            });
        }

        private void CannonStart() {
            BattleManager.instance.shipCanon.largeLaser.ChargeAttack();
            DOVirtual.DelayedCall(m_CannonStartDuration, ReturnDialogue);
        }

        private void DragonReveal() {
            DataManager.instance.SaveCheckpoint(Level4StateController.InBattleID);

            BattleManager.instance.shipCanon.SetMood(ShipCanon.ShipMoodAngryHash);
            BattleManager.instance.PlayBattleMusic();
            DOVirtual.DelayedCall(m_DragonEncounterTime, ReturnDialogue);
        }

        private void ShowsTheDragon() {
            m_CameraTarget.transform.position = GameCharactersManager.instance.bastheet.transform.position;
            Helpers.vCam.Follow = m_CameraTarget;
            Helpers.vCam.PreviousStateIsValid = false;

            m_CameraTarget.DOMoveX(m_DragonShowsCamPosition, m_DragonShowsCamDuration).SetEase(m_DragonShowsCamAnimEase).OnComplete(ReturnDialogue);
        }

        private IEnumerator SpammyLeaves() {
            var spammy = GameCharactersManager.instance.spammy;
            var coroutine = StartCoroutine(spammy.WalkOut(m_SpammyPondPosition));
            DOVirtual.DelayedCall(m_SpammyLeavesDuration, ReturnDialogue);
            GameKeysManager.instance.ToggleGameKey(SpammyInPondGameKey, true);
            yield return coroutine;
            SetSpammyInPond(true);
        }

        private void SpamInPartyChanged(string name, object value) {
            var spamInParty = (bool)value;
            if (!spamInParty) return;

            GameManager.instance.ReloadSpammyInParty(spamInParty);
            GameCharactersManager.instance.spammy.stateMachine.EnterDefaultState();
            GameCharactersManager.instance.dinner.stateMachine.EnterDefaultState();
            ArticyManager.notifications.RemoveListener("gameState.spamInParty", SpamInPartyChanged);
        }

        private void QTE_ForceFieldEvent() {
            InputReader.instance.QTE_ForceField -= QTE_ForceFieldEvent;
            InputReader.instance.PopMap(InputReader.InputMap.QuickTimeEvents);
            _timeTween.Kill();
            Time.timeScale = 1.0f;
            GameCharactersManager.instance.bastheet.forceField.SetFieldActive(true);
            m_QTEButtonImage.SetActive(false);
            ReturnDialogue();
        }

        private void ReturnDialogue() {
            _currentHandler.onReturnToDialogue.Invoke();
        }
    }
}
