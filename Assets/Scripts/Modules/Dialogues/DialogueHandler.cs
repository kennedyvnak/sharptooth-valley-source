using System;
using Articy.Unity;

namespace NFHGame.DialogueSystem {
    public class DialogueHandler {
        public System.Action onDialogueStartDraw;
        public System.Action onDialogueFinishDraw;
        public System.Action onDialogueShowBranches;
        public System.Action<Branch> onDialogueSelectBranch;
        public System.Action<string> onDialogueProcessGameTrigger;
        public System.Action onDialogueFinished;
    }
}