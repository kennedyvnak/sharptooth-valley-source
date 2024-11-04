using System;
using System.Collections;
using DG.Tweening;
using NFHGame.AudioManagement;
using UnityEngine;

namespace NFHGame.Characters.StateMachines {
    public abstract class BastheetStateBase {
        public readonly BastheetStateMachine machine;
        public BastheetCharacterController bastheet => machine.bastheet;

        protected BastheetStateBase(BastheetStateMachine stateMachine) {
            this.machine = stateMachine;
        }

        public virtual void Enter(BastheetStateBase previousState) { }

        public virtual void Update() { }

        public virtual void UpdateAnimation(bool facingRight) { }

        public virtual void Exit() { }
    }

    public interface IBastheetInputState { }

    public sealed class BastheetValidationState : BastheetStateBase {
        public BastheetValidationState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            machine.EnterState(machine.idleState);
        }
    }

    public class BastheetIdleState : BastheetStateBase, IBastheetInputState {
        public BastheetIdleState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SetHorizontalVelocity(0.0f);
            bastheet.SwitchAnimation(BastheetCharacterController.IdleAnimationHashes);
            bastheet.footstepSource.Stop();
        }

        public override void UpdateAnimation(bool facingRight) {
            bastheet.SwitchAnimation(BastheetCharacterController.IdleAnimationHashes);
        }

        public override void Update() {
            if (bastheet.moveDirection != 0) {
                machine.EnterState(bastheet.isRunning ? machine.runState : machine.walkState);
            }
        }
    }

    public class BastheetWalkState : BastheetStateBase, IBastheetInputState {
        public BastheetWalkState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SwitchAnimation(BastheetCharacterController.WalkAnimationHashes);
            bastheet.footstepWalkSound.CloneToSource(bastheet.footstepSource);
            bastheet.footstepSource.Play();
        }

        public override void UpdateAnimation(bool facingRight) {
            bastheet.SwitchAnimation(BastheetCharacterController.WalkAnimationHashes);
        }

        public override void Update() {
            if (bastheet.moveDirection == 0) {
                machine.EnterState(machine.idleState);
            } else if (bastheet.isRunning) {
                machine.EnterState(machine.runState);
            } else {
                bastheet.SetHorizontalVelocity(bastheet.moveDirection * bastheet.moveSpeed);
                if (bastheet.UpdateAnimationByVelocity()) {
                    bastheet.SwitchAnimation(BastheetCharacterController.WalkAnimationHashes);
                }
            }
        }
    }

    public class BastheetRunState : BastheetStateBase, IBastheetInputState {
        public BastheetRunState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SwitchAnimation(BastheetCharacterController.RunAnimationHashes);
            bastheet.footstepRunSound.CloneToSource(bastheet.footstepSource);
            bastheet.footstepSource.Play();
        }

        public override void Update() {
            if (bastheet.moveDirection == 0) {
                machine.EnterState(machine.idleState);
            } else if (!bastheet.isRunning) {
                machine.EnterState(machine.walkState);
            } else {
                bastheet.SetHorizontalVelocity(bastheet.moveDirection * bastheet.runSpeed);
                if (bastheet.UpdateAnimationByVelocity()) {
                    bastheet.SwitchAnimation(BastheetCharacterController.RunAnimationHashes);
                }
            }
        }

        public override void UpdateAnimation(bool facingRight) {
            bastheet.SwitchAnimation(BastheetCharacterController.RunAnimationHashes);
        }
    }

    public class BastheetHitState : BastheetStateBase {
        public float stunSeconds { get; private set; }
        public BastheetHitState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SetHorizontalVelocity(0.0f);
            bastheet.SwitchAnimation(BastheetCharacterController.HittedAnimationHash);
            bastheet.footstepSource.Stop();
        }

        public override void Update() {
            stunSeconds -= Time.deltaTime;
            if (stunSeconds <= 0.0f) {
                stunSeconds = 0.0f;
                machine.EnterDefaultState();
            }
        }

        public void Hit(float stunTime) {
            stunSeconds += stunTime;
            if (machine.currentState != this) machine.EnterState(this);
        }
    }

    public class BastheetMoveState : BastheetStateBase {
        public float walkToPosition { get; private set; }
        public Func<float> walkToPositionLambda { get; private set; }
        public bool run { get; private set; }
        public bool flipX { get; private set; }

        private int direction { get; set; }
        private float speed { get; set; }

        public BastheetMoveState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            direction = (int)Mathf.Sign((walkToPosition - bastheet.rb.position.x));
            speed = run ? bastheet.runSpeed : bastheet.moveSpeed;
            (run ? bastheet.footstepRunSound : bastheet.footstepWalkSound).CloneToSource(bastheet.footstepSource);
            bastheet.SetHorizontalVelocity(speed * direction);
            if (flipX) bastheet.UpdateAnimationByVelocity();
            bastheet.SwitchAnimation(run ? BastheetCharacterController.RunAnimationHashes : BastheetCharacterController.WalkAnimationHashes);
            bastheet.footstepSource.Play();
        }

        public void WalkTo(float position, bool run = false, bool flipX = true) {
            walkToPositionLambda = null;
            walkToPosition = position;
            WalkTo(run, flipX);
        }

        public void WalkTo(Func<float> position, bool run = false, bool flipX = true) {
            walkToPositionLambda = position;
            walkToPosition = walkToPositionLambda();
            WalkTo(run, flipX);
        }

        private void WalkTo(bool run, bool flipX) {
            this.run = run;
            this.flipX = flipX;

            machine.EnterState(this);
        }

        public override void Update() {
            if (walkToPositionLambda != null) {
                walkToPosition = walkToPositionLambda();
            }

            if (bastheet.moveDirection != 0) {
                bastheet.StopAutoMove();
            } else if ((bastheet.rb.position.x > walkToPosition && direction == 1) || (bastheet.rb.position.x < walkToPosition && direction == -1)) {
                ReachEnd();
            } else {
                var velocity = speed * direction;
                bastheet.SetHorizontalVelocity(velocity);
                if (bastheet.CheckNextWalkPosition(bastheet.rb, velocity, walkToPosition, direction))
                    ReachEnd();
            }

            void ReachEnd() {
                var pos = bastheet.rb.position;
                pos.x = walkToPosition;
                bastheet.rb.position = pos;
                machine.EnterDefaultState();
            }
        }

        public IEnumerator WaitExit() {
            while (machine.currentState == this)
                yield return null;
        }
    }

    public class BastheetAvatarState : BastheetStateBase {
        public Vector2 endPos { get; private set; }
        public bool noisyStart { get; private set; }
        public bool noisyLoop { get; private set; }
        public bool autoGetUp { get; private set; }
        public int enterSound { get; private set; }
        public int loopSound { get; private set; }

        public PooledAudioHandler avatarEnterSound { get; private set; }
        public PooledAudioHandler avatarBgSound { get; private set; }

        private float startY { get; set; }
        private float gravity { get; set; }

        public event Action StartLoop;
        public event Action GetDown;
        public event Action EndAvatarState;

        public BastheetAvatarState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            startY = bastheet.rb.position.y;
            gravity = bastheet.rb.gravityScale;
            bastheet.rb.gravityScale = 0.0f;

            bastheet.SetHorizontalVelocity(0.0f);
            bastheet.footstepSource.Stop();
            if (noisyStart) NoisyState();
            else EnterState();
            bastheet.anim.SetBool("AvatarNoisy", noisyLoop);
            avatarEnterSound = AudioPool.instance.PlaySound(bastheet.avatarState.GetAvatarEnterSound(enterSound));
        }

        public override void Update() {
            var position = bastheet.shadowTransform.position;
            position.y = bastheet.shadowPositionY;
            bastheet.shadowTransform.position = position;
        }

        public override void Exit() {
            var position = bastheet.shadowTransform.position;
            position.y = bastheet.shadowPositionY;
            bastheet.shadowTransform.position = position;
        }

        public void EnterAvatarState(int enterSound, int loopSound, bool autoGetUp, bool noisyStart, bool noisyLoop) => EnterAvatarState(enterSound, loopSound, bastheet.avatarState.endY, autoGetUp, noisyStart, noisyLoop);

        public void EnterAvatarState(int enterSound, int loopSound, float endY, bool autoGetUp, bool noisyStart, bool noisyLoop) => EnterAvatarState(enterSound, loopSound, new Vector2(bastheet.transform.position.x, endY), autoGetUp, noisyStart, noisyLoop);

        public void EnterAvatarState(int enterSound, int loopSound, Vector2 endPos, bool autoGetUp, bool noisyStart, bool noisyLoop) {
            this.autoGetUp = autoGetUp;
            this.endPos = endPos;
            this.noisyStart = noisyStart;
            this.noisyLoop = noisyLoop;
            this.enterSound = enterSound;
            this.loopSound = loopSound;
            machine.EnterState(this);
        }

        public Tweener GoDownLikeEnter(bool right = false) {
            bastheet.SwitchAnimation(right ? BastheetCharacterController.AvatarStateExitAnimationHashes.right : BastheetCharacterController.AvatarStateEpicExitAnimationHash);
            return bastheet.rb.DOMoveY(startY, right ? bastheet.avatarState.gentleDown : bastheet.avatarState.enterDuration)
                .SetEase(bastheet.avatarState.upCurve)
                .SetTarget(this);
        }

        public void StopSounds() {
            if (avatarBgSound.source) avatarBgSound.source.Stop();
            if (avatarEnterSound.source) avatarEnterSound.source.Stop();
        }

        private void NoisyState() {
            bastheet.SwitchAnimation(BastheetCharacterController.AvatarStateNoisyAnimationHash);
            DOVirtual.DelayedCall(bastheet.avatarState.noisyDuration, EnterState).SetTarget(this);
        }

        private void EnterState() {
            avatarBgSound = AudioPool.instance.PlaySound(bastheet.avatarState.GetAvatarSound(loopSound));
            bastheet.SwitchAnimation(BastheetCharacterController.AvatarStateEnterAnimationHashes);
            bastheet.rb.DOMove(endPos, bastheet.avatarState.enterDuration)
                .SetEase(bastheet.avatarState.upCurve)
                .SetTarget(this)
                .OnComplete(LoopState);
        }

        private void LoopState() {
            bastheet.rb.DOMoveY(startY, bastheet.avatarState.downDuration)
               .SetEase(bastheet.avatarState.downCurve)
               .SetDelay(bastheet.avatarState.gravityReturnTime)
               .SetTarget(this)
               .OnStart(() => bastheet.SwitchAnimation(BastheetCharacterController.AvatarStateExitAnimationHashes))
               .OnComplete(DownState);
            StartLoop?.Invoke();
        }

        private void DownState() {
            bastheet.rb.gravityScale = gravity;
            if (autoGetUp) {
                bastheet.SwitchAnimation(BastheetCharacterController.SitAnimationHash);
                DOVirtual.DelayedCall(bastheet.avatarState.sitAnimDuration, GetUpState).SetTarget(this);
            } else {
                StopSounds();
                AudioPool.instance.PlaySound(bastheet.avatarState.downSound);
                bastheet.SwitchAnimation(BastheetCharacterController.DownAnimationHash);
            }
            HaloManager.HaloManager.instance.ForceToggle(false);
            GetDown?.Invoke();
        }

        private void GetUpState() {
            bastheet.SwitchAnimation(BastheetCharacterController.GetUpAnimationHash);
            DOVirtual.DelayedCall(bastheet.avatarState.cleanAnimDuration, () => {
                machine.EnterDefaultState();
                EndAvatarState?.Invoke();
            }).SetTarget(this);
        }
    }

    public class BastheetPickState : BastheetStateBase {
        public Transform target { get; private set; }

        private bool _holding;

        public BastheetPickState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SetHorizontalVelocity(0.0f);
            if (_holding)
                bastheet.SwitchAnimation(BastheetCharacterController.PickLoopStateAnimationHash);
            else
                bastheet.SwitchAnimation(BastheetCharacterController.PickStateAnimationHash);
            bastheet.footstepSource.Stop();
            _holding = true;
        }

        public override void Exit() {
            bastheet.transform.Find("Hands").gameObject.SetActive(false);
        }

        public void Pick(Transform target, bool holding = false) {
            _holding = holding;
            this.target = target;
            target.SetParent(bastheet.pickItemParent);
            machine.EnterState(this);
        }

        public void DestroyPickObject() {
            GameObject.Destroy(target.gameObject);
            machine.EnterDefaultState();
        }
    }

    public class BastheetDropState : BastheetStateBase {
        public BastheetDropState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SetHorizontalVelocity(0.0f);
            bastheet.SwitchAnimation(BastheetCharacterController.DropItemAnimationHash);
            bastheet.footstepSource.Stop();
        }

        public void StartDrop(Sprite sprite) {
            bastheet.dropItemRenderer.sprite = sprite;
            machine.EnterState(this);
        }

        public void DropOnPool() {
            bastheet.SwitchAnimation(BastheetCharacterController.CrouchedDropingItemAnimationHash);
        }
    }

    public class BastheetKinematicState : BastheetStateBase {
        public BastheetKinematicState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SetHorizontalVelocity(0.0f);
            bastheet.rb.isKinematic = true;
            bastheet.SwitchAnimation(BastheetCharacterController.IdleAnimationHashes);
            bastheet.footstepSource.Stop();
        }

        public override void Exit() {
            bastheet.rb.isKinematic = false;
        }

        public override void UpdateAnimation(bool facingRight) {
            bastheet.SwitchAnimation(BastheetCharacterController.IdleAnimationHashes);
        }

        public override void Update() {
            bastheet.SetHorizontalVelocity(0.0f);
        }
    }

    public class BastheetAnimState : BastheetStateBase {
        public int animationHash { get; private set; }
        public BastheetCharacterController.AnimatorAnimationHashes composedHash;

        public BastheetAnimState(BastheetStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter(BastheetStateBase previousState) {
            bastheet.SetHorizontalVelocity(0.0f);
            if (animationHash != 0) bastheet.SwitchAnimation(animationHash);
            else bastheet.SwitchAnimation(composedHash);
            bastheet.footstepSource.Stop();
        }

        public override void UpdateAnimation(bool facingRight) {
            if (animationHash == 0) bastheet.SwitchAnimation(composedHash);
        }

        public void Animate(int animHash) {
            animationHash = animHash;
            machine.EnterState(this);
        }

        public void Animate(BastheetCharacterController.AnimatorAnimationHashes animHash) {
            animationHash = 0;
            composedHash = animHash;
            machine.EnterState(this);
        }
    }
}