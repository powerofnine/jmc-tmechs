using System;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorJump : PlayerBehavior
    {
        public int maxAirJumps = 1;
        public float jumpForce = 25F;
        
        public override void OnPush()
        {
            base.OnPush();

            //TODO animation
            
            player.forces.velocity.y = jumpForce;
            player.PopBehavior();
        }
    }
}