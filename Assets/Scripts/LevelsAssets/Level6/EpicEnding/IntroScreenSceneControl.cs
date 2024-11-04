using DG.Tweening;
using System;
using UnityEngine;

namespace NFHGame.LevelAssets.Level6.EpicEnding {
    public class IntroScreenSceneControl : EpicEndingSceneControl {
        [Serializable]
        public struct Movement {
            public float startFrom, endIn, duration;
            public Ease ease;
        }

        [SerializeField] private Transform m_CharactersTransform;
        [SerializeField] private Transform m_MovementTransform;

        [SerializeField] private float m_FloatingMaxY;
        [SerializeField] private float m_MoveSpeed;

        [Header("Move")]
        [SerializeField] private Movement m_MoveA;
        [SerializeField] private Movement m_MoveB;

        [SerializeField] private float m_OutsideShipFollowPos;
        [SerializeField] private TrailRenderer m_Trail;

        private void Update() {
            var pos = m_MovementTransform.localPosition;
            pos.y = PositiveSin(Time.time * m_MoveSpeed) * m_FloatingMaxY;
            m_MovementTransform.localPosition = pos;
        }

        public void SetOutsideShip() {
            {
                var pos = m_CameraFocus.position;
                pos.x = m_OutsideShipFollowPos;
                m_CameraFocus.position = pos;
                Helpers.vCam.PreviousStateIsValid = false;
            }
            {
                var pos = m_CharactersTransform.localPosition;
                pos.y = m_MoveA.startFrom;
                m_CharactersTransform.localPosition = pos;
            }

            m_CharactersTransform.DOLocalMoveY(m_MoveA.endIn, m_MoveA.duration).SetEase(m_MoveA.ease);
        }

        public Tweener OutsideShotOut() {
            m_Trail.gameObject.SetActive(true);
            return m_CharactersTransform.DOMoveY(m_MoveB.endIn, m_MoveB.duration).SetEase(m_MoveB.ease);
        }

        private float PositiveSin(float t) {
            return (1.0f + Mathf.Sin(t)) * 0.5f;
        }
    }
}
