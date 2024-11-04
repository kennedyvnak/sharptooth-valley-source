using System;
using System.Collections.Generic;
using NFHGame;
using NFHGame.Input;
using NFHGame.SceneManagement;
using NFHGame.Serialization;
using NFHGame.Serialization.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGameTests {
    public class SelectGameState : MonoBehaviour {
        [SerializeField] private SerializedDictionary<string, GameDataAsset> m_States;
        [SerializeField] private Button m_ButtonPrefab;
        [SerializeField] private Transform m_ButtonsParent;

        private void Start() {
            foreach (var state in m_States) {
                var button = Instantiate(m_ButtonPrefab, m_ButtonsParent);
                button.onClick.AddListener(() => ButtonClicked(state));
                button.GetComponentInChildren<TextMeshProUGUI>().text = state.Key;
            }
        }

        private void ButtonClicked(KeyValuePair<string, GameDataAsset> state) {
            DataManager.instance.dataHandler.Serialize(state.Value.GenerateNew(1 << 8), 1 << 8);
            DataManager.instance.userManager.SetUser(1 << 8);
            var dtState = DataManager.instance.gameData.state;
            var handler = SceneLoader.instance.CreateHandler(dtState.sceneRef, SceneStatesData.StateAnchorID);
            handler.saveGame = false;
            handler.blackScreen = true;
            InputReader.instance.PopMap();
            InputReader.instance.PopMap();
            InputReader.instance.PopMap();
            InputReader.instance.PopMap();
            InputReader.instance.PushMap(InputReader.InputMap.Gameplay | InputReader.InputMap.UI);
            SceneLoader.instance.LoadScene(handler);
        }
    }
}