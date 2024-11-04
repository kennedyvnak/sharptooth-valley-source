using Articy.Unity;
using NFHGame.Interaction.Behaviours;
using NFHGame.RangedValues;
using UnityEngine;
using UnityEngine.Events;

namespace NFHGame.LevelAssets.Level5 {
    public class Level5FrameControl : MonoBehaviour {
        [SerializeField] private bool m_ControlCamera;
        [SerializeField] private float m_StartCameraY;
        [SerializeField] private RangedFloat m_CameraRangeY;

        [SerializeField] private UnityEvent<bool> m_Toggled;

        [SerializeField] private InteractionPlayDialogue[] m_Dialogues;
        [SerializeField] private ArticyRef[] m_Refs;

        public bool controlCamera => m_ControlCamera;
        public float startCameraY => m_StartCameraY;
        public RangedFloat cameraRangeY => m_CameraRangeY;
        public UnityEvent<bool> toggled => m_Toggled;

        private void Awake() {
            toggled.AddListener((b) => {
                if (!b) return;
                for (int i = 0; i < m_Dialogues.Length; i++)
                    m_Dialogues[i].dialogueReference = m_Refs[i];
            });
        }
    }
}
