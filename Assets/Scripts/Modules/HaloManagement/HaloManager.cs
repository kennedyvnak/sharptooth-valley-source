using NFHGame.Input;
using NFHGame.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.HaloManager {
    public class HaloManager : SingletonPersistent<HaloManager> {
        [SerializeField] private UnityEvent<bool> m_HaloToggled;
        public UnityEvent<bool> haloToggled => m_HaloToggled;
        [SerializeField] private UnityEvent<bool> m_HaloForceToggled;
        public UnityEvent<bool> haloForceToggled => m_HaloForceToggled;
        [SerializeField] private UnityEvent<bool> m_HaloBlackoutToggled;
        public UnityEvent<bool> haloBlackoutToggled => m_HaloBlackoutToggled;

        private bool _haloActive;
        private bool _blackout;
        public bool haloActive { get => IsActive(); set => SetActive(value); }
        public bool blackout { get => _blackout && ArticyImpl.Variables.ArticyVariables.globalVariables.gameState.blackout; set => SetBlackout(value); }

        protected override void Awake() {
            base.Awake();
            if (DataManager.instance.gameData != null)
                ForceToggle(DataManager.instance.gameData.haloActive);
            DataManager.instance.afterDeserializeGameData.AddListener(EVENT_DeserializeGameData);
        }

        private void OnEnable() {
            InputReader.instance.OnToggleHalo += Toggle;
        }

        private void OnDisable() {
            InputReader.instance.OnToggleHalo -= Toggle;
        }

        public bool IsActive() => _haloActive;

        public void SetActive(bool value) {
            if (blackout) return;
            if (value == haloActive) return;

            DataManager.instance.gameData.haloActive = value;
            ArticyImpl.Variables.ArticyVariables.globalVariables.gameState.haloOn = value;
            _haloActive = value;
            haloToggled?.Invoke(value);
        }

        public void Toggle() => haloActive = !haloActive;

        public void Toggle(bool active) => haloActive = active;

        public void ForceToggle(bool active) {
            if (blackout) return;
            if (active == haloActive) return;

            DataManager.instance.gameData.haloActive = active;
            ArticyImpl.Variables.ArticyVariables.globalVariables.gameState.haloOn = active;
            _haloActive = active;
            haloForceToggled?.Invoke(active);
        }

        public void SetBlackout(bool isBlackout) {
            _blackout = isBlackout;

            if (blackout && haloActive) {
                DataManager.instance.gameData.haloActive = false;
                ArticyImpl.Variables.ArticyVariables.globalVariables.gameState.haloOn = false;
                _haloActive = false;
                haloToggled?.Invoke(false);
            }

            haloBlackoutToggled?.Invoke(_blackout);
        }

        public void ProcessArticyTrigger(string triggerCode) {
            if (Helpers.StringHelpers.StartsWith(triggerCode, "setHalo=")) {
                var valueString = triggerCode.Remove(0, 8);
                if (bool.TryParse(valueString, out bool enabled)) {
                    haloActive = enabled;
                } else {
                    GameLogger.articy.LogWarning($"{triggerCode} ({valueString}) isn't a valid trigger.");
                }
            }
        }

        private void EVENT_DeserializeGameData(GameData gameData) {
            ForceToggle(DataManager.instance.gameData.haloActive);
        }
    }
}