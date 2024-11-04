using UnityEngine;
using NFHGame.Input;
using System.Collections;
using NFHGame.RangedValues;
using NFHGame.Interaction.Input;
using NFHGame.Interaction.Behaviours;
using NFHGame.Battle;
using NFHGame.AudioManagement;
using NFHGame.Characters.StateMachines;
using System;
using NFHGame.Interaction;

namespace NFHGame.Characters {
    [RequireComponent(typeof(Rigidbody2D))]
    public class BastheetCharacterController : GameCharacterController {
        public readonly struct AnimatorAnimationHashes {
            public readonly int left;
            public readonly int right;

            public AnimatorAnimationHashes(string left, string right) {
                this.left = Animator.StringToHash(left);
                this.right = Animator.StringToHash(right);
            }

            public AnimatorAnimationHashes(int left, int right) {
                this.left = left;
                this.right = right;
            }

            public int GetAnimation(int direction) {
                return direction == -1 ? left : right;
            }

            public int GetAnimation(bool isRight) {
                return isRight ? right : left;
            }
        }

        [System.Serializable]
        public class AvatarState {
            public float endY;
            public AnimationCurve upCurve, downCurve;
            public AudioProviderObject downSound, avatarStateSound;
            public float noisyDuration, enterDuration, downDuration, gravityReturnTime, cleanAnimDuration, sitAnimDuration, gentleDown;
            public AudioProviderObject[] enterSounds;
            public AudioProviderObject[] loopSounds;

            public AudioProviderObject GetAvatarEnterSound(int idx) => enterSounds[idx];
            public AudioProviderObject GetAvatarSound(int idx) => loopSounds[idx];
        }

        public static readonly AnimatorAnimationHashes IdleAnimationHashes = new AnimatorAnimationHashes("IdleLeft", "IdleRight");
        public static readonly AnimatorAnimationHashes WalkAnimationHashes = new AnimatorAnimationHashes("WalkLeft", "WalkRight");
        public static readonly AnimatorAnimationHashes RunAnimationHashes = new AnimatorAnimationHashes("RunLeft", "RunRight");
        public static readonly AnimatorAnimationHashes BackAnimationHashes = new AnimatorAnimationHashes("BackLeft", "BackRight");

        public static readonly AnimatorAnimationHashes HeadacheAnimationHashes = new AnimatorAnimationHashes("HeadacheL", "HeadacheR");
        public static readonly AnimatorAnimationHashes IdleEpicAnimationHashes = new AnimatorAnimationHashes("IdleEpicLeft", "IdleEpicRight");

        public static readonly AnimatorAnimationHashes AvatarStateEnterAnimationHashes = new AnimatorAnimationHashes("AvatarStateEpicEnterL", "AvatarStateEpicEnterR");
        public static readonly AnimatorAnimationHashes AvatarStateExitAnimationHashes = new AnimatorAnimationHashes("AvatarStateGentleExit", "AvatarStateGentleExit");

        public static readonly int HittedAnimationHash = Animator.StringToHash("Hitted");
        public static readonly int SitAnimationHash = Animator.StringToHash("Sit");
        public static readonly int DownAnimationHash = Animator.StringToHash("Down");
        public static readonly int GetUpAnimationHash = Animator.StringToHash("GetUp");
        public static readonly int AvatarStateNoisyAnimationHash = Animator.StringToHash("AvatarStateNoisy");
        public static readonly int IdleEyeClosedAnimationHash = Animator.StringToHash("IdleEyesClosedLeft");
        public static readonly int AvatarStateEpicExitAnimationHash = Animator.StringToHash("AvatarStateEpicExitL");
        public static readonly int PickStateAnimationHash = Animator.StringToHash("Pickitem");
        public static readonly int DropItemAnimationHash = Animator.StringToHash("DropItem");
        public static readonly int CrouchedDropingItemAnimationHash = Animator.StringToHash("CrouchedDropingItem");
        public static readonly int PickLoopStateAnimationHash = Animator.StringToHash("PickitemLoop");
        public static readonly int ShockStateAnimationHash = Animator.StringToHash("Shocked");
        public static readonly int ThrowAmensiaRockAnimHash = Animator.StringToHash("ThrowAmnesiaRock");
        public static readonly int PickAmensiaRockAnimHash = Animator.StringToHash("PickAmnesiaRock");
        public static readonly int DrowedAnimationHash = Animator.StringToHash("Drowned");
        public static readonly int BastheetCannonShotAnimationHash = Animator.StringToHash("BastheetCannonShot");

        [Header("Movement")]
        [SerializeField] private float m_MoveSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] private Transform m_ShadowTransform;

        [Header("Sounds")]
        [SerializeField] private AudioSource m_FootstepSource;
        [SerializeField] private AudioObject m_FootstepWalkSound;
        [SerializeField] private AudioObject m_FootstepRunSound;

        [Header("Followers")]
        [SerializeField] private float m_DinnerOffset;
        [SerializeField] private float m_SpammyOffset;

        [Header("Interaction")]
        [SerializeField, RangedValue(-1000.0f, 1000.0f)] private RangedFloat m_RaycastHeightRange = new RangedFloat(-100.0f, 100.0f);
        [SerializeField] private LayerMask m_GroundLayer;
        [SerializeField] private Interactor m_Interactor;
        [SerializeField] private LayerMask m_IgnoreClicksLayer;
        [SerializeField, RangedValue(-1000.0f, 1000.0f)] private RangedFloat m_RaycastRange;

        [SerializeField] private AvatarState m_AvatarState;

        [Header("Pick item")]
        [SerializeField] private Vector2 m_PickItemOffset;
        [SerializeField] private Transform m_PickItemParent;
        [SerializeField] private float m_PickObjectTime;
        [SerializeField] private SpriteRenderer m_DropItemRenderer;

        public BastheetStateMachine stateMachine { get; private set; }

        private InputClickController _clickController;
        private readonly Coroutine[] _moveCoroutine = new Coroutine[3];

        private bool _dirtyAnimHash;
        private int _currentAnimationHash;
        private Vector2 _velocity;

        public float shadowPositionY { get; private set; }
        public int moveDirection { get; private set; }
        public bool isRunning { get; private set; }
        public BastheetForceField forceField { get; private set; }

        public float moveSpeed => m_MoveSpeed;
        public float runSpeed => m_RunSpeed;
        public Transform pickItemParent => m_PickItemParent;
        public AudioSource footstepSource => m_FootstepSource;
        public AudioObject footstepWalkSound => m_FootstepWalkSound;
        public AudioObject footstepRunSound => m_FootstepRunSound;
        public float dinnerOffset => m_DinnerOffset;
        public float spammyOffset => m_SpammyOffset;
        public AvatarState avatarState => m_AvatarState;
        public Transform shadowTransform => m_ShadowTransform;
        public SpriteRenderer dropItemRenderer { get => m_DropItemRenderer; set => m_DropItemRenderer = value; }
        public float currentMoveSpeed => isRunning ? m_RunSpeed : m_MoveSpeed;

        protected override void Awake() {
            base.Awake();

            _clickController = new InputClickController(m_IgnoreClicksLayer, m_RaycastRange, m_RaycastHeightRange, m_GroundLayer);
            m_Interactor.walkToInteractionObjectTrigger.AddListener(TRIGGER_WalkToObject);
            m_Interactor.setupInteractionObjectTrigger.AddListener(TRIGGER_SetupInteraction);

            stateMachine = new BastheetStateMachine(this);
            shadowPositionY = m_ShadowTransform.position.y;
        }

        private void OnEnable() {
            InputReader.instance.OnMoveAxis += INPUT_MoveAxis;
            InputReader.instance.OnMouseClick += INPUT_OnClick;
            InputReader.instance.OnRun += INPUT_Run;
        }

        private void OnDisable() {
            InputReader.instance.OnMoveAxis -= INPUT_MoveAxis;
            InputReader.instance.OnMouseClick -= INPUT_OnClick;
            InputReader.instance.OnRun -= INPUT_Run;
        }

        protected override void Start() {
            base.Start();

            GameCharactersManager.instance.dinner.Init(m_DinnerOffset, this);
            if (GameManager.instance.spammyInParty)
                GameCharactersManager.instance.spammy.Init(m_SpammyOffset, this);
        }

        private void Update() {
            stateMachine.currentState.Update();

            if (_dirtyAnimHash) {
                anim.Play(_currentAnimationHash);
                _dirtyAnimHash = false;
            }
        }

        private void FixedUpdate() {
            rb.velocity = _velocity;
        }

        public bool UpdateAnimationByVelocity() {
            if (facingDirection == 1 && _velocity.x < 0.0f || facingDirection == -1 && _velocity.x > 0.0f) {
                facingDirection *= -1;
                return true;
            }
            return false;
        }

        public void SwitchAnimation(int animationHash) {
            _currentAnimationHash = animationHash;
            _dirtyAnimHash = true;
        }

        public void SwitchAnimation(AnimatorAnimationHashes animationHash) => SwitchAnimation(animationHash.GetAnimation(facingDirection));

        public void SetHorizontalVelocity(float velocity) => _velocity = new Vector2(velocity, rb.velocity.y);

        public override void SetPosition(Vector3 position, bool facingRight) {
            base.SetPosition(position, facingRight);
            DirtyCinemachineFollow();
        }

        private void DirtyCinemachineFollow() {
            if (Helpers.vCam.Follow == transform) Helpers.vCam.PreviousStateIsValid = false;
        }

        public override void ToggleLookBack(bool lookBack) {
            if (lookBack) stateMachine.animState.Animate(BackAnimationHashes.GetAnimation(facingDirection));
            else stateMachine.EnterDefaultState();
        }

        public override void SetFacingDirection(bool facingRight) {
            base.SetFacingDirection(facingRight);
            stateMachine?.currentState?.UpdateAnimation(facingRight);
        }

        public void InitSpammy(SpammyCharacterController spammy) {
            GameCharactersManager.instance.UpdateCharacters(true);
            spammy.Init(m_SpammyOffset, this);
        }

        private IEnumerator WalkToObject(InteractionObject interactionObject) {
            var dinner = GameCharactersManager.instance.dinner;
            var spammy = GameCharactersManager.instance.spammy;
            var spammyInParty = GameManager.instance.spammyInParty;

            if (interactionObject.TryGetComponent<InteractionPositions>(out var pos)) {
                _moveCoroutine[0] = pos.ingoreBast ? null : StartCoroutine(WalkToPosition(pos.bastPos, faceDir: GetFaceDir(pos.bastPos, rb.position.x, pos.bastDir)));
                _moveCoroutine[1] = pos.ignoreDinner ? null : MoveCharacter(dinner, pos.dinnerPos, pos.dinnerDir);
                _moveCoroutine[2] = !spammyInParty || pos.ignoreSpammy ? null : MoveCharacter(spammy, pos.spamPos, pos.spamDir);

                foreach (var coroutine in _moveCoroutine)
                    if (coroutine != null) yield return coroutine;

                Coroutine MoveCharacter(FollowerCharacterController character, float target, bool dir) => StartCoroutine(character.WalkOut(target, faceDir: GetFaceDir(target, character.rb.position.x, dir)));

                int GetFaceDir(float targetPos, float charPos, bool right) => (pos.setFaceDir ? targetPos < charPos : right) ? 1 : -1;
            } else if (interactionObject.TryGetComponent<InteractionArea>(out var interactionArea)) {
                _moveCoroutine[0] = StartCoroutine(WalkToPosition(transform.position.x < interactionArea.min ? () => interactionArea.min : () => interactionArea.max));
                yield return _moveCoroutine[0];

                if (interactionArea.setFacingDirection) {
                    float center = (interactionArea.min + interactionArea.max) * 0.5f;

                    SetFacingDirection(GetFaceDir(rb) == 1);
                    dinner.SetFacingDirection(GetFaceDir(dinner.rb));
                    if (spammyInParty) spammy.SetFacingDirection(GetFaceDir(spammy.rb));

                    int GetFaceDir(Rigidbody2D rb) => rb.position.x < center ? 1 : -1;
                } else if (interactionObject.TryGetComponent<InteractionFacingDirection>(out var facingDirection)) {
                    SetFaceDirByCenter(facingDirection.center, dinner, spammy, spammyInParty);
                }
            } else {
                _moveCoroutine[0] = StartCoroutine(WalkToPosition(interactionObject.transform.position.x));
                yield return _moveCoroutine[0];
            }

            interactionObject.Interact(m_Interactor);
        }

        public IEnumerator WalkToPosition(float position, bool run = false, int faceDir = 0, bool flipX = true) {
            if (CheckWalkPosition(rb, position, run)) yield break;
            stateMachine.moveState.WalkTo(position, run, flipX);
            yield return WaitExit(faceDir, flipX);
        }

        public IEnumerator WalkToPosition(Func<float> getPosition, bool run = false, int faceDir = 0, bool flipX = true) {
            if (CheckWalkPosition(rb, getPosition(), run)) yield break;
            stateMachine.moveState.WalkTo(getPosition, run, flipX: flipX);
            yield return WaitExit(faceDir, flipX);
        }

        private IEnumerator WaitExit(int faceDir, bool flipX) {
            yield return stateMachine.moveState.WaitExit();

            if (faceDir != 0 && flipX)
                SetFacingDirection(faceDir == 1);
        }

        public bool CheckWalkPosition(Rigidbody2D rb, float position, bool run) {
            float delta = Time.fixedDeltaTime * (run ? m_RunSpeed : m_MoveSpeed);
            Vector2 pos = rb.position;
            if (Mathf.Abs(position - pos.x) < delta) {
                pos.x = position;
                rb.position = pos;
                return true;
            }
            return false;
        }

        public bool CheckNextWalkPosition(Rigidbody2D rb, float velocityX, float endPosition, int direction) {
            float posX = rb.position.x;
            float deltaV = Time.fixedDeltaTime * velocityX;
            return (direction == -1 && posX + deltaV <= endPosition) || (direction == 1 && posX + deltaV >= endPosition);
        }

        public void PauseMovement() => stateMachine.animState.Animate(IdleAnimationHashes.GetAnimation(facingDirection == 1));

        public void ResumeMovement() => stateMachine.EnterDefaultState();

        public void FaceTo(Transform t) => SetFacingDirection(t.position.x >= transform.position.x);

        public void SetForceField(bool forceEnabled) {
            if (forceEnabled && !forceField) forceField = Instantiate(Resources.Load<BastheetForceField>("Battle/ForceField"), transform);
            if (forceField) forceField.gameObject.SetActive(forceEnabled);
        }

        public void PickObject(Transform target, Vector2 handOffset, Action picked = null) => StartCoroutine(COROUTINE_PickObject(target, handOffset, picked));

        public void StopAutoMove() {
            for (int i = 0; i < _moveCoroutine.Length; i++) {
                Coroutine coroutine = _moveCoroutine[i];
                if (coroutine != null) {
                    StopCoroutine(coroutine);
                    _moveCoroutine[i] = null;

                    if (i == 0) {
                        stateMachine.EnterDefaultState();
                    } else {
                        FollowerCharacterController follower = i == 1 ? GameCharactersManager.instance.dinner : i == 2 && GameManager.instance.spammyInParty ? GameCharactersManager.instance.spammy : null;
                        if (follower) follower.stateMachine.EnterDefaultState();
                    }
                }
            }
        }

        private IEnumerator COROUTINE_PickObject(Transform target, Vector2 handOffset, System.Action picked) {
            InputReader.instance.PushMap(InputReader.InputMap.None);
            float walkPosition = target.position.x - m_PickItemOffset.x - handOffset.x;
            yield return WalkToPosition(walkPosition, false, 1);
            transform.position = new Vector3(walkPosition, transform.position.y, transform.position.z);
            stateMachine.pickState.Pick(target);
            yield return Helpers.GetWaitForSeconds(m_PickObjectTime);
            InputReader.instance.PopMap(InputReader.InputMap.None);
            picked?.Invoke();
        }

        private void TRIGGER_WalkToObject(InteractionObject interactionObject) => StartCoroutine(WalkToObject(interactionObject));

        private void TRIGGER_SetupInteraction(InteractionObject interactionObject) {
            var dinner = GameCharactersManager.instance.dinner;
            var spammy = GameCharactersManager.instance.spammy;
            var spammyInParty = GameManager.instance.spammyInParty;

            if (interactionObject.TryGetComponent<InteractionPositions>(out var pos)) {
                if (!pos.ingoreBast) SetFacingDirection(GetFaceDir(pos.bastPos, rb.position.x, pos.bastDir) == 1);
                if (!pos.ignoreDinner) SetupCharacter(dinner, pos.dinnerPos, pos.dinnerDir);
                if (spammyInParty && !pos.ignoreSpammy) SetupCharacter(spammy, pos.spamPos, pos.spamDir);

                void SetupCharacter(FollowerCharacterController character, float target, bool dir) => character.SetFacingDirection(GetFaceDir(target, character.rb.position.x, dir));

                int GetFaceDir(float targetPos, float charPos, bool right) => (pos.setFaceDir ? targetPos < charPos : right) ? 1 : -1;
            } else if (interactionObject.TryGetComponent<InteractionArea>(out var interactionArea) && interactionArea.setFacingDirection) {
                SetFaceDirByCenter((interactionArea.min + interactionArea.max) * 0.5f, dinner, spammy, spammyInParty);
            } else if (interactionObject.TryGetComponent<InteractionFacingDirection>(out var interactionFacingDirection)) {
                SetFaceDirByCenter(interactionFacingDirection.center, dinner, spammy, spammyInParty);
            }
        }

        private void SetFaceDirByCenter(float center, DinnerCharacterController dinner, SpammyCharacterController spammy, bool spammyInParty) {
            SetFacingDirection(GetFaceDir(rb) == 1);
            dinner.SetFacingDirection(GetFaceDir(dinner.rb));
            if (spammyInParty) spammy.SetFacingDirection(GetFaceDir(spammy.rb));

            int GetFaceDir(Rigidbody2D rb) => rb.position.x < center ? 1 : -1;
        }

        private void INPUT_OnClick(Vector2 screenPosition) => _clickController.Raycast(m_Interactor.point, screenPosition);

        private void INPUT_MoveAxis(int direction) => moveDirection = direction;

        private void INPUT_Run(bool running) => isRunning = running;
    }
}
