using System;
using Animancer;
using TMechs.Attributes;
using TMechs.Player.Modules;
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

        [Space]
        public float aoeRange = 10F;
        [MinMax]
        public Vector2 aoeDamage = new Vector2(5F, 10F);
        
        private AnimancerState jump;
        private AnimancerState airJump;

        private bool isAirJump;
        
        public override void OnInit()
        {
            base.OnInit();

            AnimancerLayer layer = Animancer.GetLayer(LAYER);
            layer.SetName("Jump Layer");

            jump = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Jump), LAYER);
            jump.Speed = 2F;
            
            airJump = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.AirJump), LAYER);
        }

        public override void OnPush()
        {
            base.OnPush();

            Animancer.CrossFadeFromStart(player.forces.IsGrounded ? jump : airJump).OnEnd = OnAnimEnd;
            isAirJump = !player.forces.IsGrounded;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            GamepadLabels.AddLabel(IconMap.Icon.ActionBottomRow1, "Jump");
        }

        public override void OnAnimationEvent(AnimationEvent e)
        {
            base.OnAnimationEvent(e);

            switch (e.stringParameter)
            {
                case "Jump":
                    if (!isAirJump)
                    {
                        VfxModule.SpawnEffect(player.vfx.jump, player.centerOfMass.position, transform.rotation, .6F);
                        player.vfx.SpawnGroundSlam();
                    }

                    player.forces.velocity.y = jumpForce;
                    player.PopBehavior();
                    break;
                case "JumpDamage":
                    player.combat.DealAoe(aoeRange, aoeDamage.x, aoeDamage.y);
                    break;
            }
        }

        private void OnAnimEnd()
        {
            Animancer.GetLayer(LAYER).StartFade(0F);
        }
    }
}