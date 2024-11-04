using Cinemachine;
using NFHGame.Characters;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.PostProcessing;
using NFHGame.RangedValues;
using UnityEngine;
using UnityEngine.Rendering;

namespace NFHGame.LevelAssets.Level6.EpicEnding {
    public class EpicEndingSceneControl : MonoBehaviour {
        [Header("Scene")]
        [SerializeField] protected Transform m_CameraFocus;
        [SerializeField] protected VolumeProfile m_Profile;
        [SerializeField, RangedValue(-2.0f, 2.0f)] protected RangedFloat m_GammaRange;

        protected ComposedEpicEndingCutscene _epicEnding;
        protected CinemachineConfiner2D _cameraConfiner;
        protected Volume _volume;
        protected VolumeProfile _cachedProfiled;
        protected RangedFloat _cachedGamma;

        protected virtual void OnEnable() {
            _cameraConfiner.enabled = false;
            Helpers.vCam.Follow = m_CameraFocus;
            Helpers.vCam.PreviousStateIsValid = false;

            _cachedProfiled = _volume.profile;
            _cachedGamma = GammaController.instance.gammaRange;
            GammaController.instance.gammaRange = m_GammaRange;
            _volume.profile = m_Profile;
            _epicEnding.ToggleSceneLights(false);
        }

        protected virtual void OnDisable() {
            _cameraConfiner.enabled = true;

            Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
            Helpers.vCam.PreviousStateIsValid = false;

            _volume.profile = _cachedProfiled;
            GammaController.instance.gammaRange = _cachedGamma;
            _epicEnding.ToggleSceneLights(true);
        }

        public void Init(ComposedEpicEndingCutscene composedEpicEnding, CinemachineConfiner2D confiner, Volume volume) {
            _epicEnding = composedEpicEnding;
            _volume = volume;
            _cameraConfiner = confiner;
        }
    }
}
