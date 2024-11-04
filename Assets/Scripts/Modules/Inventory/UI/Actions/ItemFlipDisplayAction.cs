using System.Collections;
using Articy.Unity;
using Articy.Unity.Interfaces;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.DialogueSystem;
using UnityEngine;

namespace NFHGame.Inventory.UI.ItemActions {
    [CreateAssetMenu(menuName = "Scriptable/Inventory/Actions/Wanted Poster Flip Action")]
    public class ItemFlipDisplayAction : InventoryItemAction {
        public ArticyRef postDialogue;
        public Sprite sprite;

        public override bool IsValid() {
            var dialogueObj = postDialogue.GetObject();
            if (dialogueObj is IInputPinsOwner iPinOwner) {
                var pin = iPinOwner.GetInputPins()[0];
                if (!pin.Evaluate(DialogueManager.instance.executionEngine.flowPlayer.MethodProvider, ArticyVariables.globalVariables)) return false;
            }
            return true;
        }

        public override IEnumerator OnTrigger(ActionContext context) {
            context.itemDisplay.sprite = sprite;
            var handler = DialogueManager.instance.PlayHandledDialogue(postDialogue);
            bool finished = false;
            handler.onDialogueFinished += () => finished = true;
            while (!finished) {
                yield return null;
            }
            context.itemDisplay.sprite = context.item.display;
        }

        public override string GetLabel() {
            return postDialogue.HasReference ? postDialogue.GetObject().ExtractText() : label;
        }
    }
}