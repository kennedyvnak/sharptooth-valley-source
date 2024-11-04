using System.Collections;
using Articy.Unity;
using NFHGame.ArticyImpl;
using NFHGame.DialogueSystem;
using NFHGame.Serialization;
using TMPro;
using UnityEngine;

namespace NFHGame.Inventory.UI.ItemActions {
    [CreateAssetMenu(menuName = "Scriptable/Inventory/Actions/Praytos Book Read Passage Action")]
    public class PraytosBookReadPassageAction : InventoryItemAction {
        public string anotherPassage;
        public ArticyRef startDialogue;
        public ArticyRef[] passages;

        public override bool IsValid() => startDialogue.HasReference && startDialogue.ValidStart();

        public override IEnumerator OnTrigger(ActionContext context) {
            var handler = DialogueManager.instance.PlayHandledDialogue(startDialogue);
            var passage = DataManager.instance.gameData.readPassage;
            DialogueManager.instance.executionEngine.EnqueueDialogue(passages[passage]);
            bool finished = false;
            handler.onDialogueFinished += () => finished = true;
            while (!finished) yield return null;
            passage++;
            if (passage >= passages.Length) passage = 0;
            DataManager.instance.gameData.readPassage = passage;
            context.button.GetComponentInChildren<TextMeshProUGUI>().text = anotherPassage;
        }

        public override string GetLabel() {
            return DataManager.instance.gameData.readPassage > 0 ? anotherPassage : startDialogue.HasReference ? startDialogue.GetObject().ExtractText() : label;
        }
    }
}