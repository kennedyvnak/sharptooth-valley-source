using Cinemachine;
using UnityEngine;

namespace NFHGame.Graphics {
    [RequireComponent(typeof(Camera))]
    public class CameraController : Singleton<CameraController> {
        [SerializeField] private Transform m_OverrideFollowOnStart;

        private Camera _camera;
        private CinemachineVirtualCamera _vCam;

#if UNITY_EDITOR
        public new Camera camera => _camera;
#else
        public Camera camera => _camera;
#endif

        public CinemachineVirtualCamera vCam => _vCam;

        protected override void Awake() {
            base.Awake();
            _camera = GetComponent<Camera>();
            _vCam = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        private void Start() {
            if (!_vCam) return;

            if (CameraBoundsInstance.instance)
                _vCam.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = CameraBoundsInstance.instance.boundingShape;
            else
                _vCam.GetComponent<CinemachineConfiner2D>().enabled = false;

            if (Characters.GameCharactersManager.instance)
                _vCam.Follow = Characters.GameCharactersManager.instance.bastheet.transform;

            if (m_OverrideFollowOnStart)
                _vCam.Follow = m_OverrideFollowOnStart;
        }
    }
}
