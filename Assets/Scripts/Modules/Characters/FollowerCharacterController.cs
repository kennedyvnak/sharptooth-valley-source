using System;
using System.Collections;
using DG.Tweening;
using NFHGame.Characters.StateMachines;
using UnityEngine;

namespace NFHGame.Characters {
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class FollowerCharacterController : GameCharacterController {
        [SerializeField] protected float m_RunFactor = 5.0f / 3.0f;
        [SerializeField] protected SpriteRenderer m_Renderer;
        [SerializeField] protected GameObject m_InteractionObject;
        [SerializeField] protected FootstepsSound m_FootstepsSound;

        protected float _offset;
        protected BastheetCharacterController _bastheet;
        protected int _currentAnimationHash;
        protected bool _dirtyAnimHash;

        public FollowerStateMachine stateMachine { get; protected set; }

        [System.NonSerialized] public Vector2 velocity;
        [System.NonSerialized] public bool running;

        public float offset => _offset;
        public float runFactor => m_RunFactor;

        public virtual int idleAnimationHash { get; }
        public virtual int walkAnimationHash { get; }
        public virtual int runAnimationHash { get; }
        public virtual int backAnimationHash { get; }

        public SpriteRenderer spriteRenderer => m_Renderer;
        public BastheetCharacterController bastheet => _bastheet;

        public void Init(float offset, BastheetCharacterController bastheet) {
            _bastheet = bastheet;
            _offset = offset;
        }

        protected virtual void Update() {
            stateMachine.currentState.Update();
            m_FootstepsSound.UpdateSound(velocity.x, running);

            if (_dirtyAnimHash) {
                anim.Play(_currentAnimationHash);
                _dirtyAnimHash = false;
            }
        }

        protected virtual void FixedUpdate() {
            rb.velocity = new Vector2(velocity.x, rb.velocity.y);
        }

        protected virtual void OnDestroy() {
            this.DOKill();
        }

        public override void ToggleLookBack(bool lookBack) {
            if (lookBack) {
                stateMachine.animState.Animate(backAnimationHash);
            } else {
                stateMachine.EnterState(stateMachine.followState);
            }
        }

        public void SetFacingDirection(int direction) {
            if (direction != facingDirection) {
                facingDirection = direction;
                spriteRenderer.flipX = direction == -1;
            }
        }

        public void SwitchAnimation(int animationHash) {
            if (!_dirtyAnimHash) _dirtyAnimHash = _currentAnimationHash != animationHash;
            _currentAnimationHash = animationHash;
        }

        public IEnumerator WalkOut(float finalPositionX, int faceDir = 0, bool run = false, int animHash = 0, bool flipX = true) {
            if (bastheet.CheckWalkPosition(rb, finalPositionX, run)) yield break;
            stateMachine.moveState.MoveTo(finalPositionX, out int autoFaceDir, animHash: animHash, run: run, flipX: flipX);
            yield return WaitWalkOut(faceDir == 0 ? autoFaceDir : faceDir, flipX);
        }

        public IEnumerator WalkOut(Func<float> finalPositionX, int faceDir = 0, bool run = false, int animHash = 0, bool flipX = true) {
            if (bastheet.CheckWalkPosition(rb, finalPositionX(), run)) yield break;
            stateMachine.moveState.MoveTo(finalPositionX, out int autoFaceDir, animHash: animHash, run: run, flipX: flipX);
            yield return WaitWalkOut(faceDir == 0 ? autoFaceDir : faceDir, flipX);
        }

        protected IEnumerator WaitWalkOut(int faceDir, bool flipX) {
            yield return stateMachine.moveState.WaitExit();

            if (flipX && faceDir != 0)
                SetFacingDirection(faceDir);
        }

        public override void SetFacingDirection(bool facingRight) {
            SetFacingDirection(facingRight ? 1 : -1);
        }
    }
}
