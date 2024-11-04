using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public abstract class GameTrigger : GameTriggerBase {
        [SerializeField] private string m_TriggerCode;
        public string triggerCode => m_TriggerCode;

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            bool equals = Match(id);

            if (equals) return DoLogic(handler);

            return equals;
        }

        protected virtual bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            return false;
        }

        public override bool Match(string id) => id == m_TriggerCode;
    }
}
