using NFHGame.DialogueSystem;
using NFHGame.Input;
using System;
using UnityEngine;

namespace NFHGame.Interaction.Behaviours {
    public class InteractionDisable : InteractionBehaviour {
        [System.Flags]
        public enum DisableFlags {
            OnDialogue = 1 << 0,
            OnInputNone = 1 << 1,
        }

        [SerializeField] private DisableFlags m_DisableFlags;

        private DisableFlags _currentFlags;

        private void OnEnable() {
            if (m_DisableFlags.HasFlag(DisableFlags.OnDialogue))
                DialogueManager.instance.DialogueToggled += EVENT_DialogueToggled;
            if (m_DisableFlags.HasFlag(DisableFlags.OnInputNone))
                InputReader.instance.MapToggled += EVENT_MapToggled;
            UpdateDisable();
        }

        private void OnDisable() {
            if (m_DisableFlags.HasFlag(DisableFlags.OnDialogue)) 
                if (DialogueManager.instance) DialogueManager.instance.DialogueToggled -= EVENT_DialogueToggled;
            if (m_DisableFlags.HasFlag(DisableFlags.OnInputNone))
                if (InputReader.instance) InputReader.instance.MapToggled -= EVENT_MapToggled;
            if (_currentFlags != 0)
                interactionObject.Enable();
        }

        protected override void BindEvents() { }

        protected override void UnbindEvents() { }

        private void UpdateDisable() {
            if (_currentFlags != 0) {
                interactionObject.Disable();
            } else {
                interactionObject.Enable();
            }
        }

        private void EVENT_DialogueToggled(bool dialogueEnabled) {
            ToggleFlag(DisableFlags.OnDialogue, dialogueEnabled);
        }

        private void EVENT_MapToggled(InputReader.InputMap map) {
            ToggleFlag(DisableFlags.OnInputNone, map == InputReader.InputMap.None);
        }

        private void ToggleFlag(DisableFlags flag, bool enabled) {
            var flags = _currentFlags;

            if (enabled)
                _currentFlags |= flag;
            else
                _currentFlags &= ~flag;

            if (_currentFlags != flags) {
                UpdateDisable();
            }
        }
    }
}
