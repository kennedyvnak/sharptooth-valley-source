using System.Collections.Generic;
using NFHGame.DialogueSystem.Portraits;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.DialogueBoxes {
    public class PortraitDisplay : MonoBehaviour {
        [SerializeField] private Image m_PortraitPartPrefab;
        [SerializeField] private Image m_Portrait;
        [SerializeField] private Image m_Halo;
        [SerializeField] private bool m_IsInRight;

        public Image portrait => m_Portrait;

        private List<Image> _portraitParts = new List<Image>();

        public RectTransform rectTransform => (RectTransform)transform;

        public void SetPortrait(Portrait portrait, int direction) {
            m_Portrait.sprite = ((direction == 1 && portrait.facingRight) ? portrait.body : (portrait.flippedBody ? portrait.flippedBody : portrait.body));
            Vector2 size = m_Portrait.sprite.rect.size;
            rectTransform.sizeDelta = size;

            rectTransform.anchoredPosition = new Vector2(size.x * (m_IsInRight ? -0.5f : 0.5f), size.y * 0.5f);

            int pLen = portrait.parts.Length;
            for (int i = 0; i < Mathf.Max(pLen, _portraitParts.Count); i++) {
                if (i < pLen) {
                    if (_portraitParts.Count <= i) {
                        _portraitParts.Add(Instantiate(m_PortraitPartPrefab, m_Portrait.rectTransform));
                    }
                    var partImage = _portraitParts[i];
                    var part = portrait.parts[i];
                    partImage.sprite = part;
                    if (!part) {
                        partImage.enabled = false;
                    } else {
                        partImage.SetNativeSize();
                        partImage.rectTransform.anchoredPosition = portrait.offsetPerPart;
                        partImage.enabled = true;
                    }
                } else {
                    _portraitParts[i].enabled = false;
                }
            }

            for (int i = 0; i < portrait.order.Length; i++) {
                var partImage = _portraitParts[i];
                partImage.transform.SetSiblingIndex(portrait.order[i]);
            }

            m_Halo.gameObject.SetActive(portrait.actor == Actors.DialogueActor.Actor.Bastheet || portrait.actor == Actors.DialogueActor.Actor.Thinking);

            var scale = transform.localScale;
            scale.x = direction;
            transform.localScale = scale;
        }
    }
}