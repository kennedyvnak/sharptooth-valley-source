using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFHGame.AchievementsManagement.UI {
    public class AchievementButton : MonoBehaviour, ISelectHandler {
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private UnityEvent<AchievementButton> m_OnButtonClick;

        public UnityEvent<AchievementButton> onButtonClick => m_OnButtonClick;
        public Button button { get; private set; }
        public CanvasGroup group { get; private set; }
        public TextMeshProUGUI text => m_Text;

        public AchievementObject achievement { get; set; }

        public void Init() {
            button = GetComponent<Button>();
            group = GetComponent<CanvasGroup>();
            button.onClick.AddListener(() => m_OnButtonClick?.Invoke(this));
        }

        void ISelectHandler.OnSelect(BaseEventData eventData) {
            m_OnButtonClick?.Invoke(this);
        }
    }
}
