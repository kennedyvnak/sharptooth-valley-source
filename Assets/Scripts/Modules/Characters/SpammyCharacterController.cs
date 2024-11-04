using NFHGame.Characters.StateMachines;
using UnityEngine;

namespace NFHGame.Characters {
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpammyCharacterController : FollowerCharacterController {
        public static readonly int IdleAnimationHash = Animator.StringToHash("SpammyIdle");
        public static readonly int WalkAnimationHash = Animator.StringToHash("SpammyWalk");
        public static readonly int RunAnimationHash = Animator.StringToHash("SpammyRun");
        public static readonly int BackAnimationHash = Animator.StringToHash("SpammyBack");
        public static readonly int ThinkAnimationHash = Animator.StringToHash("SpammyThink");
        public static readonly int ShootingAlphaAAnimationHash = Animator.StringToHash("SpammyShootingAlphaA");
        public static readonly int ShootingAlphaBAnimationHash = Animator.StringToHash("SpammyShootingAlphaB");
        public static readonly int ShootingAlphaCAnimationHash = Animator.StringToHash("SpammyShootingAlphaC");
        public static readonly int ShootingNormalBAnimationHash = Animator.StringToHash("SpammyShootingNormalB");
        public static readonly int ShootingReleaseAnimationHash = Animator.StringToHash("SpammyShootingRELEASE");
        public static readonly int ShootingReleasedAnimationHash = Animator.StringToHash("SpammyShootingReleased");
        public static readonly int RunCryingAnimationHash = Animator.StringToHash("SpammyRunCRYING");
        public static readonly int EpicEnterAnimationHash = Animator.StringToHash("SpammyEpicEnter");

        public SpammyStateMachine spammyStateMachine { get; private set; }

        public override int idleAnimationHash => IdleAnimationHash;
        public override int walkAnimationHash => WalkAnimationHash;
        public override int runAnimationHash => RunAnimationHash;
        public override int backAnimationHash => BackAnimationHash;

        protected override void Awake() {
            base.Awake();
            stateMachine = spammyStateMachine = new SpammyStateMachine(this);
        }
    }
}
