using UnityEngine;
using NFHGame.DialogueSystem.Portraits;
using TMPro;
using NFHGame.ArticyImpl.Variables;

namespace NFHGame.DialogueSystem.Actors {
    [CreateAssetMenu(menuName = "Scriptable/Dialogue/Actor")]
    public class DialogueActor : ScriptableObject {
        public enum Actor { Bastheet = 0, Dinner = 1, Crystal = 2, SilhouetteDinner = 3, Spammy = 4, Tyranx = 5, Narrator = 6, Dragon = 7, Arken = 8, LynnJournal = 9, Thinking = 10, SilhouetteSpammy = 11 }

        public string articyTechName;
        public string actorName;
        public SerializedDictionary<string, string> nameRevealVariable;
        public bool hasDefaultPortrait;
        public Actor actor;
        public TMP_FontAsset dialogueFont;

        public PortraitCollection portraitCollection;

        public string GetName() {
            if (nameRevealVariable.Count > 0) {
                foreach (var kvp in nameRevealVariable) {
                    if (ArticyVariables.globalVariables.GetVariableByString<bool>(kvp.Key))
                        return kvp.Value;
                }
                return "??????";
            }

            return actorName;
        }

        public Portrait GetDefaultPortrait() {
            return portraitCollection.GetPortrait("default", actor);
        }
    }
}