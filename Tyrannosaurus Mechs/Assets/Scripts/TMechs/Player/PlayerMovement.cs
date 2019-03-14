using System;
using TMechs.Environment.Targets;
using TMechs.InspectorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public static Rewired.Player Input { get; private set; }

        [Name("AA Camera")]
        public Transform aaCamera;

        [Header("Forces")]
        public float movementSpeed = 10F;
        public float jumpForce = 2 * 9.8F;

        public int maxJumps = 1;

        // State
        private float intendedY;
        private float yDampVelocity;

        private new Collider collider;
        private Rigidbody rb;
        private CharacterController controller;

        private int jumps;

        private Animator animator;
        private static readonly int ANIM_PLAYER_SPEED = Animator.StringToHash("Player Speed");

        private float yVelocity;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.SetFloat(ANIM_PLAYER_SPEED, movementSpeed);

            Input = Rewired.ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);

            if (!aaCamera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                aaCamera = transform;
            }

            intendedY = transform.eulerAngles.y;

            collider = GetComponentInChildren<Collider>();
            rb = GetComponent<Rigidbody>();
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Arms")).IsTag("NoMove"))
                movement = Vector3.zero;

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, aaCamera.eulerAngles.y, 0F) * movement;

            float movementMag = movement.sqrMagnitude;

            if (movementMag > float.Epsilon)
            {
                if (movementMag > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }

            yVelocity -= 9.8F * Time.deltaTime;

            if (controller.isGrounded)
            {
                jumps = 0;
                yVelocity = float.Epsilon;
            }

            if (Input.GetButtonDown(JUMP) && jumps < maxJumps)
            {
                if (!controller.isGrounded)
                    jumps++;
                yVelocity = jumpForce;
            }
            
            controller.Move((movement * movementSpeed + Vector3.up * yVelocity) * Time.deltaTime);
            
            EnemyTarget target = TargetController.Instance.GetLock();

            if (target)
            {
                transform.LookAt(target.transform.position.Set(transform.position.y, Utility.Axis.Y));
                intendedY = transform.eulerAngles.y;
                return;
            }

            if (Math.Abs(transform.eulerAngles.y - intendedY) > float.Epsilon)
            {
                float inRot = Mathf.SmoothDampAngle(transform.eulerAngles.y, intendedY, ref yDampVelocity, .1F);
                transform.eulerAngles = transform.eulerAngles.Set(inRot, Utility.Axis.Y);
            }

            if (transform.up != Vector3.up)
            {
                transform.up = Vector3.up;
                transform.eulerAngles = transform.eulerAngles.Set(intendedY, Utility.Axis.Y);
            }
        }

        private RaycastHit? GetGround()
        {
            if (Physics.Raycast(collider.bounds.center, -transform.up, out RaycastHit hit, collider.bounds.extents.y + .1F))
                return hit;
            return null;
        }
    }
}