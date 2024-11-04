using UnityEngine;

namespace NFHGame.Characters {
    public class GameCharactersManager : Singleton<GameCharactersManager> {
        [SerializeField] private BastheetCharacterController m_Bastheet;
        [SerializeField] private DinnerCharacterController m_Dinner;
        [SerializeField] private SpammyCharacterController m_Spammy;

        public BastheetCharacterController bastheet => m_Bastheet;
        public DinnerCharacterController dinner => m_Dinner;
        public SpammyCharacterController spammy => m_Spammy;

        public GameCharacterController[] characters { get; private set; }

        protected override void Awake() {
            base.Awake();
            UpdateCharacters();
        }

        public void UpdateCharacters(bool forceSpammy = false) {
            if (forceSpammy || GameManager.instance.spammyInParty) {
                characters = new GameCharacterController[3] { m_Bastheet, m_Dinner, m_Spammy };
            } else {
                m_Spammy.gameObject.SetActive(false);
                characters = new GameCharacterController[2] { m_Bastheet, m_Dinner };
            }
        }

        public void ToggleLookBack(bool lookBack) {
            bastheet.ToggleLookBack(lookBack);
            dinner.ToggleLookBack(lookBack);
            spammy.ToggleLookBack(lookBack);
        }

        public void SetPosition(float positionX, bool facingRight) {
            var offset = facingRight ? -1 : 1;
            bastheet.SetPositionX(positionX, facingRight);
            dinner.SetPositionX(positionX + bastheet.dinnerOffset * offset, facingRight);
            if (GameManager.instance.spammyInParty) spammy.SetPositionX(positionX + bastheet.spammyOffset * offset, facingRight);
        }
    }
}