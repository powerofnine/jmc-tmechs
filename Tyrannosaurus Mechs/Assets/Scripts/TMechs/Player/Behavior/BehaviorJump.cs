using System;
using Animancer;
using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorJump : PlayerBehavior
    {
        private const int LAYER = 1;
        
        public int maxAirJumps = 1;
        public float jumpForce = 25F;

        private AnimancerState jump;
        private AnimancerState airJump;
        
        public override void OnInit()
        {
            base.OnInit();

            AnimancerLayer layer = Animancer.GetLayer(LAYER);
            layer.SetName("Jump Layer");

            jump = Animancer.CreateState(player.GetClip(Player.PlayerAnim.Jump), LAYER);
            airJump = Animancer.CreateState(player.GetClip(Player.PlayerAnim.AirJump), LAYER);
        }

        public override void OnPush()
        {
            base.OnPush();

            AnimancerState state = Animancer.CrossFadeFromStart(player.forces.IsGrounded ? jump : airJump);
            state.OnEnd = () => Animancer.GetLayer(LAYER).StartFade(0F);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            GamepadLabels.AddLabel(IconMap.Icon.ActionBottomRow1, "Jump");
        }

        public override void OnAnimationEvent(AnimationEvent e)
        {
            base.OnAnimationEvent(e);

            if (e.stringParameter == "Jump")
            {
                player.forces.velocity.y = jumpForce;
                player.PopBehavior();
            }
        }
    }
}