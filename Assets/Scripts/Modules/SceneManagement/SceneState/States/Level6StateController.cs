using Articy.Unity;
using NFHGame.ArticyImpl.Variables;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.SceneManagement.GameKeys;
using System.Collections.Generic;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class Level6StateController : SceneStateController {
        public const string k_RessurectDragonGameKey = "ressurectDragon_Mediator";
        [SerializeField] private ArticyRef m_FirstEnterDialogue;
        [SerializeField] private GameObject m_MediatorTyranx;
        [SerializeField] private GameObject m_AwakenTyranx;

        [SerializeField] private float m_DinnerLimits;
        [SerializeField] private float m_SpammyLimits;

        public override void BeforeAnchors(SceneLoader.SceneLoadingHandler handler, List<SceneLoadAnchor> allAnchors, ref SceneLoadAnchor anchor) {
            base.BeforeAnchors(handler, allAnchors, ref anchor);

            bool theMeditator = ArticyVariables.globalVariables.secrets.theMeditator;
            bool ressurectedDragon = ArticyVariables.globalVariables.gameState.RessurectedDragon;
            bool ressurectedDialogue = theMeditator && ressurectedDragon && !GameKeysManager.instance.HaveGameKey(k_RessurectDragonGameKey);

            bool meditating = !GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey);
            GameObject enable = meditating ? m_MediatorTyranx : m_AwakenTyranx;
            GameObject disable = !meditating ? m_MediatorTyranx : m_AwakenTyranx;
            enable.SetActive(true);
            disable.SetActive(false);

            if ((ressurectedDialogue || firstTimeInScene) && anchor is SceneLoadAnchorWalkIn walkIn) {
                walkIn.onFinish.AddListener(() => {
                    var handler = DialogueManager.instance.PlayHandledDialogue(m_FirstEnterDialogue);
                    if (ressurectedDialogue)
                        GameKeysManager.instance.ToggleGameKey(k_RessurectDragonGameKey, true);

                    if (meditating)
                        SetFollowersLimits();
                });
            }

            if (meditating)
                SetFollowersLimits();
        }

        private void SetFollowersLimits() {
            var dinner = GameCharactersManager.instance.dinner;
            dinner.stateMachine.EnterState(dinner.stateMachine.followLimitedState);
            dinner.stateMachine.followLimitedState.limitLeft = m_DinnerLimits;

            var spammy = GameCharactersManager.instance.spammy;
            spammy.stateMachine.EnterState(spammy.stateMachine.followLimitedState);
            spammy.stateMachine.followLimitedState.limitLeft = m_SpammyLimits;
        }
    }
}
