using System;
using System.Collections;
using UnityEngine;

namespace NFHGame.Characters.StateMachines {
    public abstract class FollowerStateBase {
        public readonly FollowerStateMachine machine;
        public FollowerCharacterController follower => machine.follower;
        public BastheetCharacterController bastheet => machine.follower.bastheet;

        protected FollowerStateBase(FollowerStateMachine stateMachine) {
            this.machine = stateMachine;
        }

        public virtual void Enter(FollowerStateBase previousState) { }

        public virtual void Update() { }

        public virtual void Exit() { }
    }

    public sealed class FollowerValidationState : FollowerStateBase {
        public FollowerValidationState(FollowerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(FollowerStateBase previousState) {
            machine.EnterState(machine.followState);
        }
    }

    public class FollowerFollowState : FollowerStateBase {
        public FollowerFollowState(FollowerStateMachine stateMachine) : base(stateMachine) { }

        public override void Update() {
            float characterPosition = bastheet.rb.position.x;
            float followerPosition = follower.rb.position.x;
            float distance = characterPosition - followerPosition;
            float absDistance = Mathf.Abs(distance);
            follower.SetFacingDirection((int)Mathf.Sign(distance));

            if (absDistance > follower.offset + 0.1f) {
                follower.velocity.x = bastheet.currentMoveSpeed * follower.facingDirection * follower.runFactor;
                follower.running = true;
            } else if (absDistance > follower.offset) {
                follower.velocity.x = bastheet.currentMoveSpeed * follower.facingDirection;
                follower.running = bastheet.isRunning;
            } else {
                follower.velocity.x = 0.0f;
                follower.running = false;
            }

            follower.SwitchAnimation(Mathf.Abs(follower.velocity.x) <= 0.0f ? follower.idleAnimationHash : follower.running ? follower.runAnimationHash : follower.walkAnimationHash);
        }
    }

    public class FollowerAnimState : FollowerStateBase {
        private int _lastAnimHash;
        private bool _useAnimateFunctionToEnter;

        public FollowerAnimState(FollowerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(FollowerStateBase previousState) {
            if (!_useAnimateFunctionToEnter) {
                GameLogger.LogError("Don't use Enter(FollowerStateBase) directly in AnimState. Use Animate(int) instead");
            }
            follower.velocity.x = 0.0f;
            follower.SwitchAnimation(_lastAnimHash);
            _useAnimateFunctionToEnter = false;
        }

        public void Animate(int hash) {
            _useAnimateFunctionToEnter = true;
            _lastAnimHash = hash;
            machine.EnterState(this);
        }
    }

    public class FollowerMoveState : FollowerStateBase {
        public float endPosition { get; private set; }
        public Func<float> walkToPositionLambda { get; private set; }
        public bool run { get; private set; }
        public int animHash { get; private set; }
        public bool flipX { get; private set; }

        private int direction { get; set; }
        private float speed { get; set; }

        public FollowerMoveState(FollowerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(FollowerStateBase previousState) {
            direction = (int)Mathf.Sign((endPosition - follower.rb.position.x));
            speed = run ? follower.bastheet.runSpeed : follower.bastheet.moveSpeed;
            follower.velocity.x = speed * direction;
            if (flipX) follower.SetFacingDirection(direction);
            follower.SwitchAnimation(animHash == 0 ? (run ? follower.runAnimationHash : follower.walkAnimationHash) : animHash);
        }

        public void MoveTo(float endPosition, out int faceDir, int animHash = 0, bool run = false, bool flipX = true) {
            walkToPositionLambda = null;
            this.endPosition = endPosition;
            MoveTo(out faceDir, animHash, run, flipX);
        }

        public void MoveTo(Func<float> getPosition, out int faceDir, int animHash = 0, bool run = false, bool flipX = true) {
            walkToPositionLambda = getPosition;
            this.endPosition = walkToPositionLambda();
            MoveTo(out faceDir, animHash, run, flipX);
        }

        private void MoveTo(out int faceDir, int animHash = 0, bool run = false, bool flipX = true) {
            this.run = run;
            this.animHash = animHash;
            this.flipX = flipX;

            machine.EnterState(this);
            faceDir = direction;
        }

        public override void Update() {
            if (walkToPositionLambda != null) {
                endPosition = walkToPositionLambda();
            }

            if ((follower.rb.position.x > endPosition && direction == 1) || (follower.rb.position.x < endPosition && direction == -1)) {
                ReachEnd();
            } else {
                follower.velocity.x = speed * direction;
                if (bastheet.CheckNextWalkPosition(follower.rb, follower.velocity.x, endPosition, direction)) {
                    ReachEnd();
                }
            }

            void ReachEnd() {
                var pos = follower.rb.position;
                pos.x = endPosition;
                follower.rb.position = pos;
                machine.EnterState(machine.followState);
            }
        }

        public IEnumerator WaitExit() {
            while (machine.currentState == this)
                yield return null;
        }
    }

    public class FollowerFollowLimitedState : FollowerStateBase {
        public float limitLeft = float.NegativeInfinity, limitRight = float.PositiveInfinity;

        public FollowerFollowLimitedState(FollowerStateMachine stateMachine) : base(stateMachine) { }

        public override void Update() {
            float characterPosition = bastheet.rb.position.x;
            float followerPosition = follower.rb.position.x;
            float distance = characterPosition - followerPosition;
            float absDistance = Mathf.Abs(distance);
            follower.SetFacingDirection((int)Mathf.Sign(distance));

            if (absDistance > follower.offset + 0.1f && CheckLimits()) {
                follower.velocity.x = bastheet.currentMoveSpeed * follower.facingDirection * follower.runFactor;
                follower.running = true;
            } else if (absDistance > follower.offset && CheckLimits()) {
                follower.velocity.x = bastheet.currentMoveSpeed * follower.facingDirection;
                follower.running = bastheet.isRunning;
            } else {
                follower.velocity.x = 0.0f;
                follower.running = false;
            }

            follower.SwitchAnimation(Mathf.Abs(follower.velocity.x) <= 0.0f ? follower.idleAnimationHash : follower.running ? follower.runAnimationHash : follower.walkAnimationHash);

            bool CheckLimits() {
                return followerPosition >= limitLeft && followerPosition <= limitRight;
            }
        }
    }
}