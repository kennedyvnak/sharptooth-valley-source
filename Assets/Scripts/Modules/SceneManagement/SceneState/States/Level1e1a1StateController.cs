using System.Collections.Generic;
using Articy.Unity;
using NFHGame.ArticyImpl;
using NFHGame.DialogueSystem;
using NFHGame.Serialization;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class Level1e1a1StateController : SceneStateController {
        private const string k_FirstEntryState = "golemFirstEntry";

        [Header("Friendship cutscene")]
        [SerializeField] private ArticyRef m_FirstEntryDialogue;

        private bool _firstEntryDialogue;

        private void Start() {
            HaloManager.HaloManager.instance.SetBlackout(ArticyImpl.Variables.ArticyVariables.globalVariables.gameState.blackout);
            ArticyManager.notifications.AddListener("gameState.blackout", BlackoutChanged);
        }

        protected override void OnDestroy() {
            if (HaloManager.HaloManager.instance)
                HaloManager.HaloManager.instance.SetBlackout(false);
            ArticyManager.notifications.RemoveListener("gameState.blackout", BlackoutChanged);
            base.OnDestroy();
        }

        public override void BeforeAnchors(SceneLoader.SceneLoadingHandler handler, List<SceneLoadAnchor> allAnchors, ref SceneLoadAnchor anchor) {
            base.BeforeAnchors(handler, allAnchors, ref anchor);

            if (firstTimeInScene || MatchState(handler, k_FirstEntryState)) {
                handler.ResumeInput();
                _firstEntryDialogue = true;
            }
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);

            if (_firstEntryDialogue) StartEntryCutscene();
        }

        private void StartEntryCutscene() {
            DataManager.instance.SaveCheckpoint(k_FirstEntryState);

            var handler = DialogueManager.instance.PlayHandledDialogue(m_FirstEntryDialogue);
            handler.onDialogueFinished += EndEntryCutscene;
        }

        private void EndEntryCutscene() {
            DataManager.instance.ClearSave();
        }

        private void BlackoutChanged(string varName, object isBlackout) {
            bool blackout = (bool)isBlackout;
            HaloManager.HaloManager.instance.SetBlackout(blackout);
        }
    }
}