using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class Level5Headache : GameTriggerBase {
        [SerializeField] private Animator m_Animator;

        public override bool Match(string id) {
            return id switch {
                "headacheStart" => true,
                "headacheStop" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            switch (id) {
                case "headacheStart":
                    m_Animator.Play("HeadacheR");
                    handler.onReturnToDialogue.Invoke();
                    return true;
                case "headacheStop":
                    m_Animator.Play("IdleRight");
                    handler.onReturnToDialogue.Invoke();
                    return true;
            }
            return false;
        }
    }
}   
