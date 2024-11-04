using Articy.Unity;
using DG.Tweening;
using NFHGame.Animations;
using NFHGame.AudioManagement;
using NFHGame.DialogueSystem;
using NFHGame.Interaction;
using NFHGame.SceneManagement.GameKeys;
using System;
using UnityEngine;

namespace NFHGame.LevelAssets.Level4 {
    public class PondPuzzleManager : Singleton<PondPuzzleManager> {
        public const string ToggledPuzzleGameKey = "L4PP_Toggled";

        [System.Serializable]
        public class PuzzleRock {
            public string gameKey;

            public Transform transform;
            public SpriteArrayAnimator holeAnim;
            public float disablePos, enablePos;
            public float moveDuration;
            public AudioObject enableSound, disableSound;


            [System.NonSerialized] public InteractionObject interaction;
            [System.NonSerialized] public SpriteArrayAnimator anim;
            [System.NonSerialized] public Sprite[] enabledAnim;
            [System.NonSerialized] public Sprite[] disabledAnim;

            public void Init() {
                interaction = transform.GetComponent<InteractionObject>();
                anim = transform.GetComponent<SpriteArrayAnimator>();

                enabledAnim = anim.values;
                disabledAnim = (Sprite[])anim.values.Clone();
                Array.Reverse(enabledAnim);
            }
        }

        [SerializeField] private PuzzleRock m_LeftRock, m_RightRock;
        [SerializeField] private Ease m_RockEnableEase, m_RockDisableEase;

        [Header("Root")]
        [SerializeField] private string m_RootGameKey;
        [SerializeField] private Sprite m_RootBrokeSprite;
        [SerializeField] private SpriteArrayAnimator m_RootHoleAnimator;
        [SerializeField] private InteractionObject m_RootInteraction;
        [SerializeField] private AudioObject m_RootHarvestSound;
        [SerializeField] private ArticyRef m_BrokeRootDialogue;

        private void Start() {
            InitRock(m_LeftRock);
            InitRock(m_RightRock);

            if (GameKeysManager.instance.HaveGameKey(m_RootGameKey)) {
                m_RootInteraction.Disable();
                m_RootInteraction.GetComponent<SpriteRenderer>().sprite = m_RootBrokeSprite;
                m_RootHoleAnimator.valueChanged.Invoke(m_RootHoleAnimator.values[^1]);
            }
        }

        public void MoveRock(bool leftRock) => MoveRock(leftRock ? m_LeftRock : m_RightRock);

        public void MoveRock(PuzzleRock rock) {
            GameKeysManager.instance.ToggleGameKey(ToggledPuzzleGameKey, true);
            bool rockEnabled = !GameKeysManager.instance.HaveGameKey(rock.gameKey);

            AudioPool.instance.PlaySound(rockEnabled ? rock.enableSound : rock.disableSound);
            GameKeysManager.instance.ToggleGameKey(rock.gameKey, rockEnabled);
            rock.anim.values = rockEnabled ? rock.enabledAnim : rock.disabledAnim;
            rock.interaction.Disable();

            if (rockEnabled) {
                rock.anim.enabled = true;
                rock.holeAnim.enabled = true;
                DOVirtual.DelayedCall(rock.anim.duration, () => {
                    rock.transform.DOMoveX(rock.disablePos, rock.moveDuration).OnComplete(() => rock.interaction.Enable());
                }).SetEase(m_RockDisableEase);
            } else {
                rock.transform.DOMoveX(rock.enablePos, rock.moveDuration).OnComplete(() => {
                    rock.anim.enabled = true;
                    rock.holeAnim.valueChanged.Invoke(rock.holeAnim.values[0]);
                    DOVirtual.DelayedCall(rock.anim.duration, () => rock.interaction.Enable());
                }).SetEase(m_RockEnableEase);
            }
        }

        public void InitRock(PuzzleRock rock) {
            rock.Init();
            if (GameKeysManager.instance.HaveGameKey(rock.gameKey)) {
                var pos = rock.transform.position;
                pos.x = rock.disablePos;
                rock.transform.position = pos;

                rock.anim.valueChanged.Invoke(rock.disabledAnim[0]);
                rock.holeAnim.valueChanged.Invoke(rock.holeAnim.values[^1]);
            }
        }

        public void BreakRoot() {
            m_RootInteraction.Disable();
            m_RootHoleAnimator.enabled = true;
            m_RootInteraction.GetComponent<SpriteRenderer>().sprite = m_RootBrokeSprite;
            GameKeysManager.instance.ToggleGameKey(m_RootGameKey, true);
            AudioPool.instance.PlaySound(m_RootHarvestSound);
            DialogueManager.instance.PlayDialogue(m_BrokeRootDialogue);
        }
    }
}
