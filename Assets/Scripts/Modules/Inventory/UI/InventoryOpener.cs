using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.Inventory.UI {
    public class InventoryOpener : Singleton<InventoryOpener> {
        [SerializeField] private float m_ShakeDuration;
        [SerializeField] private float m_ShakeForce;
        [SerializeField] private int m_ShakeVibrato;
        [SerializeField] private float m_ShakeRandomness;
        [SerializeField] private ShakeRandomnessMode m_ShakeRandomnessMode;
        [SerializeField] private Button m_Button;

        private AudioPlayer _audioPlayer;

        public event System.Action OnShake;

        public Button button => m_Button;

        protected override void Awake() {
            base.Awake();
            _audioPlayer = GetComponent<AudioPlayer>();
        }

        public void OpenInventory() => ScreenManager.instance.PushScreen(InventoryManager.instance);

        public void ShakeBag() {
            var rectTransform = (RectTransform)transform;
            rectTransform.DOShakeRotation(m_ShakeDuration, new Vector3(0.0f, 0.0f, m_ShakeForce), m_ShakeVibrato, m_ShakeRandomness, randomnessMode: m_ShakeRandomnessMode);
            _audioPlayer.Play();
            OnShake?.Invoke();
        }
    }
}