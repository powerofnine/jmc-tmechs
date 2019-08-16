using System;
using Animancer;
using Rewired;
using UnityEngine;

namespace TMechs.Player.Modules
{
    [Serializable]
    public class ForcesModule : PlayerModule
    {
        private CharacterController controller;

        public float gravityMultiplier = 1F;
        public float stickyGroundForce = 200F;
        
        [NonSerialized]
        public Vector3 velocity;
        [NonSerialized]
        public Vector3 motion;
        [NonSerialized]
        public Vector3 frictionedVelocity;

        public Vector3 ControllerVelocity { get; private set; }

        public bool IsGrounded => groundedFrames > 0;

        private int groundedFrames;
        [NonSerialized]
        public bool canRun = true;

        private AnimancerState fall;
        private AnimancerState land;
        
        public override void OnRegistered()
        {
            base.OnRegistered();

            controller = GetComponent<CharacterController>();

            fall = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Fall), Player.LAYER_FALL);
            land = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Land), Player.LAYER_FALL);
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            velocity.y -= Utility.GRAVITY * gravityMultiplier * Time.deltaTime;
            
            controller.Move((velocity + motion + frictionedVelocity) * Time.deltaTime);
            ControllerVelocity = controller.velocity;

            frictionedVelocity = Vector3.Lerp(frictionedVelocity, Vector3.zero, Time.deltaTime * 5F);

            if (groundedFrames > 1 && velocity.y < Mathf.Epsilon)
                controller.Move(stickyGroundForce * Time.deltaTime * Vector3.down);

            if (controller.isGrounded)
                velocity = Vector3.zero;
            motion = Vector3.zero;

            if (groundedFrames > 0)
                groundedFrames--;
            
            if (!IsGrounded && ControllerVelocity.y < -5F && !fall.IsPlaying && player.Behavior != player.jump)
                    Animancer.CrossFadeFromStart(fall, .25F);

            GroundedCheck();
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }

        public void ResetGround() => groundedFrames = 0;
        
        private void GroundedCheck()
        {
            canRun = true;

            if (!controller.isGrounded)
                return;

            bool sliding = false;

            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 1F, LayerMask.GetMask("Player", "Ignore Raycast")))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit - 1F)
                    sliding = true;
            }
            else
            {
                Physics.Raycast(player.contactPoint + Vector3.up, Vector3.down, out hit, LayerMask.GetMask("Player", "Ignore Raycast"));
                if (Vector3.Angle(hit.normal, Vector3.up) > controller.slopeLimit - 1F)
                    sliding = true;
            }

            canRun = !sliding;

            if (!sliding)
            {
                if (groundedFrames == 0)
                {
                    Animancer.CrossFadeFromStart(land, 0F).OnEnd = () => land.StartFade(0F, .1F);
                }

                groundedFrames = 2;
                return;
            }

            Vector3 normal = hit.normal;
            Vector3 direction = new Vector3(normal.x, 0F, normal.z);
            Vector3.OrthoNormalize(ref normal, ref direction);

            player.movement.intendedY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            controller.Move(player.movement.runSpeed * 1.15F * Time.deltaTime * direction);
            ControllerVelocity = controller.velocity;
        }
    }
}