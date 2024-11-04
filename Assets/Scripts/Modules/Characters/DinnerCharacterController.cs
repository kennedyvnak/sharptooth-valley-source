using NFHGame.Battle;
using NFHGame.Characters.StateMachines;
using UnityEngine;

namespace NFHGame.Characters {
    [RequireComponent(typeof(Rigidbody2D))]
    public class DinnerCharacterController : FollowerCharacterController {
        public readonly struct AnimatorAnimationHashes {
            public readonly int normal;
            public readonly int shit;

            public AnimatorAnimationHashes(string normal, string shit) {
                this.normal = Animator.StringToHash(normal);
                this.shit = Animator.StringToHash(shit);
            }

            public int GetAnimation(bool shitDinner) {
                return shitDinner ? shit : normal;
            }
        }

        public static readonly AnimatorAnimationHashes IdleAnimationHash = new AnimatorAnimationHashes("DinnerIdle", "DinnerSHITidle");
        public static readonly AnimatorAnimationHashes WalkAnimationHash = new AnimatorAnimationHashes("DinnerWalk", "DinnerSHITwalk");
        public static readonly AnimatorAnimationHashes RunAnimationHash = new AnimatorAnimationHashes("DinnerRun", "DinnerSHITrun");

        public static readonly int BackAnimationHash = Animator.StringToHash("DinnerBACK");
        public static readonly int HittedAnimationHash = Animator.StringToHash("DinnerHITTED");
        public static readonly int DrownedAnimationHash = Animator.StringToHash("DinnerDrowned");
        public static readonly int StunnedAnimationHash = Animator.StringToHash("DinnerStunned");

        public static readonly int KnockedOutAnimationHash = Animator.StringToHash("DinnerKnockedOut");
        public static readonly int KnockedDownAnimationHash = Animator.StringToHash("DinnerKnockedDown");
        public static readonly int KnockedUpAnimationHash = Animator.StringToHash("DinnerKnockedUP");

        public static readonly int SitGetUpAnimationHash = Animator.StringToHash("DinnerSITgetUP");
        public static readonly int SitIdleAnimationHash = Animator.StringToHash("DinnerSITIdle");
        public static readonly int SitLighterAnimationHash = Animator.StringToHash("DinnerSITLighter");
        public static readonly int SitLighterSparkAnimationHash = Animator.StringToHash("DinnerSITLighterSpark");

        public static readonly int DinnerIdleJournalAnimationHash = Animator.StringToHash("DinnerIdleJOURNAL");
        public static readonly int DinnerIdleLampAnimationHash = Animator.StringToHash("DinnerIdleLAMP");
        public static readonly int DinnerWalkLampAnimationHash = Animator.StringToHash("DinnerWalkLAMP");

        public static readonly int EpicEnterAnimationHash = Animator.StringToHash("DinnerEpicEnter");

        private bool _isInBattle;

        public DinnerStateMachine dinnerStateMachine { get; private set; }

        public bool accelerating => _isInBattle;

        public override int idleAnimationHash => IdleAnimationHash.GetAnimation(shitDinner);
        public override int walkAnimationHash => WalkAnimationHash.GetAnimation(shitDinner);
        public override int runAnimationHash => RunAnimationHash.GetAnimation(shitDinner);
        public override int backAnimationHash => BackAnimationHash;

        public bool shitDinner { get; set; }

        protected override void Awake() {
            base.Awake();
            stateMachine = dinnerStateMachine = new DinnerStateMachine(this);
        }

        public void SetBattleOffset(bool inBattle) {
            m_InteractionObject.SetActive(!inBattle);
            _offset = inBattle ? BattleProvider.instance.characters.dinnerOffset : bastheet.dinnerOffset;
            shitDinner = _isInBattle;
        }

        public void ToggleBattleMode(bool inBattle) {
            _isInBattle = inBattle;
            SetBattleOffset(inBattle);
        }
    }
}
