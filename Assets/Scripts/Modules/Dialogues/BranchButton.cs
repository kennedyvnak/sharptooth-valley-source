using Articy.Unity;
using Articy.Unity.Interfaces;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem {
    public class BranchButton : Button, ISelectHandler, IDeselectHandler {
        [SerializeField] private Color m_DefaultColor, m_WasSelectedBeforeColor;
        [SerializeField] private RectTransform m_BaseLine;
        [SerializeField] private float m_BaseLineAnimDuration;
        [SerializeField] private float m_Border;
        [SerializeField] private float m_BorderWidth;
        [SerializeField] private Ease m_BaseLineAnimEase;

        private TMPro.TextMeshProUGUI _text;
        private Branch _branch;
        private Button _button;
        private System.Action<Branch> _branchSelected;
        private bool _selected;

        private bool _currentSelected;

        public event System.Action<BranchButton> OnMouseEnter;
        public Branch branch => _branch;
        public bool locked;

        private TweenerCore<Vector2, Vector2, VectorOptions> _baseLineTweener;

        protected override void Awake() {
            base.Awake();
            if (!Application.isPlaying) return;
            _text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnBranchSelected);
        }

        public void AssignBranch(Branch branch, string buttonLabel, bool wasSelectedBefore, bool locked, bool selected, System.Action<Branch> branchSelected) {
            _branch = branch;
            _branchSelected = branchSelected;
            _selected = selected;
            this.locked = locked;

            _text.color = wasSelectedBefore ? m_WasSelectedBeforeColor : m_DefaultColor;
            _text.text = locked ? $"<sprite=14>{buttonLabel}" : buttonLabel;
            _button.enabled = !locked;
        }

        private void OnBranchSelected() {
            _branchSelected?.Invoke(_branch);
        }

        public Vector2 UpdateSize() {
            _text.ForceMeshUpdate(true);
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = _text.GetRenderedValues() + new Vector2(m_BorderWidth, m_Border);
            _baseLineTweener?.Kill();
            return rectTransform.sizeDelta;
        }

        public void UpdateBaseLine() {
            m_BaseLine.sizeDelta = new Vector2(_selected ? ((RectTransform)transform).sizeDelta.x : 0.0f, 1.0f);
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            OnMouseEnter?.Invoke(this);
            BaseLineAnimTo(new Vector2(((RectTransform)transform).sizeDelta.x, 1.0f));
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnPointerExit(PointerEventData eventData) {
            if (!_currentSelected)
                BaseLineAnimTo(Vector2.up);
        }

        public override void OnSelect(BaseEventData eventData) {
            _currentSelected = true;
            BaseLineAnimTo(new Vector2(((RectTransform)transform).sizeDelta.x, 1.0f));
            base.OnSelect(eventData);
        }

        public override void OnDeselect(BaseEventData eventData) {
            _currentSelected = false;
            BaseLineAnimTo(Vector2.up);
            base.OnDeselect(eventData);
        }

        private void BaseLineAnimTo(Vector2 scale) {
            _baseLineTweener = m_BaseLine.DOSizeDelta(scale, m_BaseLineAnimDuration).SetEase(m_BaseLineAnimEase);
        }
    }
}