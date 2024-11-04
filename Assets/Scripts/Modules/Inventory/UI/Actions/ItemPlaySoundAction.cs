using System.Collections;
using Articy.Unity;
using NFHGame.ArticyImpl;
using NFHGame.AudioManagement;
using NFHGame.DialogueSystem;
using NFHGame.Inventory.UI;
using UnityEngine;

namespace NFHGame {
    [CreateAssetMenu(menuName = "Scriptable/Inventory/Actions/Item Play Sound Action")]
    public class ItemPlaySoundAction : InventoryItemAction {
        public ArticyRef postDialogue;
        public AudioMusicObject sound;

        public override IEnumerator OnTrigger(ActionContext context) {
            var sourceHandler = AudioPool.instance.PlaySound(sound);
            bool released = false;
            sourceHandler.onRelease.AddListener(() => released = true);
            yield return new WaitUntil(() => released);

            var handler = DialogueManager.instance.PlayHandledDialogue(postDialogue);
            bool finished = false;
            handler.onDialogueFinished += () => finished = true;
            yield return new WaitUntil(() => finished);
        }

        public override string GetLabel() {
            return postDialogue.HasReference ? postDialogue.GetObject().ExtractText() : label;
        }
    }
}
