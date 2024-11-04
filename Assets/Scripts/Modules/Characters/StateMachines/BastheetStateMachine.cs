using UnityEngine.Events;

namespace NFHGame.Characters.StateMachines {
    public class BastheetStateMachine {
        public readonly BastheetCharacterController bastheet;

        public BastheetStateBase currentState { get; private set; }

        public readonly BastheetIdleState idleState;
        public readonly BastheetWalkState walkState;
        public readonly BastheetRunState runState;
        public readonly BastheetHitState hitState;
        public readonly BastheetMoveState moveState;
        public readonly BastheetAvatarState avatarState;
        public readonly BastheetPickState pickState;
        public readonly BastheetDropState dropState;
        public readonly BastheetKinematicState kinematicState;
        public readonly BastheetAnimState animState;

        public BastheetStateMachine(BastheetCharacterController bastheet) {
            this.bastheet = bastheet;

            currentState = new BastheetValidationState(this);
            idleState = new BastheetIdleState(this);
            walkState = new BastheetWalkState(this);
            runState = new BastheetRunState(this);
            hitState = new BastheetHitState(this);
            moveState = new BastheetMoveState(this);
            avatarState = new BastheetAvatarState(this);
            pickState = new BastheetPickState(this);
            dropState = new BastheetDropState(this);
            kinematicState = new BastheetKinematicState(this);
            animState = new BastheetAnimState(this);

            currentState.Enter(null);
        }

        public virtual void EnterState(BastheetStateBase state) {
            BastheetStateBase previousState = currentState;
            currentState = state;
            previousState.Exit();
            state.Enter(previousState);
        }

        public void EnterDefaultState() {
            EnterState(bastheet.moveDirection == 0 ? idleState : bastheet.isRunning ? runState : walkState);
        }
    }
}