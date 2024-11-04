using NFHGame.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame {
    public class MapController : Singleton<MapController> {
        [SerializeField] private Vector2 m_ScaleFactor;
        [SerializeField] private float m_ArrowMoveSpeed;
        [SerializeField] private RectTransform m_BigMapTransform;
        [SerializeField] private ScrollRect m_ScrollRect;
        [SerializeField] private CanvasGroup m_Group;

        [SerializeField] private RawImage m_SmallMap;
        [SerializeField] private RawImage m_BigMap;
        [SerializeField] private CanvasGroup m_LargeMapInterface;

        [Header("Tips")]
        [SerializeField] private TextMeshProUGUI m_SmallTip;
        [SerializeField] private TextMeshProUGUI m_BigTip;

        private bool _smallMap = true;

        private bool _smallTip = true;
        private int _bigTip = 2;

        private bool _active = false;

        private Vector2 _axisMove;
        private System.Action _onClose;

        private void Update() {
            if (_active && !_smallMap) {
                m_ScrollRect.horizontalNormalizedPosition += _axisMove.x * Time.deltaTime * m_ArrowMoveSpeed;
                m_ScrollRect.verticalNormalizedPosition += _axisMove.y * Time.deltaTime * m_ArrowMoveSpeed;
            }
        }

        public void ShowMap(System.Action onClose = null) {
            m_SmallMap.texture = Resources.Load<Texture2D>("MAP_Iridia_GAME_Small");
            m_BigMap.texture = Resources.Load<Texture2D>("MAP_Iridia_GAME_Big");

            _smallTip = true;
            _bigTip = 2;
            _axisMove = Vector2.zero;
            _onClose = onClose;

            Toggle(true);
            _active = true;

            m_Group.ToggleGroup(true);
            transform.GetChild(0).gameObject.SetActive(true);
            InputReader.instance.OnNavigate += INPUT_OnNavigate;
        }

        public void CloseMap() {
            InputReader.instance.OnNavigate -= INPUT_OnNavigate;
            _active = false;
            m_Group.ToggleGroup(false);
            transform.GetChild(0).gameObject.SetActive(false);
            _onClose?.Invoke();

            var smallTex = m_SmallMap.texture;
            var bigTex = m_SmallMap.texture;
            m_SmallMap.texture = null;
            m_BigMap.texture = null;
            Resources.UnloadAsset(smallTex);
            Resources.UnloadAsset(bigTex);
        }

        public void PointClick(BaseEventData eventData) {
            if (!_smallMap) return;

            var pos = (eventData as PointerEventData).position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_SmallMap.rectTransform, pos, Camera.main, out var localPoint);
            m_BigMapTransform.anchoredPosition = localPoint * m_ScaleFactor;

            Toggle(false);

            _smallTip = false;
            m_SmallTip.gameObject.SetActive(false);
        }

        public void ReturnSmallMap() => Toggle(true);

        public void OnScrollChanged(Vector2 value) {
            if (_smallMap) return;
            _bigTip--;
            if (_bigTip == 0)
                m_BigTip.gameObject.SetActive(false);
        }

        private void Toggle(bool smallMap) {
            _smallMap = smallMap;
            m_LargeMapInterface.ToggleGroup(!smallMap);
            m_BigMap.enabled = !smallMap;
            m_SmallMap.enabled = smallMap;

            if (!smallMap && _bigTip > 0) {
                m_BigTip.gameObject.SetActive(true);
            } else if (smallMap && _smallTip) {
                m_SmallTip.gameObject.SetActive(true);
            }
        }

        private void INPUT_OnNavigate(Vector2 nav) {
            _axisMove = nav;
        }
    }
}
