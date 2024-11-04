using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.HaloManager;
using NFHGame.Input;
using NFHGame.Inventory.UI;
using NFHGame.SceneManagement.SceneState;
using NFHGame.Screens;
using NFHGame.Tutorial;
using System.Collections;
using TMPro;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedAloneInTheDark : GameTriggerBase {
        private static ComposedAloneInTheDark _instance;
        public static ComposedAloneInTheDark instance => _instance;

        [SerializeField] private ArticyRef m_StartDialogue;
        [SerializeField] private SpriteRenderer m_BagRenderer;
        [SerializeField] private float m_BastAwakeTime, m_BastCleanTime;
        [SerializeField] private AudioObject m_PickSound, m_MetalClankSound;

        [SerializeField] private GameObject[] m_CreateCanvasGroup;
        [SerializeField] private GameObject[] m_DisableObjects;
        [SerializeField] private Transform m_OptionsParent;

        [SerializeField] private RectTransform m_TutorialTransform;
        [SerializeField] private float m_ReturnDialogueDelay;

        [SerializeField] private CanvasGroup m_GetUpInput;
        [SerializeField] private float m_GetUpInputDelay;
        [SerializeField] private float m_StartDialogueDelay;

        public GameTriggerProcessor.GameTriggerHandler handler { get; private set; }

        private bool _thumbB;
        private Tweener _tween;
        private bool _getUp;

        private void Awake() {
            _instance = this;
        }

        public void StartGame() {
            CreateCanvasGroups();
            GameCharactersManager.instance.dinner.gameObject.SetActive(false);
            StartCoroutine(TheAwakening());
        }

        private IEnumerator TheAwakening() {
            var bastheet = GameCharactersManager.instance.bastheet;
            InputReader.instance.PushMap(InputReader.InputMap.None);
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.DownAnimationHash);

            yield return Helpers.GetWaitForSeconds(m_GetUpInputDelay);

            _getUp = false;
            InputReader.instance.QTE_GetUp += EVENT_OnGetUp;
            InputReader.instance.PushMap(InputReader.InputMap.QuickTimeEvents);

            m_GetUpInput.gameObject.SetActive(true);
            m_GetUpInput.ToggleGroupAnimated(true, 1.0f);

            while (!_getUp)
                yield return null;

            _getUp = false;
            m_GetUpInput.ToggleGroupAnimated(false, 1.0f).onComplete += () => m_GetUpInput.gameObject.SetActive(false);
            InputReader.instance.PopMap(InputReader.InputMap.QuickTimeEvents);

            bastheet.stateMachine.animState.Animate(BastheetCharacterController.SitAnimationHash);

            yield return Helpers.GetWaitForSeconds(m_StartDialogueDelay);

            InputReader.instance.PopMap(InputReader.InputMap.None);
            DialogueManager.instance.PlayHandledDialogue(m_StartDialogue).onDialogueFinished += () => {
                Level1Tutorial.instance.StartMovement(() => !DialogueManager.instance.executionEngine.running);
                Level1Tutorial.instance.StartInteraction(() => !DialogueManager.instance.executionEngine.running);

                InventoryManager.instance.enabled = false;
                HaloManager.HaloManager.instance.enabled = false;
            };

            while (!_getUp)
                yield return null;

            yield return Helpers.GetWaitForSeconds(m_StartDialogueDelay);
            bastheet.stateMachine.animState.Animate(BastheetCharacterController.GetUpAnimationHash);
            yield return Helpers.GetWaitForSeconds(m_BastCleanTime);
            bastheet.stateMachine.EnterDefaultState();
            yield return Helpers.GetWaitForSeconds(m_StartDialogueDelay);
            handler.onReturnToDialogue.Invoke();
        }

        public override bool Match(string id) {
            return id switch {
                "aloneInTheDarkGetUp" => true,
                "openInventory" => true,
                "weirdBox" => true,
                "closerLook" => true,
                "exitInventory" => true,
                "haloMinigame" => true,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            this.handler = handler;
            switch (id) {
                case "aloneInTheDarkGetUp":
                    GT_GetUp();
                    return true;
                case "openInventory":
                    GT_OpenInventory();
                    return true;
                case "weirdBox":
                    GT_WeirdBox();
                    return true;
                case "closerLook":
                    GT_CloserLook();
                    return true;
                case "exitInventory":
                    GT_ExitInventory();
                    return true;
                case "haloMinigame":
                    GT_HaloMinigame();
                    return true;
                default:
                    return false;
            };
        }

        public void StoreCrystalBoxThumbContext(OpenCrystalBoxAction openCrystalBoxAction, ActionContext context) {
            if (_thumbB) {
                context.itemDisplay.sprite = openCrystalBoxAction.displayC;
                context.itemThumb.sprite = openCrystalBoxAction.thumbC;
                AudioPool.instance.PlaySound(m_PickSound);
            } else {
                context.itemDisplay.sprite = openCrystalBoxAction.displayB;
                context.itemThumb.sprite = openCrystalBoxAction.thumbB;
                AudioPool.instance.PlaySound(m_MetalClankSound);
            }

            var btn = m_OptionsParent.GetChild(0);
            var canvas = btn.GetComponent<CanvasGroup>();
            canvas.interactable = false;

            _thumbB = true;

            DOVirtual.DelayedCall(m_ReturnDialogueDelay, () => handler.onReturnToDialogue.Invoke());
        }

        public void CLickInventoryButton() {
            var btn = m_OptionsParent.GetChild(0);
            var canvas = btn.GetComponent<CanvasGroup>();
            canvas.interactable = false;
            handler.onReturnToDialogue.Invoke();
        }

        private void CreateCanvasGroups() {
            foreach (var disableObj in m_DisableObjects)
                disableObj.SetActive(false);

            foreach (var createCanvas in m_CreateCanvasGroup) {
                var canvas = createCanvas.AddComponent<CanvasGroup>();
                canvas.interactable = false;
                canvas.blocksRaycasts = false;
                canvas.alpha = 0.0f;
                canvas.ignoreParentGroups = true;
            }
        }

        private void GT_GetUp() {
            _getUp = true;
        }

        private void GT_OpenInventory() {
            m_BagRenderer.DOFade(0.0f, 1.0f);
            var group = m_CreateCanvasGroup[0].GetComponent<CanvasGroup>();
            _tween = group.ToggleGroupAnimated(true, 1.0f);
            group.interactable = true;
            group.blocksRaycasts = true;
            Level1Tutorial.instance.StartInventory();
            InventoryOpener.instance.ShakeBag();
            StartCoroutine(Coroutine_OpenInventory());
            InventoryOpener.instance.button.interactable = true;
            InputReader.instance.QTE_OpenInventory += EVENT_OpenInventory;
            InputReader.instance.PushMap(InputReader.InputMap.QuickTimeEvents | InputReader.InputMap.UI);
        }

        private void GT_WeirdBox() {
            var btn = m_OptionsParent.GetChild(0);
            var canvas = btn.gameObject.AddComponent<CanvasGroup>();
            canvas.alpha = 1;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            canvas.ignoreParentGroups = true;
        }

        private void GT_CloserLook() {
            var btn = m_OptionsParent.GetChild(0);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = "Open Box";
            var canvas = btn.GetComponent<CanvasGroup>();
            canvas.interactable = true;
        }

        private void GT_ExitInventory() {
            var btn = m_OptionsParent.GetChild(0);
            btn.GetComponent<CanvasGroup>().ignoreParentGroups = false;

            InventoryManager.instance.SetInteraction(true);
            ScreenManager.instance.PopScreen();
            handler.onReturnToDialogue.Invoke();
            ArticyVariables.globalVariables.items.weirdBox = 0;

            Destroy(m_TutorialTransform.gameObject);
        }

        private void GT_HaloMinigame() {
            HaloToggleButton.instance.HaloMinigame(() => {
                ((Level1StateController)Level1StateController.instance).LightupScene();
                var group = m_CreateCanvasGroup[1].GetComponent<CanvasGroup>();
                group.ToggleGroupAnimated(true, 1.0f).onComplete += () => {
                    DestroyCanvas(group);
                };
                handler.onReturnToDialogue.Invoke();
            });
        }

        private void EVENT_OpenInventory() {
            InputReader.instance.PopMap(InputReader.InputMap.QuickTimeEvents | InputReader.InputMap.UI);
            ScreenManager.instance.PushScreen(InventoryManager.instance);
            m_TutorialTransform.SetParent(m_OptionsParent);
            m_TutorialTransform.localScale = Vector3.one;
            m_TutorialTransform.gameObject.SetActive(true);
        }

        private IEnumerator Coroutine_OpenInventory() {
            while (ScreenManager.instance.currentScreen != (IScreen)InventoryManager.instance)
                yield return null;

            InputReader.instance.QTE_OpenInventory -= EVENT_OpenInventory;

            _tween.Kill();
            DestroyCanvas(m_CreateCanvasGroup[0].GetComponent<CanvasGroup>());
            Level1Tutorial.instance.EndInventory();

            handler.onReturnToDialogue?.Invoke();

            InventoryManager.instance.SetInteraction(false);
            InputReader.instance.PopMap(InputReader.InputMap.UI);
            InputReader.instance.PopMap(InputReader.InputMap.QuickTimeEvents | InputReader.InputMap.UI);

            DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => {
                InventoryManager.instance.enabled = true;
                HaloManager.HaloManager.instance.enabled = true;

                foreach (var disableObj in m_DisableObjects)
                    disableObj.SetActive(true);
            };
        }

        private void DestroyCanvas(CanvasGroup group) {
            group.ignoreParentGroups = false;
            Destroy(group);
        }

        private void EVENT_OnGetUp() {
            InputReader.instance.QTE_GetUp -= EVENT_OnGetUp;
            _getUp = true;
        }
    }
}
