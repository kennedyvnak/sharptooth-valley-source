using System.Collections.Generic;
using UnityEngine;

namespace NFHGame.UI.Input {
    [RequireComponent(typeof(CanvasGroup))]
    public class UserInterfaceInput : Singleton<UserInterfaceInput> {
        [SerializeField] private CanvasGroup m_ButtonsGroup;
        public CanvasGroup buttonsGroup => m_ButtonsGroup;

        public CanvasGroup canvasGroup { get; private set; }

        public HashSet<int> enableBlocks { get; private set; }


        protected override void Awake() {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
            enableBlocks = new HashSet<int>();
        }

        public bool GetInteractable() => canvasGroup.interactable;

        public void SetInteractable(int key, bool interactable) {
            if (!interactable)
                enableBlocks.Add(key);
            else
                enableBlocks.Remove(key);

            canvasGroup.interactable = enableBlocks.Count == 0;
        }
    }
}