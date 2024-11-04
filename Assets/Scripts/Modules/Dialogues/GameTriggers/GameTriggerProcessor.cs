using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class GameTriggerProcessor : Singleton<GameTriggerProcessor> {
        public class GameTriggerHandler {
            public readonly GameTriggerProcessor gameTriggerProcessor;
            public UnityEngine.Events.UnityEvent onReturnToDialogue = new UnityEngine.Events.UnityEvent();
            public Articy.SharptoothValley.GameTrigger articyTrigger { get; set; }

            public GameTriggerHandler(GameTriggerProcessor gameTriggerProcessor) {
                this.gameTriggerProcessor = gameTriggerProcessor;
            }
        }

        private GameTriggerHandler _handler;

        protected override void Awake() {
            base.Awake();
            _handler = new GameTriggerHandler(this);
        }

        public GameTriggerHandler CreateHandler(Articy.SharptoothValley.GameTrigger trigger) {
            _handler.articyTrigger = trigger;
            _handler.onReturnToDialogue.RemoveAllListeners();
            return _handler;
        }

        public GameTriggerBase GetTrigger(string triggerID) {
            foreach (var child in GetComponentsInChildren<GameTriggerBase>()) {
                if (child.Match(triggerID)) {
                    return child;
                }
            }

            return null;
        }

        public bool ProcessGameTrigger(GameTriggerBase trigger, GameTriggerHandler handler, string triggerID) {
            return trigger.Process(_handler, triggerID);
        }
    }
}
