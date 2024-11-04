using UnityEngine;

namespace NFHGame.Characters {
    public class GameCharacterController : MonoBehaviour {
        public Rigidbody2D rb { get; private set; }
        public Animator anim { get; private set; }
        public int facingDirection { get; protected set; }

        protected virtual void Awake() {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            SetPosition(transform.position, true);
        }

        protected virtual void Start() {
        }

        public virtual void SetFacingDirection(bool facingRight) {
            facingDirection = facingRight ? 1 : -1;
        }

        public virtual void SetPosition(Vector3 position, bool facingRight) {
            if (gameObject.activeInHierarchy) rb.position = position;
            else transform.position = position;
            SetFacingDirection(facingRight);
        }

        public virtual void SetPositionX(float x, bool facingRight) {
            var selfPosition = gameObject.activeInHierarchy ? (Vector3)rb.position : transform.position;
            selfPosition.x = x;
            SetPosition(selfPosition, facingRight);
        }

        public virtual void SetPositionX(float x) {
            SetPositionX(x, facingDirection == 1);
        }

        public virtual void ToggleLookBack(bool lookBack) {
        }
    }
}