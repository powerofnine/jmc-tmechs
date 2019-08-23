using System;
using Animancer;
using TMechs.Environment.Targets;
using TMechs.InspectorAttributes;
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

        [Space]
        public AvatarMask legs;
        
        [NonSerialized]
        public float intendedY;
        private float yDampVelocity;

        [NonSerialized]
        public bool isSprinting;

        private LinearMixerState armsMixer;
        private LinearMixerState legsMixer;
        private AnimancerLayer legsLayer;

        private float paramVelocity;
        
        public override void OnRegistered()
        {
            base.OnRegistered();
            
            if (!aaCamera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                aaCamera = transform;
            }

            ResetIntendedY();

            AnimancerLayer armsLayer = Animancer.GetLayer(Player.LAYER_ARMS);
            legsLayer = Animancer.GetLayer(Player.LAYER_LEGS);
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
                isSprinting = false;
                return;
            }

            if (!player.Behavior.CanRun())
                isSprinting = false;

            armsMixer.Parameter = Mathf.SmoothDamp(armsMixer.Parameter, player.forces.ControllerVelocity.Remove(Utility.Axis.Y).magnitude, ref paramVelocity, .25F);
            legsMixer.Parameter = armsMixer.Parameter;
            if (player.Behavior.OverridesLegs())
                legsMixer.Parameter = 0F;
            legsLayer.SetWeight(Mathf.Clamp01(Utility.MathRemap(legsMixer.Parameter, 0, movementSpeed + 1F, 0F, 1F)));
            
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, aaCamera.eulerAngles.y, 0F) * movement;

            float inputMagnitude = movement.sqrMagnitude;

            if (inputMagnitude > float.Epsilon)
            {
                if (inputMagnitude > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;

                if (player.forces.canRun && player.Behavior.CanRun())
                {
                    if (Input.GetButtonDown(SPRINT))
                        isSprinting = !isSprinting;
                }
            }
            else
                isSprinting = false;

            float speed = player.Speed;
            if (!player.forces.canRun)
            {
                speed = movementSpeed * .85F;
            }

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

        public override void OnAnimationEvent(AnimationEvent e)
        {
            base.OnAnimationEvent(e);

            if (e.animatorClipInfo.weight <= .1F)
                return;
                
            if (!"StepLeft".Equals(e.stringParameter) && !"StepRight".Equals(e.stringParameter))
                return;

            AudioSource src = player.audio.GetPooledStep(player.IsInWater ? Player.PlayerAudio.StepType.Water : Player.PlayerAudio.StepType.Standard);
            if (src)
            {
                src.RandyPitchford();
                src.Play();
            }

            if (!player.IsInWater)
                return;

            Transform anchor = null;

            switch (e.stringParameter)
            {
                case "StepLeft":
                    anchor = player.vfx.leftFootAnchor;
                    break;
                case "StepRight":
                    anchor = player.vfx.rightFootAnchor;
                    break;
            }

            if (anchor != null)
            {
                VfxModule.SpawnEffect(player.vfx.waterSplash, anchor.position, anchor.rotation, 1F);
            }
        }

        public void ResetIntendedY()
            => intendedY = transform.eulerAngles.y;
    }
}