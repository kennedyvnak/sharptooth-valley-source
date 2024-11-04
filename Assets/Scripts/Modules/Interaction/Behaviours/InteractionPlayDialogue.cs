using Articy.Unity;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.Actors;
using UnityEngine;
using static NFHGame.ArticyImpl.ArticyManager;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionPlayDialogue : InteractionBehaviour {
        [System.Serializable]
        public struct PortraitSide {
            public DialogueActor.Actor actor;
            public bool rightSide;
        }

        [SerializeField] private ArticyRef m_DialogueReference;
        [SerializeField] private bool m_LookBack;
        [SerializeField] private bool m_DisableRollback;

        [SerializeField] private DialogueHandlerCallbacks m_Callbacks;

        [SerializeField] private PortraitSide[] m_PortraitSides;

        public ArticyRef dialogueReference { get => m_DialogueReference; set => m_DialogueReference = value; }
        public DialogueHandlerCallbacks callbacks { get => m_Callbacks; set => m_Callbacks = value; }
        public bool lookBack { get => m_LookBack; set => m_LookBack = value; }

        protected override void BindEvents() {
            interactionObject.onInteract.AddListener(EVENT_Interact);
        }

        protected override void UnbindEvents() {
            interactionObject.onInteract.RemoveListener(EVENT_Interact);
        }

        private void EVENT_Interact(Interactor interactor) {
            var handler = DialogueManager.instance.CreateHandler();
            ToggleRollbackScope? rollback = m_DisableRollback ? new ToggleRollbackScope(false) : null;
            m_Callbacks.Connect(handler);

            if (m_PortraitSides != null && m_PortraitSides.Length > 0) {
                var overrideSide = DialogueManager.instance.executionEngine.overridePortraitsSide;
                foreach (var portraitSide in m_PortraitSides)
                    overrideSide[portraitSide.actor] = portraitSide.rightSide;
            }

            if (m_LookBack) {
                GameCharactersManager.instance.ToggleLookBack(true);
            }

            handler.onDialogueFinished += () => {
                if (m_LookBack)
                    GameCharactersManager.instance.ToggleLookBack(false);
                rollback?.Dispose();

                if (m_PortraitSides != null && m_PortraitSides.Length > 0) {
                    var overrideSide = DialogueManager.instance.executionEngine.overridePortraitsSide;
                    foreach (var portraitSide in m_PortraitSides)
                        overrideSide.Remove(portraitSide.actor);
                }
            };

            DialogueManager.instance.PlayHandler(m_DialogueReference, handler);
        }
    }
}
