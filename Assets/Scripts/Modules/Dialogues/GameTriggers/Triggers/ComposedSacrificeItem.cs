using NFHGame.AchievementsManagement;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.Inventory;
using NFHGame.LevelAssets.Level4e1;
using NFHGame.Serialization;
using System.Collections;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class ComposedSacrificeItem : GameTriggerBase {
        [System.Serializable]
        public struct SacrificeData {
            public InventoryItem item;
            public Sprite sprite;
            public SacrificeCutsceneSubtitle manager;
        }

        [SerializeField] private float m_StartDropDuration;
        [SerializeField] private float m_DropDuration;

        [SerializeField] private SerializedDictionary<string, SacrificeData> m_Sacrifices;
        [SerializeField] private AchievementObject m_SacrifyItemAchievement;

        [SerializeField] private AudioProviderObject m_DropSound;
        [SerializeField] private ParticleSystem m_SmokeParticle;

        [SerializeField] private float m_DinnerStepBackPositionA,  m_DinnerStepBackPositionB, m_SpammyStepBackPositionA, m_SpammyStepBackPositionB;

        private int _stepBack;
        private GameTriggerProcessor.GameTriggerHandler _currentHandler;

        public override bool Match(string id) {
            if (m_Sacrifices.ContainsKey(id)) return true;
            return id switch {
                "fewStepsBack" => _stepBack <= 1,
                _ => false,
            };
        }

        public override bool Process(GameTriggerProcessor.GameTriggerHandler handler, string id) {
            _currentHandler = handler;

            if (id == "fewStepsBack") {
                if (_stepBack == 0) DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => _stepBack = 0;
                float dinnerPos = _stepBack == 1 ? m_DinnerStepBackPositionB : m_DinnerStepBackPositionA;
                float spammyPos = _stepBack == 1 ? m_SpammyStepBackPositionB : m_SpammyStepBackPositionA;
                StartCoroutine(StepsBack(dinnerPos, spammyPos, _stepBack == 0));
                _stepBack++;
                return true;
            }

            if (m_Sacrifices.TryGetValue(id, out var sacrifice)) {
                StartCoroutine(DoSacrifice(sacrifice));
                return true;
            }

            return false;
        }

        private IEnumerator DoSacrifice(SacrificeData sacrifice) {
            AchievementsManager.instance.UnlockAchievement(m_SacrifyItemAchievement);
            AudioPool.instance.PlaySound(m_DropSound);
            m_SmokeParticle.Play();
            
            var bastheet = GameCharactersManager.instance.bastheet;
            bastheet.stateMachine.dropState.StartDrop(sacrifice.sprite);

            yield return Helpers.GetWaitForSeconds(m_StartDropDuration);
            bastheet.stateMachine.dropState.DropOnPool();

            yield return Helpers.GetWaitForSeconds(m_DropDuration);
            sacrifice.manager.StartAnimation(() => {
                AchievementsManager.instance.UnlockAchievement(sacrifice.item.sacrificeAchievement);
                DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += DataManager.instance.Save;
                _currentHandler.onReturnToDialogue.Invoke();
            }, () => {
                GameCharactersManager.instance.bastheet.stateMachine.EnterDefaultState();
            });
        }

        private IEnumerator StepsBack(float dinnerPos, float spammyPos, bool flipX) {
            bool spammyInParty = GameManager.instance.spammyInParty;
            var dinner = GameCharactersManager.instance.dinner;
            var spammy = GameCharactersManager.instance.spammy;

            DialogueManager.instance.executionEngine.currentHandler.onDialogueFinished += () => {
                dinner.stateMachine.EnterDefaultState();
                if (spammyInParty)
                    spammy.stateMachine.EnterDefaultState();
            };

            var dinnerCoroutine = StartCoroutine(DinnerCoroutine());
            if (spammyInParty) yield return SpammyCoroutine();
            yield return dinnerCoroutine;

            _currentHandler.onReturnToDialogue.Invoke();

            IEnumerator DinnerCoroutine() {
                yield return dinner.WalkOut(dinnerPos, 1, flipX: flipX);
                dinner.stateMachine.animState.Animate(DinnerCharacterController.IdleAnimationHash.GetAnimation(false));
            }
            IEnumerator SpammyCoroutine() {
                yield return spammy.WalkOut(spammyPos, 1, flipX: flipX);
                spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
            }
        }
    }
}