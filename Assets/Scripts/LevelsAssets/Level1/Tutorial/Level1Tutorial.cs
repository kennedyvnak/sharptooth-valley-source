using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace NFHGame.Tutorial {
    public class Level1Tutorial : Singleton<Level1Tutorial> {
        [SerializeField] private float m_ToggleDuration;

        [SerializeField] private CanvasGroup m_InventoryGroup;

        [SerializeField] private CanvasGroup m_MovementGroup;
        [SerializeField] private Transform m_MovementFollow;
        [SerializeField] private Vector2 m_MovementOffset;

        [SerializeField] private CanvasGroup m_InteractGroup;
        [SerializeField] private Transform m_InteractFollow;
        [SerializeField] private Vector2 m_InteractOffset;

        [System.NonSerialized] public bool movementFollow;
        [System.NonSerialized] public bool interactionFollow;

        public void StartInventory() {
            m_InventoryGroup.gameObject.SetActive(true);
            m_InventoryGroup.ToggleGroupAnimated(true, 1.0f / 3.0f);
        }

        public void EndInventory() {
            m_InventoryGroup.ToggleGroupAnimated(false, 1.0f / 3.0f).onComplete += () => {
                m_InventoryGroup.gameObject.SetActive(false);
            };
        }

        public void StartMovement(System.Func<bool> active) {
            StartCoroutine(Follow(m_MovementGroup, m_MovementOffset, m_MovementFollow, active));
        }

        public void StartInteraction(System.Func<bool> active) {
            StartCoroutine(Follow(m_InteractGroup, m_InteractOffset, m_InteractFollow, active));
        }

        private IEnumerator Follow(CanvasGroup group, Vector2 offset, Transform target, System.Func<bool> active) {
            group.gameObject.SetActive(true);
            group.ToggleGroupAnimated(true, 1.0f / 3.0f);
            var rectTransform = (RectTransform)group.transform;

            while (active()) {
                Follow();
                yield return null;
            }

            Tween t = group.ToggleGroupAnimated(false, 1.0f / 3.0f);

            while (t.IsActive()) {
                Follow();
                yield return null;
            }

            group.gameObject.SetActive(false);

            void Follow() {
                rectTransform.position = (Vector2)target.position + offset;
            }
        }
    }
}
