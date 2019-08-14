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
        private const int LAYER = Player.LAYER_GENERIC_1;

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

            jump = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Jump), LAYER);
            airJump = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.AirJump), LAYER);
        }

        public override void OnPush()
        {
            base.OnPush();

            AnimancerState state = Animancer.CrossFadeFromStart(player.forces.IsGrounded ? jump : airJump, .025F);
            state.OnEnd = () =>
            {
                state.OnEnd = null;
                state.StartFade(0F, .1F);
            };

            isAirJump = !player.forces.IsGrounded;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            GamepadLabels.EnableLabel(GamepadLabels.ButtonLabel.Jump, "Jump");
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
                    break;
                case "JumpDamage":
                    player.combat.DealAoe(aoeRange, aoeDamage.x, aoeDamage.y);
                    break;
                case "JumpPop":
                    player.PopBehavior();
                    break;
            }
        }

        public override bool OverridesLegs() => true;
        public override bool CanAnimateTakeDamage() => false;
    }
}