using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl;
using NFHGame.ArticyImpl.Variables;
using NFHGame.Characters;
using NFHGame.DinnerTrust;
using NFHGame.Input;
using NFHGame.SceneManagement.SceneState;
using NFHGame.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedDinnerTime : GameTriggerBase {

        public GameTriggerProcessor.GameTriggerHandler handler { get; private set; }

        [SerializeField] private int m_StartDinnerPoints;
        [SerializeField] private float m_DinnerGoPosition;
        [SerializeField] private float m_DinnerEndPosition;
        [SerializeField] private float m_DinnerArrivesDialogueStartDelay;
        [SerializeField] private ArticyRef m_DinnerTimeDialogue;
        [SerializeField] private Transform m_AnimCamera;
        [SerializeField] private float m_AnimCameraPos;
        [SerializeField] private Ease m_AnimCameraEase;
        [SerializeField] private float m_AnimCameraDuration;

        [Header("Minigame")]
        [SerializeField] private float m_BastheetPosX;
        [SerializeField] private CanvasGroup m_BarGroup;
        [SerializeField] private AudioSource m_TrustPlimSource;
        [SerializeField] private float m_SliderAnimDuration;

        [Header("Bast Hesitation")]
        [SerializeField] private float m_BastHesitationPosition;
        [SerializeField] private float m_BastHesitationDelay;
        [SerializeField] private float m_DinnerHesitationPosition;

        private void OnDestroy() {
            ArticyManager.notifications.RemoveListener("trustPoints.dinnerPoints", EVENT_DinnerPointsChanged);
        }

        public override bool Match(string id) {
            return id switch {
                "dinnerMinigame" => true,
                "smallRockFall" => true,
                "bastheetHesitation" => true,
                "dinnerJoins" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            this.handler = handler;
            switch (id) {
                case "dinnerMinigame":
                    DinnerMinigame();
                    return true;
                case "smallRockFall":
                    SmallRockFall();
                    return true;
                case "bastheetHesitation":
                    BastheetHesitation();
                    return true;
                case "dinnerJoins":
                    DinnerJoins();
                    return true;
                default:
                    return false;
            };
        }

        public void DialogueTriggerEnter() {
            StartCoroutine(DinnerArrives());
        }


        public void StartDinnerTime() {
            var pos = m_AnimCamera.position;
            pos.x = m_AnimCameraPos;
            m_AnimCamera.position = pos;
            Helpers.vCam.Follow = m_AnimCamera;
            Helpers.vCam.PreviousStateIsValid = false;
            ((Level1StateController)Level1StateController.instance).UnlockPassage();

            var dinner = GameCharactersManager.instance.dinner;
            dinner.SetPositionX(m_DinnerEndPosition, false);
            dinner.stateMachine.animState.Animate(DinnerCharacterController.DinnerIdleLampAnimationHash);
            dinner.transform.Find("LampLight").gameObject.SetActive(true);

            GameCharactersManager.instance.bastheet.SetPositionX(m_BastheetPosX, true);

            DialogueManager.instance.PlayHandledDialogue(m_DinnerTimeDialogue);
        }

        private IEnumerator DinnerArrives() {
            ((Level1StateController)Level1StateController.instance).UnlockPassage();

            InputReader.instance.PushMap(InputReader.InputMap.None);
            var dinner = GameCharactersManager.instance.dinner;
            dinner.gameObject.SetActive(true);
            dinner.SetPositionX(m_DinnerGoPosition);

            StartCoroutine(DinnerArrivesMove());

            m_AnimCamera.position = GameCharactersManager.instance.bastheet.transform.position;
            Helpers.vCam.Follow = m_AnimCamera;
            m_AnimCamera.DOMoveX(m_AnimCameraPos, m_AnimCameraDuration).SetEase(m_AnimCameraEase);

            yield return Helpers.GetWaitForSeconds(m_DinnerArrivesDialogueStartDelay);

            InputReader.instance.PopMap(InputReader.InputMap.None);
            DialogueManager.instance.PlayHandledDialogue(m_DinnerTimeDialogue);

            IEnumerator DinnerArrivesMove() {
                dinner.transform.Find("LampLight").gameObject.SetActive(true);
                yield return dinner.WalkOut(m_DinnerEndPosition, -1, animHash: DinnerCharacterController.DinnerWalkLampAnimationHash);
                dinner.stateMachine.animState.Animate(DinnerCharacterController.DinnerIdleLampAnimationHash);
            }
        }

        private void DinnerMinigame() {
            DataManager.instance.SaveCheckpoint(Level1StateController.DinnerTimeID);

            m_BarGroup.gameObject.SetActive(true);
            DinnerTrustGameOver.instance.overrideGameOver = delegate { return false; };
            ArticyVariables.globalVariables.trustPoints.dinnerPoints = m_StartDinnerPoints;
            m_TrustPlimSource.volume = 0.0f;
            DOVirtual.DelayedCall(m_TrustPlimSource.clip.length, () => m_TrustPlimSource.volume = 1.0f);
            m_BarGroup.GetComponent<Slider>().value = m_StartDinnerPoints;
            ArticyManager.notifications.AddListener("trustPoints.dinnerPoints", EVENT_DinnerPointsChanged);
            m_BarGroup.ToggleGroupAnimated(true, 1.0f).onComplete += () => {
                handler.onReturnToDialogue.Invoke();
            };
        }

        private void SmallRockFall() {
            handler.onReturnToDialogue.Invoke();
        }

        private void BastheetHesitation() {
            StartCoroutine(BastheetHesitationCoroutine());
        }

        private IEnumerator BastheetHesitationCoroutine() {
            DisableBar();

            var bastheet = GameCharactersManager.instance.bastheet;
            var dinner = GameCharactersManager.instance.dinner;

            StartCoroutine(DinnerMove());

            yield return Helpers.GetWaitForSeconds(m_BastHesitationDelay);

            yield return bastheet.WalkToPosition(m_BastHesitationPosition, flipX: false);

            handler.onReturnToDialogue.Invoke();

            IEnumerator DinnerMove() {
                yield return dinner.WalkOut(m_DinnerHesitationPosition, -1, animHash: DinnerCharacterController.DinnerWalkLampAnimationHash);
                dinner.stateMachine.animState.Animate(DinnerCharacterController.DinnerIdleLampAnimationHash);
            }
        }

        private void DinnerJoins() {
            DisableBar();
            ((Level1StateController)Level1StateController.instance).GetDinner();
            DinnerTrustGameOver.instance.overrideGameOver = null;
            ArticyManager.notifications.RemoveListener("trustPoints.dinnerPoints", EVENT_DinnerPointsChanged);
            GameCharactersManager.instance.dinner.transform.Find("LampLight").gameObject.SetActive(false);
            GameCharactersManager.instance.dinner.stateMachine.EnterDefaultState();
            m_AnimCamera.DOMoveX(GameCharactersManager.instance.bastheet.transform.position.x, m_AnimCameraDuration).SetEase(m_AnimCameraEase).OnComplete(() => {
                Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                handler.onReturnToDialogue.Invoke();
            });
        }


        private void DisableBar() {
            if (!m_BarGroup.gameObject.activeSelf) return;

            m_BarGroup.ToggleGroupAnimated(false, 1.0f).onComplete += () => {
                m_BarGroup.gameObject.SetActive(false);
            };
            DinnerTrustBarController.instance.canvasGroup.ToggleGroupAnimated(true, 1.0f);
        }

        private void EVENT_DinnerPointsChanged(string arg1, object arg2) {
            var slider = m_BarGroup.GetComponent<Slider>();
            int val = (int)arg2;
            DOVirtual.Float(slider.value, val, m_SliderAnimDuration, (x) => {
                slider.value = x;
            });
        }
    }
}
