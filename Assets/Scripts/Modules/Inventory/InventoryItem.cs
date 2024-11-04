using UnityEngine;
using Articy.Unity;
using NFHGame.AchievementsManagement;
using NFHGame.Inventory.UI;

namespace NFHGame.Inventory {
    [CreateAssetMenu(menuName = "Scriptable/Inventory/Item")]
    public class InventoryItem : ScriptableObject {
        [System.Serializable]
        public class ItemDialogueAction {
            public ArticyRef dialogue;
        }

        public string itemName;
        public string articyVariable;
        public AchievementObject achievement;
        public AchievementObject sacrificeAchievement;
        public ItemDialogueAction[] dialogues;
        public InventoryItemAction[] actions;
        public Sprite thumb, display;
        public bool hiddenItem;

        public string conditionToRecreate;
    }
}
