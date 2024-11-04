using DG.Tweening;
using NFHGame.HaloManager;
using NFHGame.SceneManagement.GameKeys;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class DoubleHalo : GameTrigger {
        [Header("Grow")]
        [SerializeField] private float m_GrowTime;
        [SerializeField] private float m_GrowMoveTime;
        [SerializeField] private float m_GrowPositionOffsetY;

        [Header("Move")]
        [SerializeField] private float m_MoveDelay;
        [SerializeField] private float m_MoveTime;
        [SerializeField] private Vector2 m_EndPosition;

        [Header("Object")]
        [SerializeField] private Transform m_HaloReference;
        [SerializeField] private Transform m_DoubleHalo;
        [SerializeField] private Light2D[] m_HaloLights;

        private Vector2[] _lightRanges;

        private void OnDestroy() {
            this.DOKill();
        }

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            HaloManager.HaloManager.instance.Toggle(true);

            _lightRanges = new Vector2[m_HaloLights.Length];
            for (int i = 0; i < m_HaloLights.Length; i++) {
                Light2D haloLight = m_HaloLights[i];
                _lightRanges[i] = new Vector2(haloLight.pointLightInnerRadius, haloLight.pointLightOuterRadius);
                haloLight.pointLightInnerRadius = 0.0f;
                haloLight.pointLightOuterRadius = 0.0f;
            }

            m_DoubleHalo.transform.position = m_HaloReference.position;
            m_DoubleHalo.gameObject.SetActive(true);

            DOVirtual.Float(0.0f, 1.0f, m_GrowTime, (t) => {
                for (int i = 0; i < m_HaloLights.Length; i++) {
                    Light2D haloLight = m_HaloLights[i];
                    var range = _lightRanges[i];
                    haloLight.pointLightInnerRadius = range.x * t;
                    haloLight.pointLightOuterRadius = range.y * t;
                }
            }).SetTarget(this).OnComplete(() => {
                m_DoubleHalo.DOMove(m_EndPosition, m_MoveTime).SetDelay(m_MoveDelay).SetTarget(this).OnComplete(() => {
                    GameKeysManager.instance.ToggleGameKey("Checkpoint_1e1_2", true);
                    handler.onReturnToDialogue.Invoke();
                });
            });

            m_DoubleHalo.DOMoveY(m_DoubleHalo.transform.position.y + m_GrowPositionOffsetY, m_GrowMoveTime).SetTarget(this);

            return true;
        }
    }
}
