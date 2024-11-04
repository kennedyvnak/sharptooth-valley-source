using Articy.Unity;
using NFHGame.DialogueSystem.GameTriggers;
using NFHGame.DinnerTrust;
using NFHGame.Interaction.Behaviours;
using NFHGame.Inventory;
using NFHGame.Inventory.UI;
using NFHGame.Screens;
using NFHGame.Serialization;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class Level1StateController : SceneStateController {
        public const string DinnerTimeID = "dinnerTime";

        [SerializeField] private ComposedAloneInTheDark m_AloneInTheDark;
        [SerializeField] private ComposedDinnerTime m_DinnerTime;
        [SerializeField] private InteractionPlayDialogue m_EntryInteraction, m_CarvingInteraction;
        [SerializeField] private ArticyRef m_SoloEntryInteraction, m_DinnerEntryInteraction;
        [SerializeField] private InventoryItem m_CrystalBoxItem;
        [SerializeField] private GameObject m_SoloRockInteraction, m_DinnerRockInteraction, m_RightBlock, m_RightEndBlock, m_DinnerFound, m_BagAndCandle;

        [Header("Scenary")]
        [SerializeField] private ComposedFinalPositions m_FinalPositions;

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);
            if (handler.anchorID.Equals("newGame")) {
                handler.ResumeInput();
                m_AloneInTheDark.StartGame();
                m_RightBlock.SetActive(true);
                m_RightEndBlock.SetActive(true);
                m_EntryInteraction.dialogueReference = m_SoloEntryInteraction;
                m_CarvingInteraction.interactionObject.Disable();
                m_SoloRockInteraction.SetActive(true);
                m_DinnerRockInteraction.SetActive(false);
                m_BagAndCandle.SetActive(true);
                DinnerTrustBarController.instance.canvasGroup.ToggleGroup(false);
                PauseScreen.instance.canSave = false;
                InventoryManager.instance.AddItem(m_CrystalBoxItem);
            } else if (MatchState(handler, DinnerTimeID)) {
                handler.ResumeInput();
                m_DinnerTime.StartDinnerTime();
                m_EntryInteraction.dialogueReference = m_SoloEntryInteraction;
                m_CarvingInteraction.interactionObject.Disable();
                m_SoloRockInteraction.SetActive(true);
                m_DinnerRockInteraction.SetActive(false);
                DinnerTrustBarController.instance.canvasGroup.ToggleGroup(false);
                PauseScreen.instance.canSave = false;
            } else if (MatchState(handler, ComposedFinalPositions.FinalPositionsID)) {
                handler.ResumeInput();
                m_FinalPositions.StartFinalPositions();
            }
        }

        public void LightupScene() {
            m_RightBlock.SetActive(false);
            m_DinnerFound.SetActive(true);
            m_BagAndCandle.SetActive(false);
        }

        public void UnlockPassage() {
            m_RightEndBlock.SetActive(false);
        }

        public void GetDinner() {
            m_EntryInteraction.dialogueReference = m_DinnerEntryInteraction;
            m_CarvingInteraction.interactionObject.Enable();
            m_SoloRockInteraction.SetActive(false);
            m_DinnerRockInteraction.SetActive(true);
            m_DinnerFound.SetActive(false);
            DinnerTrustBarController.instance.canvasGroup.ToggleGroupAnimated(true, 1.0f);
            PauseScreen.instance.canSave = true;
            DataManager.instance.ClearSave();
        }
    }
}