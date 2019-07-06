using System;
using Animancer;
using TMechs.Environment.Targets;
using TMechs.InspectorAttributes;
using TMechs.PlayerOld;
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

        [Space]
        public AvatarMask legs;
        public AvatarMask arms;
        
        [NonSerialized]
        public float intendedY;
        private float yDampVelocity;
        
        [NonSerialized]
        public float inputMagnitude;

        private LinearMixerState armsMixer;
        private LinearMixerState legsMixer;
        private AnimancerLayer legsLayer;

        public override void OnRegistered()
        {
            base.OnRegistered();
            
            if (!aaCamera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                aaCamera = transform;
            }

            ResetIntendedY();

            AnimancerLayer armsLayer = Animancer;
            armsLayer.SetName("Arms Layer");
            armsLayer.SetMask(arms);
            legsLayer = Animancer.GetLayer(3);
            legsLayer.SetName("Legs Layer");
            legsLayer.SetMask(legs);
            
            armsMixer = new LinearMixerState(armsLayer);
            legsMixer = new LinearMixerState(legsLayer);
            
            armsMixer.Initialise(
                    player.GetClip(Player.PlayerAnim.Idle), 
                    player.GetClip(Player.PlayerAnim.Walk), 
                    player.GetClip(Player.PlayerAnim.Run), 
                    0F, 
                    movementSpeed + 1F, 
                    runSpeed);
            legsMixer.Initialise(
                    player.GetClip(Player.PlayerAnim.Walk), 
                    player.GetClip(Player.PlayerAnim.Run), 
                    movementSpeed + 1F, 
                    runSpeed);
            Animancer.Play(armsMixer);
            Animancer.Play(legsMixer);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!player.CanMove)
            {
                armsMixer.Parameter = 0F;
                legsMixer.Parameter = 0F;
                legsLayer.SetWeight(0F);
                return;
            }

            armsMixer.Parameter = player.forces.ControllerVelocity.Remove(Utility.Axis.Y).magnitude;
            legsMixer.Parameter = armsMixer.Parameter;
            legsLayer.SetWeight(Mathf.Clamp01(Utility.MathRemap(legsMixer.Parameter, 0, movementSpeed + 1F, 0F, 1F)));
            
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, aaCamera.eulerAngles.y, 0F) * movement;

            inputMagnitude = movement.sqrMagnitude;

            if (inputMagnitude > float.Epsilon)
            {
                if (inputMagnitude > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }

            float speed = player.Speed;
            if (!player.forces.canRun)
                speed = movementSpeed * .85F;
            
            player.forces.motion = movement * speed;

            EnemyTarget target = TargetController.Instance.GetLock();

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
        }

        public void ResetIntendedY()
            => intendedY = transform.eulerAngles.y;
    }
}