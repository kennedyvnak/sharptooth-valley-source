using Articy.Unity;
using NFHGame.Animations;
using NFHGame.Interaction.Behaviours;
using UnityEngine;

namespace NFHGame.LevelAssets.Level1e1 {
    public class Level1e1SpiderOnWeb : MonoBehaviour {
        [SerializeField] private SpriteArrayAnimator m_Animator;
        [SerializeField] private GameObject m_GroundSpider;
        [SerializeField] private ArticyRef m_DialogueOutWebRef;
        [SerializeField] private InteractionPlayDialogue m_DialogueInteraction;

        private bool _active;

        public void EnterArea() {
            if (_active) return;
            if (!HaloManager.HaloManager.instance.haloActive)
                PlayAnimation();
            else {
                HaloManager.HaloManager.instance.haloToggled.AddListener(EVENT_HaloToggled);
            }
        }

        public void ExitArea() {
            if (_active) return;
            if (HaloManager.HaloManager.instance && HaloManager.HaloManager.instance.haloActive)
                HaloManager.HaloManager.instance.haloToggled.RemoveListener(EVENT_HaloToggled);
        }

        private void PlayAnimation() {
            m_Animator.enabled = true;
            _active = true;
            m_GroundSpider.SetActive(true);
            m_DialogueInteraction.dialogueReference = m_DialogueOutWebRef;
        }

        private void EVENT_HaloToggled(bool active) {
            if (!active) {
                PlayAnimation();
                HaloManager.HaloManager.instance.haloToggled.RemoveListener(EVENT_HaloToggled);
            }
        }
    }
}
