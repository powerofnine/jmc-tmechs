using System;
using TMechs.Environment.Targets;
using TMechs.PlayerOld;
using TMechs.UI.GamePad;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorStandard : PlayerBehavior
    {
        private int jumps;

        [NonSerialized]
        public float rocketFistCharge;
        
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (rocketFistCharge > 0F)
                rocketFistCharge -= player.rocketFist.rechargeSpeed * Time.deltaTime;
            if (player.forces.IsGrounded)
                jumps = 0;
            
            if (player.movement.inputMagnitude > Mathf.Epsilon)
            {
                GamepadLabels.AddLabel(IconMap.IconGeneric.L3, "Sprint", -100);

                if (Input.GetButtonDown(SPRINT))
                {
                    player.PushBehavior(player.sprint);
                    return;
                }
            }

            if (player.forces.IsGrounded)
            {
                GamepadLabels.AddLabel(IconMap.Icon.R1, "Dash");

                if (Input.GetButtonDown(DASH))
                {
                    player.PushBehavior(player.dash);
                    return;
                }
            }
            
            if (jumps < player.jump.maxAirJumps)
            {
                GamepadLabels.AddLabel(IconMap.Icon.ActionBottomRow1, "Jump");
                
                if (Input.GetButtonDown(JUMP))
                {
                    player.PushBehavior(player.jump);
                    if (!player.forces.IsGrounded)
                        jumps++;
                    return;
                }
            }

            GamepadLabels.AddLabel(IconMap.Icon.ActionTopRow1, "Attack");
            if (Input.GetButtonDown(ATTACK))
            {
                player.PushBehavior(player.attack);
                return;
            }

            EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();

            if (enemy != null)
            {
                GamepadLabels.AddLabel(IconMap.Icon.L2, "Rocket Fist");
                if (Input.GetButton(LEFT_ARM))
                {
                    player.PushBehavior(player.rocketFist);
                    return;
                }
            }
        }
    }
}