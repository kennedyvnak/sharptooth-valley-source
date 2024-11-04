using UnityEngine;
using UnityEngine.InputSystem;

namespace NFHGame.Input {
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : SingletonPersistent<InputManager> {
        [SerializeField] private SerializedDictionary<string, Sprite> m_KeyboardBinding;
        [SerializeField] private SerializedDictionary<string, Sprite> m_MouseBinding;

        private PlayerInput _playerInput;
        public PlayerInput playerInput => _playerInput;

        protected override void Awake() {
            base.Awake();
            _playerInput = GetComponent<PlayerInput>();
        }

        public Sprite GetBindingIcon(string controlPath) {
            if (!m_KeyboardBinding.TryGetValue(controlPath, out var sprite))
                m_MouseBinding.TryGetValue(controlPath, out sprite);
            return sprite;
        }
    }
}