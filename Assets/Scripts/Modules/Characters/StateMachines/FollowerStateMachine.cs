using UnityEngine.Events;

namespace NFHGame.Characters.StateMachines {
    public class FollowerStateMachine {
        public readonly FollowerCharacterController follower;

        public FollowerStateBase currentState { get; private set; }

        public FollowerFollowState followState { get; protected set; }
        public FollowerFollowLimitedState followLimitedState { get; protected set; }
        public FollowerAnimState animState { get; protected set; }
        public FollowerMoveState moveState { get; protected set; }

        public FollowerStateMachine(FollowerCharacterController follower) {
            this.follower = follower;
        }

        public void Init() {
            currentState = new FollowerValidationState(this);
            currentState.Enter(null);
        }

        public virtual void EnterState(FollowerStateBase state) {
            FollowerStateBase previousState = currentState;
            currentState = state;
            previousState.Exit();
            state.Enter(previousState);
        }

        public void EnterDefaultState() {
            EnterState(followState);
        }
    }
}