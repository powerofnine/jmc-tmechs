using System;
using TMechs.Environment.Targets;
using TMechs.InspectorAttributes;
using TMechs.PlayerOld;
using TMechs.UI.GamePad;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player.Modules
{
    [Serializable]
    public class MovementModule : PlayerModule
    {
        [Name("AA Camera")]
        public Transform aaCamera;
        
        public float movementSpeed = 25F;
        public float runSpeed = 40F;

        [HideInInspector]
        public float intendedY;
        private float yDampVelocity;

        private bool sprinting;
        
        public override void OnRegistered()
        {
            base.OnRegistered();
            
            if (!aaCamera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                aaCamera = transform;
            }

            ResetIntendedY();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!player.CanMove)
                return;
            
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, aaCamera.eulerAngles.y, 0F) * movement;

            float movementMag = movement.sqrMagnitude;

            if (movementMag > float.Epsilon)
            {
                GamepadLabels.AddLabel(IconMap.IconGeneric.L3, sprinting ? "Stop Sprinting" : "Sprint", -100);
                
                if (Input.GetButtonDown(SPRINT))
                {
                    sprinting = !sprinting;
                }
                
                if (movementMag > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }
            else
            {
                sprinting = false;
            }

            float speed = player.Speed;
            if (!player.forces.canRun)
                speed = movementSpeed * .85F;
            
            player.forces.motion = movement * speed;

//            if(isGrounded)
//                GamepadLabels.AddLabel(IconMap.Icon.R1, "Dash");
            
//            if (canJump && jumps < maxJumps)
//            {
//                GamepadLabels.AddLabel(IconMap.Icon.ActionBottomRow1, "Jump");
//                
//                if (Input.GetButtonDown(JUMP))
//                {
//                    animator.SetTrigger(isGrounded ? Anim.JUMP : Anim.AIR_JUMP);
//                    canJump = false;
//                }
//            }

            EnemyTarget target = TargetController.Instance.GetLock();

            if (!player.CanMove)
                return;
            
            if (target)
            {
                transform.LookAt(target.transform.position.Set(transform.position.y, Utility.Axis.Y));
                ResetIntendedY();
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
            
//            if (isGrounded)
//            {
//                jumps = 0;
//                velocity = Vector3.zero;
//            }
        }

        public void ResetIntendedY()
            => intendedY = transform.eulerAngles.y;
    }
}