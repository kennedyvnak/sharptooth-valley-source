using DG.Tweening;
using NFHGame.Inventory.UI;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.Screens;
using NFHGame.Serialization;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.AchievementsManagement.UI {
    public class AchievementsScreen : Singleton<AchievementsScreen>, IScreen {
        [System.Serializable]
        public class MapIcon {
            public GameObject[] displays;
            public Expression expression;
        }

        [Header("UI")]
        [SerializeField] private CanvasGroup m_AchievementsGroup;
        [SerializeField] private GameObject m_SelectOnOpen;
        [SerializeField] private Button m_ReturnToInventoryButton, m_CloseButton;

        [Header("Icons")]
        [SerializeField] private MapIcon[] m_Icons;

        [Header("Achievements")]
        [SerializeField] private TextMeshProUGUI m_AchievementDescription;
        [SerializeField] private RectTransform m_AchievementsButtonParent;
        [SerializeField] private ScrollRect m_AchievementsScrollRect;
        [SerializeField] private AchievementButton m_AchievementButtonPrefab;
        [SerializeField] private float m_FocusSpeed;

        private AchievementButton[] _buttons;
        private AchievementObject _activeAchievement;
        private Coroutine _focusInThumbCoroutine;

        bool IScreen.screenActive { get; set; }
        public bool poppedByInput => true;
        GameObject IScreen.selectOnOpen => GetFirstValidThumb();
        bool IScreen.dontSelectOnActive => false;

        protected override void Awake() {
            base.Awake();
            var achievements = AchievementsManager.instance.GetAchievements();
            _buttons = new AchievementButton[achievements.Length];
            for (int i = 0; i < achievements.Length; i++) {
                AchievementObject achievement = achievements[i];
                var button = Instantiate(m_AchievementButtonPrefab, m_AchievementsButtonParent);
                _buttons[i] = button;
                button.Init();
                button.onButtonClick.AddListener(ButtonClicked);
                button.achievement = achievement;
                bool hasAchievement = GameKeysManager.instance.HaveGameKey(achievement.achievementGameKey);
                button.text.text = achievement.GetName(hasAchievement);
                button.button.interactable = hasAchievement;
            }
        }

        private void Start() {
            m_ReturnToInventoryButton.onClick.AddListener(PerformReturnToInventory);
            m_CloseButton.onClick.AddListener(PerformClose);

            ToggleIcons();
        }

        private void PerformReturnToInventory() {
            if (!((IScreen)this).screenActive) return;

            ScreenManager.instance.PopScreen();
        }

        private void PerformClose() {
            if (!((IScreen)this).screenActive) return;

            ScreenManager.instance.PopAll();
        }

        IEnumerator IScreen.OpenScreen() {
            ToggleIcons();
            if (EventSystem.current.currentSelectedGameObject.TryGetComponent<AchievementButton>(out var achievement))
                SetAchievement(achievement, true);
            transform.GetChild(0).gameObject.SetActive(true);
            yield return m_AchievementsGroup.ToggleScreen(true).WaitForCompletion();
        }

        IEnumerator IScreen.CloseScreen() {
            yield return m_AchievementsGroup.ToggleScreen(false).WaitForCompletion();
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void UpdateAchievement(AchievementObject achievement, bool enabled) {
            var button = _buttons[System.Array.IndexOf(AchievementsManager.instance.GetAchievements(), achievement)];
            button.text.text = button.achievement.GetName(enabled);
            button.button.interactable = enabled;
        }

        public void SetAchievement(AchievementButton button, bool force = false) {
            var achievement = button ? button.achievement : null;
            if (achievement && _activeAchievement == achievement) return;

            bool hasItem = achievement;

            m_AchievementDescription.text = hasItem ? achievement.achievementDescription : string.Empty;

            _activeAchievement = achievement;

            if (hasItem) {
                this.EnsureCoroutineStopped(ref _focusInThumbCoroutine);
                if (!force) {
                    _focusInThumbCoroutine = StartCoroutine(m_AchievementsScrollRect.FocusOnItemCoroutine((RectTransform)button.transform, m_FocusSpeed));
                } else {
                    m_AchievementsScrollRect.FocusOnItem((RectTransform)button.transform);
                }
            }
        }

        public GameObject GetFirstValidThumb() {
            foreach (AchievementButton button in _buttons) {
                if (GameKeysManager.instance.HaveGameKey(button.achievement.achievementGameKey))
                    return button.gameObject;
            }
            return m_SelectOnOpen;
        }

        private void ButtonClicked(AchievementButton button) {
            if (button.achievement) {
                SetAchievement(button);
            }
        }

        private void ToggleIcons() {
            foreach (var icon in m_Icons) {
                var iconEnabled = icon.expression.Get();
                foreach (var display in icon.displays) {
                    display.SetActive(iconEnabled);
                }
            }
        }
    }
}
