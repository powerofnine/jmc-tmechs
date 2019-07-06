using Animancer;
using TMechs.UI.GamePad;
using UnityEngine;
using UnityEngine.Rendering;

namespace TMechs.Player.Behavior
{
    public class BehaviorAttack : PlayerBehavior
    {
        private const int LAYER = 2;
        
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
            }
        }

        private void OnAnimEnd()
        {
            Animancer.GetLayer(LAYER).StartFade(0F);
            player.PopBehavior();
        }
    }
}