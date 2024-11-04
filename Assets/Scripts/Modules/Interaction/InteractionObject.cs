using System;
using System.Collections.Generic;
using NFHGame.Interaction.Behaviours;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.Interaction {
    public class InteractionObject : MonoBehaviour {
        [System.Serializable]
        public class InteractorEvent : UnityEvent<Interactor> { }
        [System.Serializable]
        public class InteractorPointEvent : UnityEvent<InteractorPoint> { }

        [SerializeField] private InteractorEvent m_OnInteract;

        [SerializeField] private InteractorEvent m_OnInteractorEnter;
        [SerializeField] private InteractorEvent m_OnInteractorExit;

        [SerializeField] private InteractorPointEvent m_OnInteractorPointEnter;
        [SerializeField] private InteractorPointEvent m_OnInteractorPointExit;
        [SerializeField] private InteractorPointEvent m_OnInteractorPointClick;

        [SerializeField] private UnityEvent m_OnInteractionEnabled;
        [SerializeField] private UnityEvent m_OnInteractionDisabled;

        public InteractorEvent onInteract => m_OnInteract;
        public InteractorEvent onInteractorEnter => m_OnInteractorEnter;
        public InteractorEvent onInteractorExit => m_OnInteractorExit;
        public InteractorPointEvent onInteractorPointEnter => m_OnInteractorPointEnter;
        public InteractorPointEvent onInteractorPointExit => m_OnInteractorPointExit;
        public InteractorPointEvent onInteractorPointClick => m_OnInteractorPointClick;
        public UnityEvent onInteractionEnabled => m_OnInteractionEnabled;
        public UnityEvent onInteractionDisabled => m_OnInteractionDisabled;

        private Interactor _currentInteractor;
        public Interactor currentInteractor { get => _currentInteractor; internal set => _currentInteractor = value; }

        private InteractorPoint _currentPoint;
        public InteractorPoint currentPoint { get => _currentPoint; internal set => _currentPoint = value; }

        private InteractionObjectCollider _collider;
        private InteractionObjectPointCollider _pointCollider;
        private InteractionObjectPointHoverHighlight _pointHoverHighlight;

        private List<InteractionBehaviour> _behaviours;

#if UNITY_EDITOR
        public new InteractionObjectCollider collider => _collider;
#else
        public InteractionObjectCollider collider => _collider;
#endif
        public InteractionObjectPointCollider pointCollider => _pointCollider;
        public InteractionObjectPointHoverHighlight pointHoverHighlight => _pointHoverHighlight;
        public List<InteractionBehaviour> behaviours => _behaviours;

        private bool _behaviourEnabled = true;
        private bool _onDisableCall = false;

        public bool behaviourEnabled => _behaviourEnabled && enabled;

        private void Awake() {
            _collider = GetComponentInChildren<InteractionObjectCollider>();
            _pointCollider = GetComponentInChildren<InteractionObjectPointCollider>();
            _pointHoverHighlight = GetComponentInChildren<InteractionObjectPointHoverHighlight>();

            _behaviours = new List<InteractionBehaviour>();
            GetComponents(_behaviours);

            _collider.Register(this);
            _pointCollider.Register(this);
            if (_pointHoverHighlight)
                _pointHoverHighlight.Register(this);

            foreach (var behaviour in _behaviours) {
                behaviour.Init(this);
            }
        }

        private void OnEnable() {
            CheckEnable();
        }

        private void OnDisable() {
            DoDisable();
        }

        public void Toggle(bool active) {
            if (active)
                Enable();
            else
                Disable();
        }

        public void Disable() {
            DoDisable();
            _behaviourEnabled = false;
        }

        public void Enable() {
            _behaviourEnabled = true;
            CheckEnable();
        }

        private void CheckEnable() {
            if (behaviourEnabled)
                onInteractionEnabled?.Invoke();
        }

        private void DoDisable() {
            _onDisableCall = true;
            onInteractionDisabled?.Invoke();
            _onDisableCall = false;
            _currentInteractor = null;
            _currentPoint = null;
        }

        internal void TRIGGER_InteractorEnter(Interactor interactor) {
            _currentInteractor = interactor;
            if (behaviourEnabled) {
                onInteractorEnter?.Invoke(interactor);
            }
        }

        internal void TRIGGER_InteractorExit(Interactor interactor) {
            if (behaviourEnabled) {
                onInteractorExit?.Invoke(interactor);
            }
            _currentInteractor = null;
        }

        internal void TRIGGER_PointClick(InteractorPoint point) {
            if (behaviourEnabled) {
                onInteractorPointClick?.Invoke(point);
            }
        }

        internal void TRIGGER_InteractorPointEnter(InteractorPoint point) {
            _currentPoint = point;
            if (behaviourEnabled) {
                onInteractorPointEnter?.Invoke(point);
            }
        }

        internal void TRIGGER_InteractorPointExit(InteractorPoint point) {
            if (behaviourEnabled || _onDisableCall) {
                onInteractorPointExit?.Invoke(point);
            }
            _currentPoint = null;
        }

        public void Interact(Interactor interactor) {
            onInteract?.Invoke(interactor);
        }
    }
}
