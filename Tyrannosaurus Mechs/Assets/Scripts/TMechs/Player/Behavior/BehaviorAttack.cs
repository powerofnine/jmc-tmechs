using System;
using Animancer;
using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorAttack : PlayerBehavior
    {
        private const int LAYER = 2;

        public float forwardSpeed = 5F;
        private bool applyForward = false;
        
        private int attackCount;
        private int attackPresses;

        private AnimancerState attackString1;
        private AnimancerState attackString2;
        private AnimancerState attackString3;
        
        public override void OnInit()
        {
            base.OnInit();

            AnimancerLayer layer = Animancer.GetLayer(LAYER);
            layer.SetName("Attack Layer");

            attackString1 = Animancer.CreateState(player.GetClip(Player.PlayerAnim.Attack1), LAYER);
            attackString2 = Animancer.CreateState(player.GetClip(Player.PlayerAnim.Attack2), LAYER);
            attackString3 = Animancer.CreateState(player.GetClip(Player.PlayerAnim.Attack3), LAYER);
        }
        
        public override void OnPush()
        {
            base.OnPush();

            attackCount = 0;
            attackPresses = 0;
            NextAttack();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            GamepadLabels.AddLabel(IconMap.Icon.ActionTopRow1, "Attack");
            if (Input.GetButtonDown(Controls.Action.ATTACK))
                attackPresses++;

            if (applyForward)
                player.forces.motion = transform.forward * forwardSpeed;
        }

        public void NextAttack()
        {
            AnimancerState next = null;

            switch (attackCount)
            {
                case 0:
                    next = attackString1;
                    break;
                case 1:
                    next = attackString2;
                    break;
                case 2:
                    next = attackString3;
                    break;
            }

            if (next == null)
                return;

            applyForward = false;
            attackCount++;
            Animancer.CrossFadeFromStart(next, 0.1F).OnEnd = OnAnimEnd;
        }

        public override void OnAnimationEvent(AnimationEvent e)
        {
            base.OnAnimationEvent(e);

            switch (e.stringParameter)
            {
                case "AttackCancel" when attackPresses <= 0:
                    break;
                case "AttackCancel":
                    attackPresses--;
                    NextAttack();
                    break;
                case "AttackEnd":
                    OnAnimEnd();
                    break;
                case "AttackForward":
                    applyForward = true;
                    break;
            }
        }

        public override bool CanMove() => false;

        private void OnAnimEnd()
        {
            Animancer.GetLayer(LAYER).StartFade(0F);
            player.PopBehavior();
        }
    }
}