//Base made by: https://github.com/IronWarrior
using System;
using Structure2D;
using UnityEngine;

namespace Structure2D.Example
{

    [RequireComponent(typeof(BoxCollider2D)), AddComponentMenu("Structure 2D/Example/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody2D _rigidbody2D;

        [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
        float speed = 9;

        [SerializeField, Tooltip("Acceleration while grounded.")]
        float walkAcceleration = 75;

        [SerializeField, Tooltip("Acceleration while in the air.")]
        float airAcceleration = 30;

        [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
        float groundDeceleration = 70;

        [SerializeField]
        float jumpHeight = 4;

        [SerializeField]
        private float _jumpGravityScale;

        private Vector2 velocity;

        /// <summary>
        /// Set to true when the character intersects a collider beneath
        /// them in the previous frame.
        /// </summary>
        private bool grounded;

        private SpriteRenderer _renderer;

        private Animator _anim;

        private void Awake()
        {
            GameObject.FindObjectOfType<CameraTargetController>().Player = this;

            _renderer = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();

            CellMap.MapUnloaded += DestroyPlayer;
        }

        private void DestroyPlayer()
        {
            GameObject.Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            CellMap.MapUnloaded -= DestroyPlayer;
        }

        private void Update()
        {
            //If there is no collider below is the terrain colliders aren't generated yet.
            //So we don't want the player to be affected by the simulation yet, otherwise he could fall inside a chunk.
            _rigidbody2D.simulated = Physics2D.Raycast(transform.position, Vector2.down, int.MaxValue);

            // Use GetAxisRaw to ensure our input is either 0, 1 or -1.
            float moveInput = Input.GetAxisRaw("Horizontal");

            if (grounded)
            {
                velocity.y = 0;

                if (Input.GetButton("Jump"))
                {
                    // Calculate the velocity required to achieve the target jump height.
                    velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
                    grounded = false;
                }
            }

            float acceleration = grounded ? walkAcceleration : airAcceleration;
            float deceleration = grounded ? groundDeceleration : 0;

            if (moveInput > 0)
                _renderer.flipX = false;
            else if (moveInput < 0)
                _renderer.flipX = true;

            velocity.y -= _jumpGravityScale * -Physics2D.gravity.y * Time.deltaTime;

            if (grounded)
            {
                velocity.y = 0;
            }

            UpdateAnimator();

            if (Math.Abs(moveInput) > 0.01f)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            }
        }

        private void UpdateAnimator()
        {
            if (_anim == null)
                return;

            _anim.SetBool("IsMoving", velocity.x != 0);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.1f)
                {
                    grounded = true;
                    break;
                }
            }
        }

        private void FixedUpdate()
        {
            _rigidbody2D.MovePosition(_rigidbody2D.position + velocity * Time.fixedDeltaTime);
        }
    }
}
