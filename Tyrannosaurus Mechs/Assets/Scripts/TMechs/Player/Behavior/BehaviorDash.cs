using System;
using Animancer;
using TMechs.Environment.Targets;
using TMechs.Player.Modules;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorDash : PlayerBehavior
    {
        public float dashSpeed = 40F;
        public float chargeHitDamage = 10F;
        public float chargeHitStaggerTime = 1F;
        private Vector3 forward;

        private bool isStaggered;
        private float staggerTimer;

        private AnimancerState dash;
        
        public override void OnInit()
        {
            base.OnInit();

            dash = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Dash), Player.LAYER_GENERIC_1);
        }

        public override void OnPush()
        {
            base.OnPush();

            isStaggered = false;
            staggerTimer = 0F;
            
            player.combat.SetHitbox("charge", chargeHitDamage, onHit: SetStagger);

            Animancer.CrossFadeFromStart(dash, .1F).OnEnd = () =>
            {
                dash.OnEnd = null;
                player.PopBehavior();
                Animancer.GetLayer(Player.LAYER_GENERIC_1).StartFade(0F);
            };
            
            forward = transform.forward;
            
            Vector3 movement = Player.Input.GetAxis2DRaw(Controls.Action.MOVE_HORIZONTAL, Controls.Action.MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, Player.Instance.movement.aaCamera.eulerAngles.y, 0F) * movement;

            float movementMag = movement.sqrMagnitude;

            if (movementMag > float.Epsilon)
                forward = Quaternion.Euler(0F, Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg, 0F) * Vector3.forward;

            if (player.vfx.dash)
            {
                player.vfx.dash.gameObject.SetActive(true);
                player.vfx.dash.Play();
            }
            if(player.audio.dash)
                player.audio.dash.Play();
        }
        
        public override void OnPop()
        {
            base.OnPop();
            
            player.combat.SetHitbox(null, 0F);
            
            if(player.vfx.dash)
                player.vfx.dash.Stop();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (isStaggered)
            {
                staggerTimer += Time.deltaTime;
                if (staggerTimer > chargeHitStaggerTime)
                    player.PopBehavior();
                return;
            }
            
            player.forces.motion = forward * dashSpeed;
            
            EnemyTarget target = TargetController.Instance.GetLock();

            if (target)
            {
                transform.LookAt(target.transform.position.Set(transform.position.y, Utility.Axis.Y));
                player.movement.ResetIntendedY();
            }
        }
        
        private void SetStagger()
        {
            dash.OnEnd = null;
            player.combat.SetHitbox(null, 0F);
            Animancer.GetLayer(Player.LAYER_GENERIC_1).StartFade(0F); // TODO stagger animation?
            player.forces.motion = dashSpeed * 4F * -forward;
            isStaggered = true;
        }
        
        public override bool CanMove() => false;
    }
}