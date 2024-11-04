using DG.Tweening;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.Input;
using NFHGame.Screens;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedPhotoTime : GameTriggerBase {
        [Header("Positions")]
        [SerializeField] private float m_BastheetPosition;
        [SerializeField] private float m_DinnerPosition;
        [SerializeField] private float m_SpammyPosition;

        [Header("Photo")]
        [SerializeField] private Image m_AlterphactImage;
        [SerializeField] private float m_AlterphactImageFadeDuration;
        [SerializeField] private AudioProviderObject m_CameraSnap;

        private GameTriggerProcessor.GameTriggerHandler _handler;

        public override bool Match(string id) {
            return id switch {
                "startPhoto" => true,
                "photoPositioning" => true,
                "photoTime" => true,
                "trioPhotoTime" => true,
                _ => false
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _handler = handler;
            switch (id) {
                case "startPhoto":
                    ScreenManager.instance.PopAll();
                    DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => {
                        InputReader.instance.PopMap(InputReader.InputMap.UI);
                    };
                    handler.onReturnToDialogue.Invoke();
                    break;
                case "photoPositioning":
                    StartCoroutine(Positioning());
                    break;
                case "photoTime":
                case "trioPhotoTime":
                    DoPhoto();
                    break;
            }
            return true;
        }

        private IEnumerator Positioning() {
            var bastheet = GameCharactersManager.instance.bastheet;
            var dinner = GameCharactersManager.instance.dinner;
            var spammy = GameCharactersManager.instance.spammy;
            var bastheetCoroutine = StartCoroutine(bastheet.WalkToPosition(m_BastheetPosition));
            var dinnerCoroutine = StartCoroutine(dinner.WalkOut(m_DinnerPosition));
            if (GameManager.instance.spammyInParty)
                yield return spammy.WalkOut(m_SpammyPosition);
            yield return bastheetCoroutine;
            yield return dinnerCoroutine;

            _handler.onReturnToDialogue.Invoke();
        }

        private void DoPhoto() {
            AudioPool.instance.PlaySound(m_CameraSnap);
            m_AlterphactImage.gameObject.SetActive(true);
            m_AlterphactImage.DOFade(0.0f, m_AlterphactImageFadeDuration).OnComplete(() => {
                m_AlterphactImage.gameObject.SetActive(false);
                _handler.onReturnToDialogue.Invoke();
            });
        }
    }
}
