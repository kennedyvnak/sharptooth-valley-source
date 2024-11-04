using System.Collections;
using Articy.Unity;
using NFHGame.AchievementsManagement;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.Actors;
using NFHGame.DinnerTrust;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Serialization;
using NFHGame.SpammyEvents;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class Level3StateController : SceneStateController {
        public const string DiscoveredLynssAltar = "Level3_DiscoveredLynssAltar";
        public const string DinosTrappedInAmberKey = "Level3_DinosTrappedInAmber";
        public const string InitJournalBattleKey = "initJournalBattle";

        [SerializeField] private SceneLoadTriggerWalkOut m_ExitingDinosTrigger;
        [SerializeField] private ArticyRef m_HaloExitDialogue; 
        
        [Header("Lynn's Altar Discovery")]
        [SerializeField] private AudioMusicObject m_DiscoveredAltarTrack;
        [SerializeField] private float m_DiscoveredLynnsAltarBastheetPosition;
        [SerializeField] private ArticyRef m_DiscoveredLynnsAltarDialogue;
        [SerializeField] private InteractionObject m_DiscoveredLynnsAltarInteraction;

        [Header("Dinos Trapped in Amber")]
        [SerializeField] private InteractionObject m_DinosTrappedInAmberInteraction;
        [SerializeField] private ArticyRef m_DinosTrappedInAmberDialogue;
        [SerializeField] private SceneLoadAnchorWalkIn m_WalkInTrigger;
        [SerializeField] private float m_FirstEnterPositionX;
        [SerializeField] private SceneLoadTriggerWalkOut m_WalkOutToLevel2Trigger;
        [SerializeField] private AudioProviderObject m_AcessLowTrustSound, m_AcessHighTrustSound;

        [Header("Spammy Battle")]
        [SerializeField] private SceneLoadAnchor m_InSpammyBattleAnchor;
        [SerializeField] private AchievementObject m_SpamArrowAchievement, m_SpamJournalAchievement;

        private SceneLoader.SceneLoadingHandler _sceneHandler;

        private void Start() {
            if (!GameKeysManager.instance.HaveGameKey("Checkpoint_2_3")) m_ExitingDinosTrigger.dialogue = m_HaloExitDialogue;
            
            m_DinosTrappedInAmberInteraction.onInteractorEnter.AddListener((interactor) => {
                if (GameKeysManager.instance.HaveGameKey(DinosTrappedInAmberKey)) return;

                DialogueManager.instance.executionEngine.overridePortraitsSide[DialogueActor.Actor.Dinner] = false;
                _sceneHandler?.ResumeInput();
                var handler = DialogueManager.instance.PlayHandledDialogue(m_DinosTrappedInAmberDialogue);

                handler.onDialogueProcessGameTrigger += (string trigger) => {
                    if (trigger.Equals("gamePlay")) {
                        Destroy(m_DinosTrappedInAmberInteraction.gameObject);
                        DialogueManager.instance.executionEngine.Finish();
                        m_WalkOutToLevel2Trigger.Trigger();
                        DinnerTrustBarController.instance.PlimDownAnim();
                        AudioPool.instance.PlaySound(m_AcessLowTrustSound);
                    } else if (trigger.Equals("goToTheDinos")) {
                        DialogueManager.instance.executionEngine.overridePortraitsSide.Remove(DialogueActor.Actor.Dinner);
                        GameKeysManager.instance.ToggleGameKey(DinosTrappedInAmberKey, true);
                        DinnerTrustBarController.instance.PlimUpAnim();
                        AudioPool.instance.PlaySound(m_AcessHighTrustSound);
                    }
                };
            });

            m_DiscoveredLynnsAltarInteraction.onInteractorEnter.AddListener((interactor) => {
                if (GameKeysManager.instance.HaveGameKey(DiscoveredLynssAltar)) return;
                StartCoroutine(DiscoveredLynnsAltarCoroutine());
            });
        }

        public override void BeforeAnchors(SceneLoader.SceneLoadingHandler handler, System.Collections.Generic.List<SceneLoadAnchor> allAnchors, ref SceneLoadAnchor anchor) {
            if (firstTimeInScene) {
                _sceneHandler = handler;
                m_WalkInTrigger.finalPositionX = m_FirstEnterPositionX;
            }
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);

            if (GameKeysManager.instance.HaveGameKey(InitJournalBattleKey)) {
                JournalSpammyBattle.instance.SetupEnv();
            }

            var bossBattle = ArticyVariables.globalVariables.bossBattle;
            if (bossBattle.arrowSpammy || bossBattle.journalSpammy) return;

            if (GameKeysManager.instance.HaveGameKey("Scene_level4") && !GameKeysManager.instance.HaveGameKey(InitJournalBattleKey)) {
                GameKeysManager.instance.ToggleGameKey(InitJournalBattleKey, true);
                JournalSpammyBattle.instance.SetupEnv();
            } else if (MatchState(handler, ArrowSpammyBattle.InArrowSpammyBattleID)) {
                m_InSpammyBattleAnchor.onLoad?.Invoke(handler);
                ArrowSpammyBattle.instance.InitBattle();
                handler.ResumeInput();
            }
        }

        private IEnumerator DiscoveredLynnsAltarCoroutine() {
            SoundtrackManager.instance.SetSoundtrack(m_DiscoveredAltarTrack);

            InputReader.instance.PushMap(InputReader.InputMap.None);

            yield return GameCharactersManager.instance.bastheet.WalkToPosition(m_DiscoveredLynnsAltarBastheetPosition);

            InputReader.instance.PopMap(InputReader.InputMap.None);

            DialogueManager.instance.PlayHandledDialogue(m_DiscoveredLynnsAltarDialogue).onDialogueFinished += () => {
                GameKeysManager.instance.ToggleGameKey(DiscoveredLynssAltar, true);
                if (!GameKeysManager.instance.HaveGameKey(InitJournalBattleKey))
                    ArrowSpammyBattle.instance.InitBattle();
                else
                    DataManager.instance.Save();
                Destroy(m_DiscoveredLynnsAltarInteraction.gameObject);
            };
        }
    }
}
