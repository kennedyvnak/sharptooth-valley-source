using Articy.SharptoothValley;
using Articy.Unity;
using Articy.Unity.Interfaces;
using NFHGame.ArticyImpl.Variables;
using NFHGame.DialogueSystem;

namespace NFHGame.ArticyImpl {
    public static class ArticyUtility {
        public static string ExtractText(this IFlowObject aObject) {
            if (aObject is IObjectWithText objectWithText) {
                return objectWithText.Text;
            }

            return null;
        }

        public static bool ValidStart(this ArticyRef aRef) {
            var methodProvider = DialogueManager.instance ? DialogueManager.instance.executionEngine.flowPlayer.MethodProvider : null;
            var globalVars = ArticyVariables.globalVariables;

            var dialogueObj = aRef.GetObject();
            if (dialogueObj is IInputPinsOwner iPinOwner) {
                var pin = iPinOwner.GetInputPins()[0];
                return pin.Evaluate(methodProvider, globalVars);
            } else return false;
        }
    }
}