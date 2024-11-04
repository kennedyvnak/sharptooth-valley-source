namespace NFHGame.Characters.StateMachines {
    public class SpammyStateMachine : FollowerStateMachine {
        public SpammyArrowBattleState arrowBattleSpammy { get; private set; }

        public SpammyStateMachine(FollowerCharacterController follower) : base(follower) {
            followState = new FollowerFollowState(this);
            followLimitedState = new FollowerFollowLimitedState(this);
            animState = new FollowerAnimState(this);
            moveState = new FollowerMoveState(this);
            arrowBattleSpammy = new SpammyArrowBattleState(this);
            Init();
        }
    }
}