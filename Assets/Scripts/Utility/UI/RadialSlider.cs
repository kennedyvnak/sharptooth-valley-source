using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadialSlider : MonoBehaviour, IDragHandler, IScrollHandler, IPointerClickHandler {
    public const float DegToNormal = 1.0f / 360.0f;

    [Header("Properties")]
    [SerializeField] private float m_Radius;
    [SerializeField] private float m_Steps;
    [SerializeField] private bool m_Active;
    [SerializeField] private bool m_Interact;
    [SerializeField] private UnityEvent<float> m_NormalizedValueChanged;

    [Header("Toggling")]
    [SerializeField] private GameObject[] m_InactiveObjects;
    [SerializeField] private Image m_Background;
    [SerializeField] private Color m_BackgroundActiveColor, m_BackgroundDeactiveColor;
    [SerializeField] private Image m_HandleImage;
    [SerializeField] private Color m_HandleImageInactive;

    public float steps { get => 360.0f / m_Steps; set => m_Steps = 360.0f / value; }
    public float radius { get => m_Radius; set => m_Radius = value; }
    public UnityEvent<float> normalizedValueChanged { get => m_NormalizedValueChanged; set => m_NormalizedValueChanged = value; }
    public float normalizedValue { get => _currentValue; set => SetCurrentVal(value); }
    public bool active { get => m_Active; set => SetActive(value); }
    public bool interact { get => m_Interact; set => SetInteract(value); }

    private float _currentValue;

    public void OnDrag(PointerEventData eventData) {
        if (!m_Active || !m_Interact) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out var localPos);
        float angle = Mathf.Atan2(-localPos.y, localPos.x) * Mathf.Rad2Deg + 180f;
        angle = Mathf.Clamp(Mathf.RoundToInt(angle / m_Steps) * m_Steps, 0.0f, 360.0f);

        normalizedValue = angle * DegToNormal;
    }

    public void OnScroll(PointerEventData eventData) {
        if (!m_Active || !m_Interact) return;

        float scroll = Mathf.Sign(eventData.scrollDelta.y);
        float val = normalizedValue + scroll / steps;
        if (val < 0.0f) val++;
        SetCurrentVal(val);
    }

    public void OnPointerClick(PointerEventData eventData) => OnDrag(eventData);

    private void SetCurrentVal(float value) {
        if (value >= 1.0f || value < 0.0f) _currentValue = 0.0f;
        else _currentValue = value;

        normalizedValueChanged?.Invoke(_currentValue);
        float radAngle = _currentValue * 2.0f * Mathf.PI;
        m_HandleImage.rectTransform.anchoredPosition = new Vector2(-Mathf.Cos(radAngle), Mathf.Sin(radAngle)) * m_Radius;
    }

    private void SetActive(bool active) {
        m_Active = active;
        foreach (var obj in m_InactiveObjects)
            obj.SetActive(active);
        m_Background.color = active ? m_BackgroundActiveColor : m_BackgroundDeactiveColor;
    }

    private void SetInteract(bool interact) {
        m_Interact = interact;
        m_HandleImage.color = interact ? Color.white : m_HandleImageInactive;
    }
}
