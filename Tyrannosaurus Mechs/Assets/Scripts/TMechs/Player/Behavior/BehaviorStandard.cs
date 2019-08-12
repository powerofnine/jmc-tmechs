using System;
using Animancer;
using TMechs.Environment.Targets;
using TMechs.UI.GamePad;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorStandard : PlayerBehavior
    {
        private int jumps;

        private AnimancerState extendedIdle;
        private float idleTimer;
        
        public override void OnInit()
        {
            base.OnInit();

            extendedIdle = Animancer.CreateState(player.GetClip(Player.PlayerAnim.IdleBreather), 2);
        }

        public override void OnShadowed()
        {
            base.OnShadowed();

            idleTimer = 0F;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            idleTimer += Time.deltaTime;

            if (player.forces.ControllerVelocity.sqrMagnitude > Mathf.Epsilon)
                idleTimer = 0F;
            
            if (idleTimer > 4F && !extendedIdle.IsPlaying)
            {
                Animancer.CrossFadeFromStart(extendedIdle).OnEnd = () =>
                {
                    idleTimer = 0F;
                    Animancer.GetLayer(2).StartFade(0F);
                };
            }
            
            if (player.rocketFist.rocketFistCharge > 0F)
                player.rocketFist.rocketFistCharge -= player.rocketFist.rechargeSpeed * Time.deltaTime;
            if (player.forces.IsGrounded)
                jumps = 0;

            if (player.forces.IsGrounded)
            {
                if (Input.GetButtonDown(DASH))
                {
                    player.PushBehavior(player.dash);
                    return;
                }
            }
            
            if (jumps < player.jump.maxAirJumps)
            {
                GamepadLabels.EnableLabel(GamepadLabels.ButtonLabel.Jump, "Jump");
                
                if (Input.GetButtonDown(JUMP))
                {
                    player.PushBehavior(player.jump);
                    if (!player.forces.IsGrounded)
                        jumps++;
                    return;
                }
            }

            GamepadLabels.EnableLabel(GamepadLabels.ButtonLabel.Attack, "Attack");
            if (Input.GetButtonDown(ATTACK))
            {
                player.PushBehavior(player.attack);
                return;
            }

            GrappleTarget grapple = TargetController.Instance.GetTarget<GrappleTarget>();

            if (grapple != null)
            {
                if (Input.GetButtonDown(RIGHT_ARM))
                {
                    player.PushBehavior(player.grapple);
                    return;
                }
            }
            
            EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();

            if (enemy != null)
            {
                if (player.rocketFist.rocketFistCharge <= Mathf.Epsilon)
                {
                    if (Input.GetButtonDown(LEFT_ARM))
                    {
                        player.PushBehavior(player.rocketFist);
                        return;
                    }
                }

                if (enemy.pickup != EnemyTarget.PickupType.Prohibit && Vector3.Distance(transform.position, enemy.transform.position) <= player.carry.grabRange)
                {
                    if (Input.GetButtonDown(RIGHT_ARM))
                    {
                        player.PushBehavior(player.carry);
                        return;
                    }
                }
            }
            
            player.interaction.ProcessInteractions();
        }
    }
}