namespace NFHGame.Characters.StateMachines {
    public class DinnerStateMachine : FollowerStateMachine {
        public readonly DinnerCharacterController dinner;

        public DinnerBreathlessState breathlessState { get; private set; }
        public DinnerHitState hitState { get; private set; }

        public DinnerStateMachine(FollowerCharacterController follower) : base(follower) {
            this.dinner = follower as DinnerCharacterController;
            followState = new DinnerFollowState(this);
            followLimitedState = new FollowerFollowLimitedState(this);
            animState = new FollowerAnimState(this);
            moveState = new FollowerMoveState(this);
            breathlessState = new DinnerBreathlessState(this);
            hitState = new DinnerHitState(this);
            Init();
        }
    }
}