using Articy.SharptoothValley;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public abstract class GameTriggerBase : MonoBehaviour {
        public abstract bool Match(string id);
        public abstract bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id);
    }
}
