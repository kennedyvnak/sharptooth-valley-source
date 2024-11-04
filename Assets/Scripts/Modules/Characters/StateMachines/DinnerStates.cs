using NFHGame.Battle;
using UnityEngine;

namespace NFHGame.Characters.StateMachines {
    public abstract class DinnerStateBase : FollowerStateBase {
        public readonly DinnerStateMachine dinnerMachine;
        public DinnerCharacterController dinner => dinnerMachine.dinner;

        protected DinnerStateBase(FollowerStateMachine stateMachine) : base(stateMachine) {
            dinnerMachine = stateMachine as DinnerStateMachine;
        }
    }

    public class DinnerFollowState : FollowerFollowState {
        public readonly DinnerStateMachine dinnerMachine;
        public DinnerCharacterController dinner => dinnerMachine.dinner;
        public float accelerationCooldown { get; set; }

        public DinnerFollowState(FollowerStateMachine stateMachine) : base(stateMachine) {
            dinnerMachine = stateMachine as DinnerStateMachine;
        }

        public override void Enter(FollowerStateBase previousState) {
            accelerationCooldown = BattleProvider.instance.characters.dinnerAccelerationCooldown;
        }

        public override void Update() {
            if (dinner.accelerating && dinner.running) {
                accelerationCooldown -= Time.deltaTime;
                if (accelerationCooldown <= 0.0f) {
                    machine.EnterState(dinnerMachine.breathlessState);
                    return;
                }
            }

            base.Update();
        }
    }

    public class DinnerBreathlessState : DinnerStateBase {
        private float accelerationTime { get; set; }
        public DinnerBreathlessState(DinnerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(FollowerStateBase previousState) {
            accelerationTime = 0.0f;
        }

        public override void Update() {
            float characterPosition = bastheet.rb.position.x;
            float followerPosition = dinner.rb.position.x;
            float distance = characterPosition - followerPosition;
            float absDistance = Mathf.Abs(distance);
            dinner.SetFacingDirection((int)Mathf.Sign(distance));

            var accelerationData = BattleProvider.instance.characters;
            float acceleration = accelerationData.dinnerAccelerationCurve.Evaluate(accelerationTime / accelerationData.dinnerAccelerationDuration);

            if (absDistance > dinner.offset) {
                var far = absDistance > dinner.offset + 0.1f;
                dinner.velocity.x = far ? bastheet.runSpeed * dinner.facingDirection * acceleration : bastheet.moveSpeed * dinner.facingDirection;
                dinner.running = far || bastheet.isRunning;
            } else {
                dinner.velocity.x = 0.0f;
                dinner.running = false;
            }

            accelerationTime += Time.deltaTime;
            if (accelerationTime >= accelerationData.dinnerAccelerationDuration) {
                machine.EnterState(machine.followState);
            }

            dinner.SwitchAnimation(Mathf.Abs(dinner.velocity.x) <= 0.0f ? follower.idleAnimationHash : follower.runAnimationHash);
        }
    }

    public class DinnerHitState : DinnerStateBase {
        public float stunSeconds { get; private set; }
        public bool knockout { get; private set; }
        public float knockoutUpTime { get; set; }

        public DinnerHitState(DinnerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(FollowerStateBase previousState) {
            knockoutUpTime = BattleProvider.instance.characters.dinnerKnockoutUpTime;
            dinner.velocity.x = 0.0f;
            if (knockout)
                dinner.SwitchAnimation(DinnerCharacterController.KnockedOutAnimationHash);
            else
                dinner.SwitchAnimation(DinnerCharacterController.HittedAnimationHash);
        }

        public override void Update() {
            stunSeconds -= Time.deltaTime;
            if (stunSeconds <= knockoutUpTime && knockout) {
                dinner.SwitchAnimation(DinnerCharacterController.KnockedUpAnimationHash);
            }
            if (stunSeconds <= 0.0f) {
                stunSeconds = 0.0f;
                machine.EnterState(machine.followState);
            }
        }

        public void Hit(float stunTime, bool knockout) {
            stunSeconds += stunTime;
            if (machine.currentState != this || (!this.knockout && knockout)) {
                this.knockout = knockout;
                machine.EnterState(this);
            }
        }
    }
}