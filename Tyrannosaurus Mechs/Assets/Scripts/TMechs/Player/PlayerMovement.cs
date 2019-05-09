using System;
using TMechs.Data;
using TMechs.Environment.Targets;
using TMechs.InspectorAttributes;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private static Rewired.Player Input => Player.Input;

        [Name("AA Camera")]
        public Transform aaCamera;

        [Header("Forces")]
        public float movementSpeed = 10F;
        public float runSpeed = 20F;
        public float jumpForce = 2 * Utility.GRAVITY;

        public int maxJumps = 1;

        // State
        private float intendedY;
        private float yDampVelocity;

        private CharacterController controller;

        private int jumps;

        private Animator animator;

        // Movement
        public Vector3 velocity;
        private bool isGrounded;
        private Vector3 contactPoint;
        private bool playerControl = true;

        private void Awake()
        {
            animator = Player.Instance.Animator;
            animator.SetFloat(Anim.PLAYER_SPEED, movementSpeed);

            if (!aaCamera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                aaCamera = transform;
            }

            intendedY = transform.eulerAngles.y;

            controller = Player.Instance.Controller;
        }

        private void Update()
        {
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Arms")).IsTag("NoMove"))
            {
                animator.SetFloat(Anim.MOVE_DELTA, 0F);
                return;
            }

            bool angry = Input.GetButton(ANGERY);
            
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();
            if (!playerControl)
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

            velocity.y -= Utility.GRAVITY * Time.deltaTime;

            float speed = angry ? runSpeed : movementSpeed;

            controller.Move((movement * speed + velocity) * Time.deltaTime);
            animator.SetFloat(Anim.MOVE_DELTA, controller.velocity.Remove(Utility.Axis.Y).magnitude / movementSpeed / 2F); 
            GroundedCheck();
            animator.SetBool(Anim.GROUNDED, isGrounded);

            if (isGrounded)
            {
                jumps = 0;
                velocity.y = 0F;
            }

            if (Input.GetButtonDown(JUMP) && jumps < maxJumps)
            {
                if (!isGrounded)
                    jumps++;
                velocity.y = jumpForce;
            }

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

        private void GroundedCheck()
        {
            isGrounded = controller.isGrounded;
            playerControl = true;

            if (!isGrounded)
                return;

            bool sliding = false;

            if (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 1F))
                if (!Physics.Raycast(contactPoint + Vector3.up, Vector3.down, out hit))
                    return;

            if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit)
                sliding = true;

            playerControl = !sliding;

            if (!sliding)
                return;

            isGrounded = false;

            Vector3 normal = hit.normal;
            Vector3 direction = new Vector3(normal.x, 0F, normal.z);
            Vector3.OrthoNormalize(ref normal, ref direction);

            intendedY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            controller.Move(movementSpeed * Time.deltaTime * direction);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
            => contactPoint = hit.point;
    }
}