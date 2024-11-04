using System;
using NFHGame.Configs;
using NFHGame.Options;
using NFHGame.ScriptableSingletons;
using UnityEngine;
using UnityEngine.Audio;

namespace NFHGame.AudioManagement {
    public enum AudioGroupType { None, Master, Musics, Sounds }

    public class AudioManager : ScriptableSingleton<AudioManager>, IBootableSingleton {
        [SerializeField] private AudioMixer m_Mixer;
        [SerializeField] private string m_MasterVolumeParamName, m_MusicsVolumeParamName, m_SoundsVolumeParamName;
        [SerializeField] private AudioMixerGroup m_MasterGroup;
        [SerializeField] private AudioMixerGroup m_MusicsGroup;
        [SerializeField] private AudioMixerGroup m_SoundsGroup;

        public AudioMixer mixer => m_Mixer;
        public string masterVolumeParamName => m_MasterVolumeParamName;
        public string musicsVolumeParamName => m_MusicsVolumeParamName;
        public string soundsVolumeParamName => m_SoundsVolumeParamName;
        public AudioMixerGroup masterGroup => m_MasterGroup;
        public AudioMixerGroup musicsGroup => m_MusicsGroup;
        public AudioMixerGroup soundsGroup => m_SoundsGroup;

        public void LoadAudioPrefs() {
            var optionsManager = OptionsManager.instance;
            optionsManager.onFloatOptionChanged.AddListener(EVENT_OptionChanged);
            SetMixerVolumeParameter(m_MasterVolumeParamName, optionsManager.currentOptions.GetFloat(m_MasterVolumeParamName));
            SetMixerVolumeParameter(m_MusicsVolumeParamName, optionsManager.currentOptions.GetFloat(m_MusicsVolumeParamName));
            SetMixerVolumeParameter(m_SoundsVolumeParamName, optionsManager.currentOptions.GetFloat(m_SoundsVolumeParamName));
        }

        public static float CalculateAudio(float source) => source <= Mathf.Epsilon ? -80.0f : Mathf.Log10(source) * 20.0f;

        public void SetMixerVolumeParameter(string field, float volume) {
            mixer.SetFloat(field, CalculateAudio(volume));
        }

        public void SetVolumeForGroup(AudioGroupType group, float volume) {
            mixer.SetFloat(GetAudioMixerVolumeParameterName(group), CalculateAudio(volume));
        }

        public string GetAudioMixerVolumeParameterName(AudioGroupType group) => group switch {
            AudioGroupType.Master => masterVolumeParamName,
            AudioGroupType.Musics => musicsVolumeParamName,
            AudioGroupType.Sounds => soundsVolumeParamName,
            AudioGroupType.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
        };

        public AudioMixerGroup GetAudioMixerGroup(AudioGroupType group) => group switch {
            AudioGroupType.Master => masterGroup,
            AudioGroupType.Musics => musicsGroup,
            AudioGroupType.Sounds => soundsGroup,
            AudioGroupType.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
        };

        private void EVENT_OptionChanged(string key, float value) {
            if (key != m_MasterVolumeParamName && key != m_MusicsVolumeParamName && key != m_SoundsVolumeParamName) return;

            m_Mixer.SetFloat(key, CalculateAudio(value));
        }

        void IBootableSingleton.Initialize() {
            LoadAudioPrefs();
        }
    }
}