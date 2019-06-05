using System;
using TMechs.Data;
using TMechs.Environment.Targets;
using TMechs.InspectorAttributes;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerMovement : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        private static Rewired.Player Input => Player.Input;

        [Name("AA Camera")]
        public Transform aaCamera;

        [Header("Forces")]
        public float movementSpeed = 10F;
        public float runSpeed = 20F;
        public float jumpForce = 2 * Utility.GRAVITY;

        public int maxJumps = 1;

        [NonSerialized]
        public bool disableControllerMovement;
        
        // State
        private float intendedY;
        private float yDampVelocity;

        private CharacterController controller;

        private int jumps;

        private Animator animator;

        // Movement
        public Vector3 velocity;
        public Vector3 motion;
        private bool isGrounded;
        private Vector3 contactPoint;
        private bool canRun = true;
        private bool canJump = true;

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
                return;
            if (animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Walk")).IsName("Move"))
                canJump = true;

            bool angry = Input.GetButton(ANGERY);
            
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, aaCamera.eulerAngles.y, 0F) * movement;

            float movementMag = movement.sqrMagnitude;

            if (movementMag > float.Epsilon)
            {
                if (movementMag > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }

            float speed = angry ? runSpeed : movementSpeed;
            if (!canRun)
                speed = movementSpeed * .85F;
            
            motion = movement * speed;
            
            if (canJump && Input.GetButtonDown(JUMP) && jumps < maxJumps)
            {
                animator.SetTrigger(isGrounded ? Anim.JUMP : Anim.AIR_JUMP);
                canJump = false;
            }
        }

        private void LateUpdate()
        {
            if (!disableControllerMovement)
            {
                velocity.y -= Utility.GRAVITY * Time.deltaTime;

                controller.Move((motion + velocity) * Time.deltaTime);
            }

            motion = Vector3.zero;
            
            animator.SetFloat(Anim.MOVE_DELTA, controller.velocity.Remove(Utility.Axis.Y).magnitude / movementSpeed / 2F); 
            
            GroundedCheck();
            
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
            
            animator.SetBool(Anim.GROUNDED, isGrounded);

            if (isGrounded)
            {
                jumps = 0;
                velocity = Vector3.zero;
            }
        }

        private void GroundedCheck()
        {
            isGrounded = controller.isGrounded;
            canRun = true;

            if (!isGrounded)
                return;

            bool sliding = false;

            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 1F))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit - 1F)
                    sliding = true;
            }
            else
            {
                Physics.Raycast(contactPoint + Vector3.up, Vector3.down, out hit);
                if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit - 1F)
                    sliding = true;
            }

            canRun = !sliding;

            if (!sliding)
                return;

            isGrounded = false;

            Vector3 normal = hit.normal;
            Vector3 direction = new Vector3(normal.x, 0F, normal.z);
            Vector3.OrthoNormalize(ref normal, ref direction);

            intendedY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            controller.Move(runSpeed * 1.15F * Time.deltaTime * direction);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
            => contactPoint = hit.point;

        public void OnAnimationEvent(string id)
        {
            if ("jump".Equals(id))
            {
                if (!isGrounded)
                    jumps++;
                velocity.y = jumpForce;
                canJump = true;
            }
        }
    }
}