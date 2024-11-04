using System.Collections.Generic;
using Articy.Unity;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Serialization;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class Level1e1StateController : SceneStateController {
        public const string PassageOpenKey = "passageOpen";
        private const string k_FriendshipStateID = "friendship";

        [Header("Friendship cutscene")]
        [SerializeField] private ArticyRef m_FriendshipDialogue;
        [SerializeField] private ChangeSoundtrackOnStart m_SoundtrackMng;
        [SerializeField] private AudioMusicObject m_FriendshipSoundtrack;
        [SerializeField] private SceneLoadTriggerWalkOut m_WalkOutTrigger;
        [SerializeField] private SceneLoadAnchorWalkIn m_WalkInAnchor;
        [SerializeField] private float m_FriendShipEndPosition;

        [Header("Secret Passage")]
        [SerializeField] private GameObject[] m_PassageOpenObjects;
        [SerializeField] private GameObject[] m_PassageClosedObjects;

        private bool _friendShipCutscene;

        public override void BeforeAnchors(SceneLoader.SceneLoadingHandler handler, List<SceneLoadAnchor> allAnchors, ref SceneLoadAnchor anchor) {
            base.BeforeAnchors(handler, allAnchors, ref anchor);

            bool matchFriendshipState = MatchState(handler, k_FriendshipStateID);

            if (firstTimeInScene && anchor == m_WalkInAnchor || matchFriendshipState) {
                _friendShipCutscene = true;
                SoundtrackManager.instance.SetSoundtrack(m_FriendshipSoundtrack);
                m_SoundtrackMng.enabled = false;

                m_WalkInAnchor.finalPositionX = m_FriendShipEndPosition;
                m_WalkInAnchor.onFinish.AddListener(() => {
                    GameCharactersManager.instance.bastheet.SetFacingDirection(false);
                });

                if (matchFriendshipState)
                    m_WalkInAnchor.onLoad?.Invoke(handler);
            }
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);

            var passageOpen = IsPassageOpen();
            foreach (var opnObj in m_PassageOpenObjects) opnObj.SetActive(passageOpen);
            foreach (var cldObj in m_PassageClosedObjects) cldObj.SetActive(!passageOpen);

            if (_friendShipCutscene) StartFriendshipCutscene(handler);
        }

        public bool IsPassageOpen() => GameKeysManager.instance.HaveGameKey(PassageOpenKey);

        private void StartFriendshipCutscene(SceneLoader.SceneLoadingHandler sceneHandler) {
            DataManager.instance.SaveCheckpoint(k_FriendshipStateID);

            sceneHandler.ResumeInput();
            var handler = DialogueManager.instance.PlayHandledDialogue(m_FriendshipDialogue);
            handler.onDialogueFinished += EndAmnesiaCatCutscene;
        }

        private void EndAmnesiaCatCutscene() {
            m_WalkOutTrigger.Trigger();
            DataManager.instance.ClearSave();
        }
    }
}