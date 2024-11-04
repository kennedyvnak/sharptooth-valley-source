using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.Inventory.UI;
using System.Collections;
using UnityEngine;

namespace NFHGame {
    [CreateAssetMenu(menuName = "Scriptable/Inventory/Actions/Open Crystal Box Action")]
    public class OpenCrystalBoxAction : InventoryItemAction {
        public Sprite displayB;
        public Sprite thumbB;

        public Sprite displayC;
        public Sprite thumbC;

        public override IEnumerator OnTrigger(ActionContext context) {
            if (ComposedAloneInTheDark.instance && ComposedAloneInTheDark.instance.handler != null) {
                ComposedAloneInTheDark.instance.StoreCrystalBoxThumbContext(this, context);
            }
            yield break;
        }
    }
}
