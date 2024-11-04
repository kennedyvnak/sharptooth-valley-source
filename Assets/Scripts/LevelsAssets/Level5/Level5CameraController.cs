using NFHGame.Input;
using NFHGame.RangedValues;
using NFHGame.UI;
using NFHGame.UI.Input;
using UnityEngine;

namespace NFHGame.LevelAssets.Level5 {
    public class Level5CameraController : Singleton<Level5CameraController> {
        [SerializeField] private Camera m_Camera;
        [SerializeField] private float m_ScrollFactor;
        [SerializeField] private GameObject m_ScrollUp;
        [SerializeField] private GameObject m_ScrollDown;

        [SerializeField] private float m_FadeDuration;

        public RangedFloat cameraRange { get; private set; }
        public bool controlCamera { get; private set; }
        public bool started { get; private set; }

        public float scrollSpeed { get; set; }
        public float inputScrollSpeed { get; set; }


        private Level5FrameControl _currentFrame;

        public event System.Action<Level5FrameControl> OnSetFrame;

        private void OnEnable() {
            InputReader.instance.OnVerticalAxis += INPUT_OnVerticalAxis;
        }

        private void OnDisable() {
            InputReader.instance.OnVerticalAxis -= INPUT_OnVerticalAxis;
        }

        private void Start() {
            Transform buttonsParent = UserInterfaceInput.instance.transform.GetChild(0);
            Transform scrollParent = m_ScrollUp.transform.parent;
            scrollParent.SetParent(buttonsParent);
            scrollParent.SetAsFirstSibling();
        }

        private void Update() {
            if (!controlCamera) return;

            var sp = Mathf.Clamp(scrollSpeed + inputScrollSpeed, -1, 1);
            if (Mathf.Abs(sp) - 0.2f > 0.0f) {
                Move(sp);
            }
        }

        public void StartFrame(Level5FrameControl frame) {
            if (started) return;

            cameraRange = frame.cameraRangeY;
            controlCamera = frame.controlCamera;
            SetPosition(frame.startCameraY);

            _currentFrame = frame;
            _currentFrame.toggled.Invoke(true);

            started = true;
        }

        public void SetFrame(Level5FrameControl frame) {
            InputReader.instance.PushMap(InputReader.InputMap.None);
            var fadeHandler = FadeScreen.instance.FadeFor(m_FadeDuration);
            fadeHandler.onFinishFadeIn += () => {
                cameraRange = frame.cameraRangeY;
                controlCamera = frame.controlCamera;
                SetPosition(frame.startCameraY);

                _currentFrame.toggled.Invoke(false);
                _currentFrame = frame;
                _currentFrame.toggled.Invoke(true);

                OnSetFrame?.Invoke(frame);

                fadeHandler.onFinishFadeOut += () => {
                    InputReader.instance.PopMap(InputReader.InputMap.None);
                };

                fadeHandler.FadeOut();
            };
        }

        public void Move(float dir) {
            SetPosition(m_Camera.transform.position.y + dir * m_ScrollFactor * Time.deltaTime);
        }

        public void SetPosition(float positionY) {
            var pos = m_Camera.transform.position;
            pos.y = UpdateScroll(positionY);
            m_Camera.transform.position = pos;
        }

        private float UpdateScroll(float positionY) {
            bool scrollUp = controlCamera;
            bool scrollDown = controlCamera;

            if (controlCamera) {
                if (positionY >= cameraRange.max) {
                    positionY = cameraRange.max;
                    scrollSpeed = 0.0f;
                    scrollUp = false;
                } else if (positionY <= cameraRange.min) {
                    positionY = cameraRange.min;
                    scrollSpeed = 0.0f;
                    scrollDown = false;
                }
            }

            m_ScrollUp.SetActive(scrollUp);
            m_ScrollDown.SetActive(scrollDown);

            return positionY;
        }

        private void INPUT_OnVerticalAxis(int direction) {
            inputScrollSpeed = direction;
        }
    }
}
