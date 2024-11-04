using System.Collections.Generic;
using Articy.Unity;
using DG.Tweening;
using NFHGame.ArticyImpl;
using NFHGame.AudioManagement;
using NFHGame.Characters;
using NFHGame.DialogueSystem;
using NFHGame.DialogueSystem.GameTriggers.Triggers;
using NFHGame.Input;
using NFHGame.Interaction;
using NFHGame.Interaction.Behaviours;
using NFHGame.SceneManagement.GameKeys;
using NFHGame.UI;
using UnityEngine;

namespace NFHGame.SceneManagement.SceneState {
    public class Level2StateController : SceneStateController {
        private const string k_ExamineTunnelsKey = "Level2_examineTunnels";
        private const string k_EnteredInAnyTunnelKey = "Level2_EnteredInTunnel";

        [System.Serializable]
        private struct RangeCharacter {
            public Vector2 position;
            public float scale;
        }

        [SerializeField] private ArticyRef m_FirstEnterDialogue;
        [SerializeField] private float m_FirstEnterFinalPosition;

        [SerializeField] private float m_FadeTime;
        [SerializeField] private SerializedDictionary<string, RangeCharacter[]> m_CharactersRange;
        [SerializeField] private Transform[] m_Characters;
        [SerializeField] private Transform m_RangeFocusAt;
        [SerializeField] private InteractionObject m_ReturnToPeakCollider;
        [SerializeField] private Vector3[] m_ReturnToPeakPositions;

        [SerializeField] private InteractionLoadScene[] m_TunnelsInteraction;
        [SerializeField] private ArticyRef m_FirstTunnelEntryDialogue;

        [Header("Lake Objects")]
        [SerializeField] private GameObject[] m_LitLakeObjects;
        [SerializeField] private GameObject[] m_UnlitLakeObjects;

        [Header("Soundtrack")]
        [SerializeField] private ChangeSoundtrackOnStart m_Soundtrack;
        [SerializeField] private AudioMusicObject m_STSulfurLake;
        [SerializeField] private AudioMusicObject m_STDinos;
        [SerializeField] private AudioMusicObject m_STIntoxicating;
        [SerializeField] private AudioMusicObject m_STDinosIntoxicating;
        [SerializeField] private AudioMusicObject m_STDragon;
        [SerializeField] private AudioMusicObject m_STAlive;

        [Header("Spammy Witness")]
        [SerializeField] private float m_SpammyWitnessPos;
        [SerializeField] private ArticyRef m_SpammyWitnessRef;

        private RangeCharacter[] _charactersRange;
        private bool _spammyWitness;
        private int _showTunnelIdx;

        protected override void Awake() {
            base.Awake();

            AudioMusicObject soundtrack = m_STSulfurLake;
            bool dinos = GameKeysManager.instance.HaveGameKey("Scene_level3");
            bool intoxicating = GameKeysManager.instance.HaveGameKey("Scene_level4");
            if (GameKeysManager.instance.HaveGameKey(Level4StateController.DragonAliveKey))
                soundtrack = m_STAlive;
            else if (GameKeysManager.instance.HaveGameKey(Level4StateController.DestroyedDragonKey))
                soundtrack = m_STDragon;
            else if (dinos && intoxicating)
                soundtrack = m_STDinosIntoxicating;
            else if (dinos)
                soundtrack = m_STDinos;
            else if (intoxicating)
                soundtrack = m_STIntoxicating;
            m_Soundtrack.soundtrack = soundtrack;

            foreach (var tunnelInteraction in m_TunnelsInteraction) {
                tunnelInteraction.validation = (point) => {
                    if (GameKeysManager.instance.HaveGameKey(k_EnteredInAnyTunnelKey)) {
                        return true;
                    } else {
                        GameKeysManager.instance.ToggleGameKey(k_EnteredInAnyTunnelKey, true);
                        var handler = DialogueManager.instance.PlayHandledDialogue(m_FirstTunnelEntryDialogue);
                        handler.onDialogueFinished += tunnelInteraction.LoadScene;
                        return false;
                    }
                };
            }

            bool litLake = GameKeysManager.instance.HaveGameKey(Level4StateController.LitLakeKey);

            foreach (var litObj in m_LitLakeObjects) {
                litObj.SetActive(litLake);
            }

            foreach (var unlitObj in m_UnlitLakeObjects) {
                unlitObj.SetActive(!litLake);
            }
        }

        public override void StartControl(SceneLoader.SceneLoadingHandler handler) {
            base.StartControl(handler);
            _spammyWitness = GameKeysManager.instance.HaveGameKey(ComposedDragonBattle.SpammyWitnessGameKey);

            if (m_CharactersRange.TryGetValue(handler.anchorID, out _charactersRange)) {
                Helpers.vCam.Follow = m_RangeFocusAt;
                Helpers.vCam.PreviousStateIsValid = false;
                DinnerTrustGameOver.instance.overrideGameOver = CharactersInEntryDinnerGameOver;

                int count = Mathf.Min(m_Characters.Length, _charactersRange.Length);
                for (int i = 0; i < count; i++) {
                    if (_spammyWitness && i == 2) continue; //spammy
                    var characterTransform = m_Characters[i];
                    var farPosition = _charactersRange[i];
                    characterTransform.position = farPosition.position;
                    characterTransform.localScale /= farPosition.scale;

                    if (characterTransform.TryGetComponent<Rigidbody2D>(out var rb)) {
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        rb.velocity = Vector2.zero;
                    }
                    if (characterTransform.TryGetComponent<BastheetCharacterController>(out var mcc)) {
                        mcc.PauseMovement();
                    }
                }

                handler.ResumeInput();
                StartCoroutine(Helpers.DelayForFramesCoroutine(1, () => m_ReturnToPeakCollider.enabled = true));
            }

            if (_spammyWitness) {
                var spammy = GameCharactersManager.instance.spammy;
                GameCharactersManager.instance.bastheet.InitSpammy(spammy);
                spammy.gameObject.SetActive(true);
                spammy.stateMachine.animState.Animate(SpammyCharacterController.IdleAnimationHash);
                spammy.SetFacingDirection(1);
                spammy.SetPositionX(m_SpammyWitnessPos);
            }
        }

        public void ReturnToPeak() {
            DinnerTrustGameOver.instance.overrideGameOver = null;
            m_ReturnToPeakCollider.enabled = false;
            InputReader.instance.PushMap(InputReader.InputMap.None);

            var handler = FadeScreen.instance.FadeFor(m_FadeTime);
            handler.onFinishFadeIn += () => {
                Helpers.vCam.Follow = GameCharactersManager.instance.bastheet.transform;
                Helpers.vCam.PreviousStateIsValid = false;

                int count = Mathf.Min(m_Characters.Length, m_ReturnToPeakPositions.Length);
                for (int i = 0; i < count; i++) {
                    if (_spammyWitness && i == 2) continue; //spammy
                    var characterTransform = m_Characters[i];
                    characterTransform.position = m_ReturnToPeakPositions[i];
                    characterTransform.localScale *= _charactersRange[i].scale;

                    if (characterTransform.TryGetComponent<Rigidbody2D>(out var rb)) {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                    }
                    if (characterTransform.TryGetComponent<BastheetCharacterController>(out var mcc)) {
                        mcc.ResumeMovement();
                    }
                }

                handler.FadeOut();
            };
            handler.onFinishFadeOut += () => InputReader.instance.PopMap(InputReader.InputMap.None);
        }

        public void TriggerSpammyWitness() {
            DialogueManager.instance.PlayDialogue(m_SpammyWitnessRef);
            ArticyManager.notifications.AddListener("gameState.spamInParty", ListenSpammyInParty);
        }

        public void StartShowTunnels() {
            if (_showTunnelIdx == 2) {
                Show(m_TunnelsInteraction[0]);
            } else if (_showTunnelIdx == 3) {
                Show(m_TunnelsInteraction[1]);
            }
            _showTunnelIdx++;

            void Show(InteractionLoadScene tunnel) {
                var pointHover = tunnel.GetComponentInChildren<InteractionObjectPointHoverHighlight>();
                pointHover.EnableHighlight();
                pointHover.Unregister();
            }
        }

        public void ShowTunnels() {
            GameKeysManager.instance.ToggleGameKey(k_ExamineTunnelsKey, true);
        }

        private void ListenSpammyInParty(string varName, object rawValue) {
            bool value = (bool)rawValue;
            if (!value) return;
            ArticyManager.notifications.RemoveListener("gameState.spamInParty", ListenSpammyInParty);
            _spammyWitness = false;
            GameKeysManager.instance.ToggleGameKey(ComposedDragonBattle.SpammyWitnessGameKey, false);
            var spammy = GameCharactersManager.instance.spammy;
            spammy.stateMachine.EnterDefaultState();
        }

        private bool CharactersInEntryDinnerGameOver() {
            List<SpriteRenderer> renderers = new List<SpriteRenderer>() {
                GameCharactersManager.instance.bastheet.GetComponent<SpriteRenderer>(),
                GameCharactersManager.instance.dinner.GetComponent<SpriteRenderer>(),
                GameCharactersManager.instance.bastheet.transform.Find("Tail").GetComponent<SpriteRenderer>()
            };

            var t = DOVirtual.Float(1.0f, 0.0f, 1.0f, (x) => {
                foreach (var renderer in renderers) {
                    var col = renderer.color;
                    col.a = x;
                    renderer.color = col;
                }
            });
            t.SetLoops(2, LoopType.Yoyo).OnStepComplete(() => {
                t.OnStepComplete(null);

                var bastP = renderers[0].transform.position;
                bastP.x -= 2.0f;
                renderers[0].transform.position = bastP;

                foreach (var renderer in renderers) {
                    renderer.transform.localScale = Vector3.one;
                }
            });
            return true;
        }
    }
}