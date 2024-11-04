using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.Input;
using NFHGame.UI.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NFHGame {
    public class DinnerTrustGameOver : Singleton<DinnerTrustGameOver> {
        [SerializeField] private ArticyRef m_DinnerGameOverDialogue;
        [SerializeField] private string m_DinnerGameOverLabel;

        [SerializeField] private bool m_EntryToLeft;
        [SerializeField] private float m_DinnerLeaveOffset;

        [SerializeField] private Renderer[] m_UnlitRenderers;
        [SerializeField] private Renderer[] m_LitRenderers;

        [SerializeField] private Material m_SpriteUnlitMaterial;
        [SerializeField] private Material m_SpriteLitMaterial;

        [SerializeField] private AudioSource[] m_FadeAudios;

        [SerializeField] private float m_ScreenFadeTime;
        [SerializeField] private float m_DialogueDelay;

        public bool dinnerGameOver { get; set; }
        public System.Func<bool> overrideGameOver;

        private void OnEnable() {
            ArticyManager.notifications.AddListener("trustPoints.dinnerPoints", UpdateDinnerPoints);
        }

        private void OnDisable() {
            ArticyManager.notifications.RemoveListener("trustPoints.dinnerPoints", UpdateDinnerPoints);
        }

        private void UpdateDinnerPoints(string variable, object dinnerPointsObj) {
            if ((int)dinnerPointsObj > 0 || dinnerGameOver | !DialogueManager.instance)
                return;

            if (overrideGameOver != null) {
                if (!overrideGameOver.Invoke())
                    return;
            }

            dinnerGameOver = true;

            var running = DialogueManager.instance.executionEngine.running;
            if (running) {
                var handler = DialogueManager.instance.executionEngine.currentHandler;
                if (handler != null)
                    handler.onDialogueFinished = null;
                DialogueManager.instance.executionEngine.FinishFlow();
            }

            InputReader.instance.PushMap(InputReader.InputMap.None);
            SoundtrackManager.instance.StopSoundtrack();

            var litRenderers = new List<Renderer>(m_LitRenderers);
            var unlitRenderers = new List<Renderer>(m_UnlitRenderers) {
                GameCharactersManager.instance.bastheet.GetComponent<SpriteRenderer>(),
                GameCharactersManager.instance.dinner.GetComponent<SpriteRenderer>(),
                GameCharactersManager.instance.bastheet.transform.Find("Tail").GetComponent<SpriteRenderer>()
            };
            var fadeLights = Object.FindObjectsOfType<Light2D>();
            foreach(var light in fadeLights)
                if (light.TryGetComponent<LightAsFire>(out var lightAsFire)) lightAsFire.enabled = false;

            foreach (var litSprite in litRenderers)
                litSprite.material = m_SpriteLitMaterial;

            foreach (var unlitSprite in unlitRenderers)
                unlitSprite.material = m_SpriteUnlitMaterial;

            float[] _lightIntensity = new float[fadeLights.Length];
            for (int i = 0; i < fadeLights.Length; i++)
                _lightIntensity[i] = fadeLights[i].intensity;

            float[] _soundVolume = new float[m_FadeAudios.Length];
            for (int i = 0; i < m_FadeAudios.Length; i++)
                _soundVolume[i] = m_FadeAudios[i].volume;

            DOVirtual.Float(1.0f, 0.0f, m_ScreenFadeTime, (x) => {
                for (int i = 0; i < _lightIntensity.Length; i++) {
                    fadeLights[i].intensity = _lightIntensity[i] * x;
                }

                for (int i = 0; i < _soundVolume.Length; i++) {
                    m_FadeAudios[i].volume = _soundVolume[i] * x;
                }

                UserInterfaceInput.instance.canvasGroup.alpha = x;
            }).OnComplete(() => {
                DOVirtual.DelayedCall(m_DialogueDelay, () => {
                    InputReader.instance.PopMap(InputReader.InputMap.None);
                    DialogueManager.instance.PlayHandledDialogue(m_DinnerGameOverDialogue).onDialogueFinished = () => {
                        InputReader.instance.PushMap(InputReader.InputMap.None);
                        StartCoroutine(DinnerLeavesGameOverCoroutine());
                    };
                });
            });
        }

        private IEnumerator DinnerLeavesGameOverCoroutine() {
            var dinner = GameCharactersManager.instance.dinner;
            dinner.rb.gravityScale = 0.0f;
            dinner.GetComponent<Collider2D>().enabled = false;

            yield return dinner.WalkOut(dinner.transform.position.x + (m_EntryToLeft ? -m_DinnerLeaveOffset : m_DinnerLeaveOffset));

            dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.normal);

            InputReader.instance.PopMap(InputReader.InputMap.None);
            GameManager.instance.GameOver(m_DinnerGameOverLabel);
        }
    }
}
