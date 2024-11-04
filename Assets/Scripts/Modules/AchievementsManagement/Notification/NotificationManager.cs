using DG.Tweening;
using NFHGame.AudioManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace NFHGame.AchievementsManagement {
    public class NotificationManager : Singleton<NotificationManager> {
        [SerializeField] private float m_Margin, m_FixedSize, m_AnimDuration, m_OutDelayMin, m_OutDelayPerChar;
        [SerializeField] private Ease m_EaseIn, m_EaseOut;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private RectTransform m_Transform;
        [SerializeField] private AudioPlayer m_AchievementSound;

        private Queue<string> _notificationsQueue = new Queue<string>();
        private bool _inNotification;

        public void Notify(string text) {
            if (_inNotification) {
                _notificationsQueue.Enqueue(text);
                return;
            }

            m_Text.text = text;
            m_Text.ForceMeshUpdate();
            var width = m_Text.preferredWidth;

            m_Transform.sizeDelta = new Vector2(m_FixedSize + width + m_Margin, m_Transform.sizeDelta.y);
            m_Transform.anchoredPosition = new Vector2(-m_Transform.sizeDelta.x, m_Transform.anchoredPosition.y);

            transform.GetChild(0).gameObject.SetActive(true);

            _inNotification = true;
            m_AchievementSound.Play();
            m_Transform.DOAnchorPosX(0.0f, m_AnimDuration).SetEase(m_EaseIn).OnComplete(() => {
                m_Transform.DOAnchorPosX(-m_Transform.sizeDelta.x, m_AnimDuration).SetEase(m_EaseOut).SetDelay(Mathf.Max(m_OutDelayPerChar * m_Text.textInfo.characterCount, m_OutDelayMin)).OnComplete(() => {
                    transform.GetChild(0).gameObject.SetActive(false);
                    _inNotification = false;
                    if (_notificationsQueue.Count > 0) {
                        Notify(_notificationsQueue.Dequeue());
                    }
                });
            });
        }

#if UNITY_EDITOR
        [ContextMenu("Test")]
        public void Test() {
            var name = NFHGameEditor.EditorInputDialog.Show("Notification Test", "Please enter the notification", "", true);
            if (!string.IsNullOrEmpty(name)) {
                Notify(name);
            }
        }
#endif
    }
}
