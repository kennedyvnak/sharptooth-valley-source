using System;
using NFHGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGameTests {
    public class PortraitsPage : MonoBehaviour {
        [SerializeField] private CanvasGroup[] m_Pages;
        [SerializeField] private TextMeshProUGUI m_PageText;

        private int _currentIndex;

        private void Start() {
            SelectPage(0);
        }

        public void PassPage() {
            SelectPage(_currentIndex + 1);
        }

        private void SelectPage(int value) {
            if (value >= m_Pages.Length)
                value = 0;
            _currentIndex = value;

            m_PageText.text = $"{_currentIndex + 1}/{m_Pages.Length}";
            for (int i = 0; i < m_Pages.Length; i++) {
                m_Pages[i].ToggleGroup(i == _currentIndex);
            }
        }
    }
}
