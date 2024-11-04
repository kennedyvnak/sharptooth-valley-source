using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NFHGame {
    public class InputActionLabelGroup : MonoBehaviour {
        [SerializeField] private float m_Spacing;
        [SerializeField] private bool m_SetSize;

        private IEnumerator Start() {
            transform.GetChild(0).GetComponent<InputActionLabel>().onUpdateDisplay.AddListener(EVENT_UpdateDisplay);
            yield return null;
            UpdateLayout();
        }

        private void EVENT_UpdateDisplay(Sprite arg0) {
            StartCoroutine(Helpers.DelayForFramesCoroutine(1, UpdateLayout));
        }

        private void UpdateLayout() {
            float x = 0.0f;
            float y = 0.0f;
            foreach(RectTransform child in transform) {
                var pos = child.anchoredPosition;
                pos.x = x;
                child.anchoredPosition = pos;
                x += child.sizeDelta.x + m_Spacing;
                y = Mathf.Max(y, child.sizeDelta.y);
            }
            x -= m_Spacing;
            ((RectTransform)transform).sizeDelta = new Vector2(x, y);
        }
    }
}
