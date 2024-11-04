using System.Collections;
using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Options;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.Screens {
    public class OptionsScreen : Singleton<OptionsScreen>, IScreen {
        [SerializeField] private CanvasGroup m_OptionsGroup;
        [SerializeField] private GameObject m_SelectOnOpen;

        [Space]
        [SerializeField] private float m_VolumeDiffPlaySound = 0.075f;
        [SerializeField] private Slider m_GammaVolumeSlider, m_MasterVolumeSlider, m_MusicsVolumeSlider, m_SoundVolumeSlider;
        [SerializeField] private AudioSource m_SoundsTestSource;
        [SerializeField] private AudioObject m_TestSoundVolume;

        [Space]
        [SerializeField] private Button m_ResetOptionsButton, m_ReturnToMenuButton, m_CloseButton;

        [SerializeField] private bool m_PopThisOnExit;

        private bool _screenActive;
        bool IScreen.screenActive { get => _screenActive; set => _screenActive = value; }
        public bool poppedByInput => true;
        GameObject IScreen.selectOnOpen => m_SelectOnOpen;
        bool IScreen.dontSelectOnActive => false;

        private float _cachedSoundVolume;

        private void Start() {
            m_ReturnToMenuButton.onClick.AddListener(PerformMenuClick);
            m_CloseButton.onClick.AddListener(PerformCloseClick);
            m_ResetOptionsButton.onClick.AddListener(PerformResetOptions);

            _cachedSoundVolume = m_SoundVolumeSlider.value;
            m_SoundVolumeSlider.onValueChanged.AddListener((f) => {
                if (Mathf.Abs(f - _cachedSoundVolume) >= m_VolumeDiffPlaySound) {
                    m_TestSoundVolume.CloneToSource(m_SoundsTestSource);
                    m_SoundsTestSource.Play();
                    _cachedSoundVolume = f;
                }
            });

            m_ResetOptionsButton.SetNavigation(down: m_GammaVolumeSlider, up: m_SoundVolumeSlider, right: m_ReturnToMenuButton);
            m_ReturnToMenuButton.SetNavigation(down: m_GammaVolumeSlider, up: m_SoundVolumeSlider, left: m_ResetOptionsButton, right: m_CloseButton);
            m_CloseButton.SetNavigation(down: m_GammaVolumeSlider, up: m_SoundVolumeSlider, left: m_ReturnToMenuButton);
            m_GammaVolumeSlider.SetNavigation(up: m_ReturnToMenuButton, down: m_MasterVolumeSlider);
            m_MasterVolumeSlider.SetNavigation(up: m_GammaVolumeSlider, down: m_MusicsVolumeSlider);
            m_MusicsVolumeSlider.SetNavigation(up: m_MasterVolumeSlider, down: m_SoundVolumeSlider);
            m_SoundVolumeSlider.SetNavigation(up: m_MusicsVolumeSlider, down: m_ReturnToMenuButton);
        }

        private void PerformMenuClick() {
            if (!_screenActive) return;

            ScreenManager.instance.PopScreen();
        }

        private void PerformCloseClick() {
            if (!_screenActive) return;

            if (m_PopThisOnExit)
                ScreenManager.instance.PopScreen();
            else
                ScreenManager.instance.PopAll();
        }

        private void PerformResetOptions() {
            OptionsManager.instance.ResetOptions();
        }

        IEnumerator IScreen.OpenScreen() {
            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_OptionsGroup.ToggleScreen(true).WaitForCompletion();
        }

        IEnumerator IScreen.CloseScreen() {
            yield return m_OptionsGroup.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}