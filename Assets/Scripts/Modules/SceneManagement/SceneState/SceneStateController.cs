using System.Collections.Generic;
using Articy.SharptoothValley.GlobalVariables;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.Characters;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Serialization;
using NFHGame.Serialization.States;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.SceneManagement.SceneState {
    public class SceneStateController : Singleton<SceneStateController> {
        [SerializeField] protected bool m_IsDangerZone;
        [SerializeField] private SceneStatesData m_StateData;

        [SerializeField] private UnityEvent m_FirstTimeInScene;
        [SerializeField] private UnityEvent m_AlreadyEnteredInScene;

        public bool isDangerZone => m_IsDangerZone;
        public SceneStatesData stateData => m_StateData;
        public bool firstTimeInScene { get; protected set; }

        protected override void Awake() {
            base.Awake();
            if (!m_StateData) return;

            firstTimeInScene = !GameKeysManager.instance.HaveGameKey(m_StateData.sceneKey);
            if (firstTimeInScene) {
                m_FirstTimeInScene?.Invoke();
                GameKeysManager.instance.ToggleGameKey(m_StateData.sceneKey, true);
            } else {
                m_AlreadyEnteredInScene?.Invoke();
            }

            if (!string.IsNullOrWhiteSpace(m_StateData.articyVariable))
                ArticyManager.instance.SetVariable(m_StateData.articyVariable, 2);
        }

        protected override void OnDestroy() {
            if (!m_StateData) return;
            if (!string.IsNullOrWhiteSpace(m_StateData.articyVariable) && ArticyManager.instance)
                ArticyManager.instance.SetVariable(m_StateData.articyVariable, 1);
            base.OnDestroy();
        }

        public virtual void StartControl(SceneLoader.SceneLoadingHandler handler) {
            ArticyVariables.globalVariables.gameState.dangerZone = m_IsDangerZone;
            GameManager.instance.playTimeCouting = true;

            if (handler.anchorID == SceneStatesData.StateAnchorID && string.IsNullOrWhiteSpace(DataManager.instance.gameData.state.id)) {
                handler.ResumeInput();
                var location = stateData.GetNearestLocation(DataManager.instance.gameData.state.endPosition);
                if (GameCharactersManager.instance)
                    GameCharactersManager.instance.SetPosition(location.positionX, location.facingRight);
            }
        }

        public virtual void BeforeAnchors(SceneLoader.SceneLoadingHandler handler, List<SceneLoadAnchor> allAnchors, ref SceneLoadAnchor anchor) {
        }

        public bool MatchState(SceneLoader.SceneLoadingHandler handler, string state) {
            var stateAuto = handler.anchorID.Equals(SceneStatesData.StateAnchorID);
            var gameData = DataManager.instance.gameData;
            var stateValid = !string.IsNullOrEmpty(gameData?.state?.id) && gameData.state.id.Equals(state);
            return stateAuto && stateValid;
        }
    }
}