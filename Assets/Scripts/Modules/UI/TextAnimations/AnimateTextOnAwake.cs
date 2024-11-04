using UnityEngine;

namespace NFHGame.DialogueSystem {
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class AnimateTextOnAwake : MonoBehaviour {
        private TextVertexAnimator _vertexAnimator;
        private TMPro.TMP_Text _text;

        private void Awake() {
            _text = GetComponent<TMPro.TMP_Text>();
        }

        private void Start() {
            _vertexAnimator = new TextVertexAnimator(_text);
            var commands = DialogueUtility.ProcessInputString(_text.text, out var processedMessage);
            StartCoroutine(_vertexAnimator.AnimateTextIn(commands, processedMessage, null));
        }
    }
}