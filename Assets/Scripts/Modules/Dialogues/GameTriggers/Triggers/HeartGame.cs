using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl.Variables;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.Interaction.Behaviours;
using NFHGame.LevelAssets.Level5;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.SceneManagement.SceneState;
using System.Collections;
using UnityEngine;

namespace NFHGame.DialogueSystem.GameTriggers {
    public class HeartGame : GameTrigger {
        public const string RessurrectDragonGameKey = "ressurectDragon";

        [SerializeField] private Level5Defibrillator m_Defibrilator;
        [SerializeField] private Animator m_BastheetAnimator;
        [SerializeField] private Animator m_DefibrisAnimator;
        [SerializeField] private Animator m_HeartAnimator;
        [SerializeField] private Vector2 m_AvatarPosition;
        [SerializeField] private Vector2 m_IdlePosition;
        [SerializeField] private float m_AvatarEnterDuration;
        [SerializeField] private float m_AvatarExitDuration;
        [SerializeField] private float m_DefibriTime;
        [SerializeField] private GameObject m_DoorEntry;
        [SerializeField] private GameObject m_NormalDinnerSpammy, m_HugDinnerSpammy;

        [SerializeField] private ArticyRef m_FailDialogueRef;
        [SerializeField] private ArticyRef m_LoseDialogueRef;
        [SerializeField] private ArticyRef m_ActiveCoreDialogueRef;
        [SerializeField] private InteractionPlayDialogue m_HeartInteraction;

        [Header("Sounds & Anim")]
        [SerializeField] private float m_SucessBastAnimDelay;
        [SerializeField] private float m_SucessHeartAnimDelay;
        [SerializeField] private AudioProviderObject m_SucessSound;
        [SerializeField] private AudioProviderObject m_FailSound;
        [SerializeField] private AudioProviderObject m_SucessConfirmSound;
        [SerializeField] private AudioProviderObject m_EletricSound;

        private int _heartLevel;
        private bool _init;
        private GameTriggerProcessor.GameTriggerHandler _handler;

        private PooledAudioHandler _eletricSourceHandler;

        protected override bool DoLogic(GameTriggerProcessor.GameTriggerHandler handler) {
            _handler = handler;

            if (!_init) {
                _init = true;
                using (new ArticyImpl.ArticyManager.ToggleRollbackScope(false))
                    ArticyVariables.globalVariables.bossBattle.jumpstartHeart = 0;
                EnterAvatarState();
                m_Defibrilator.ScheduleDifibri(OnDefibriEnd);
                return true;
            }

            if (_heartLevel < 4) {
                m_Defibrilator.PlayDefibri(_heartLevel, OnDefibriEnd);
            } else {
                _handler.onReturnToDialogue.Invoke();
            }
            return true;
        }

        public void EnterAvatarState() {
            _eletricSourceHandler = AudioPool.instance.PlaySound(m_EletricSound);
            m_BastheetAnimator.Play("AvatarStateGentleEnter");
            m_DefibrisAnimator.gameObject.SetActive(true);
            m_DefibrisAnimator.Play("DEFRIBISenter");
            m_BastheetAnimator.transform.DOLocalMove(m_AvatarPosition, m_AvatarEnterDuration);
            HaloManager.HaloManager.instance.SetActive(true);
            if (ArticyVariables.globalVariables.gameState.spamInParty) {
                m_NormalDinnerSpammy.SetActive(false);
                m_HugDinnerSpammy.SetActive(true);
            } else {
                m_NormalDinnerSpammy.transform.GetChild(0).GetComponent<Animator>().Play(DinnerCharacterController.IdleAnimationHash.shit);
            }
            m_DoorEntry.SetActive(false);
        }

        public void ShotHeart() => StartCoroutine(ShotHeartCoroutine());

        public void ExitAvatarState() {
            m_BastheetAnimator.Play("AvatarStateGentleExit");
            m_DefibrisAnimator.Play("DEFRIBISexit");
            m_BastheetAnimator.transform.DOLocalMove(m_IdlePosition, m_AvatarExitDuration).OnComplete(() => {
                m_DefibrisAnimator.gameObject.SetActive(false);
            });
            if (ArticyVariables.globalVariables.gameState.spamInParty) {
                m_HugDinnerSpammy.SetActive(false);
                m_NormalDinnerSpammy.SetActive(true);
            } else {
                m_NormalDinnerSpammy.transform.GetChild(0).GetComponent<Animator>().Play(DinnerCharacterController.IdleAnimationHash.normal);
            }
            m_DoorEntry.SetActive(true);
            _eletricSourceHandler.source.DOFade(0.0f, 1.0f).OnComplete(() => {
                _eletricSourceHandler.source.Stop();
            });
        }

        private void OnDefibriEnd(bool sucess) {
            if (sucess) {
                ShotHeart();
            } else {
                ((Level5StateController)Level5StateController.instance).StopHeartBaseSounds();

                AudioPool.instance.PlaySound(m_FailSound);
                EndGame();
                DialogueManager.instance.executionEngine.ForceSetDialogue(m_LoseDialogueRef);
                _handler.onReturnToDialogue.Invoke();
                _heartLevel = 0;
                m_HeartAnimator.SetInteger("HeartLevel", _heartLevel);
            }
        }

        private void EndGame() {
            ExitAvatarState();
            m_Defibrilator.EndDefibri();
            _init = false;
        }

        private IEnumerator ShotHeartCoroutine() {
            _heartLevel++;
            AudioPool.instance.PlaySound(m_SucessSound);

            yield return Helpers.GetWaitForSeconds(m_SucessBastAnimDelay);
            m_BastheetAnimator.Play("AvatarStateGentleSHOT");

            yield return Helpers.GetWaitForSeconds(m_SucessHeartAnimDelay);
            m_HeartAnimator.Play("HEART00defribi");
            m_HeartAnimator.SetInteger("HeartLevel", _heartLevel);

            yield return Helpers.GetWaitForSeconds(m_DefibriTime);

            ((Level5StateController)Level5StateController.instance).PlayHeartBaseSound(_heartLevel - 1);

            if (_heartLevel == 4) {
                GameKeysManager.instance.ToggleGameKey(RessurrectDragonGameKey, true);
                GameKeysManager.instance.ToggleGameKey(Level4StateController.DragonAliveKey, true);
                ArticyVariables.globalVariables.gameState.RessurectedDragon = true;
                m_HeartInteraction.dialogueReference = m_ActiveCoreDialogueRef;
                AudioPool.instance.PlaySound(m_SucessConfirmSound);

                ((Level5StateController)Level5StateController.instance).PlayHeartBaseSound(4);
                ((Level5StateController)Level5StateController.instance).SetRessurrectDragonMusic();

                EndGame();
            }

            _handler.onReturnToDialogue.Invoke();
            using (new ArticyImpl.ArticyManager.ToggleRollbackScope(false))
                ArticyVariables.globalVariables.bossBattle.jumpstartHeart++;
        }
    }
}
