using System;
using TMechs.UI.GamePad;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorStandard : PlayerBehavior
    {
        private int jumps;
        
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (player.movement.inputMagnitude > Mathf.Epsilon)
            {
                GamepadLabels.AddLabel(IconMap.IconGeneric.L3, "Sprint", -100);
                
                if(Input.GetButtonDown(SPRINT))
                    player.PushBehavior(player.sprint);
            }
            
            if (jumps < player.jump.maxAirJumps)
            {
                GamepadLabels.AddLabel(IconMap.Icon.ActionBottomRow1, "Jump");
                
                if (Input.GetButtonDown(JUMP))
                {
                    player.PushBehavior(player.jump);
                    if (!player.forces.IsGrounded)
                        jumps++;
                }
            }

            if (player.forces.IsGrounded)
                jumps = 0;
        }
    }
}