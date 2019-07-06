using System;
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

            if (Input.GetButtonDown(JUMP) && jumps < player.jump.maxAirJumps)
            {
                player.PushBehavior(player.jump);
                if (!player.forces.IsGrounded)
                    jumps++;
            }

            if (player.forces.IsGrounded)
                jumps = 0;
        }
    }
}