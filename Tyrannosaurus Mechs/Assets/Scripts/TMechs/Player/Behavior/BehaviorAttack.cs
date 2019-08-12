using System;
using Animancer;
using TMechs.Attributes;
using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorAttack : PlayerBehavior
    {
        private const int LAYER = Player.LAYER_GENERIC_2;

        public float forwardSpeed = 5F;

        [Space]
        public float attack1Damage = 10F;
        public float attack2Damage = 15F;
        [Space]
        [MinMax]
        public Vector2 attack3Damage = new Vector2(10F, 20F);
        public float attack3Range = 20F;
        
        private bool applyForward = false;
        
        private int attackCount;
        private int attackPresses;

        private AnimancerState attackString1;
        private AnimancerState attackString2;
        private AnimancerState attackString3;

        private string nextHitbox;
        private float nextDamage;

        private bool queueDash;

        [NonSerialized]
        public bool singleAttack;
        
        public override void OnInit()
        {
            base.OnInit();

            attackString1 = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Attack1), LAYER);
            attackString2 = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Attack2), LAYER);
            attackString3 = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Attack3), LAYER);
            attackString3.Speed = .8F;
        }
        
        public override void OnPush()
        {
            base.OnPush();

            attackCount = 0;
            attackPresses = 0;
            queueDash = false;
            NextAttack();
        }

        public override void OnPop()
        {
            base.OnPop();
            
            player.combat.SetHitbox(null, 0F);
            if(player.vfx.leftPunchTrail)
                player.vfx.leftPunchTrail.Stop();
            if(player.vfx.rightPunchTrail)
                player.vfx.rightPunchTrail.Stop();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (queueDash)
                return;
            
            GamepadLabels.EnableLabel(GamepadLabels.ButtonLabel.Attack, "Attack");
            if (Input.GetButtonDown(Controls.Action.ATTACK))
                attackPresses++;
            else if (Input.GetButtonDown(Controls.Action.DASH))
                queueDash = true;
            if (applyForward)
                player.forces.motion = transform.forward * forwardSpeed;
        }

        public void NextAttack()
        {
            AnimancerState next = null;

            if(player.vfx.leftPunchTrail)
                player.vfx.leftPunchTrail.Stop();
            if(player.vfx.rightPunchTrail)
                player.vfx.rightPunchTrail.Stop();
            
            switch (attackCount)
            {
                case 0:
                    next = attackString1;
                    nextHitbox = "left";
                    nextDamage = attack1Damage;

                    if (player.vfx.leftPunchTrail)
                    {
                        player.vfx.leftPunchTrail.gameObject.SetActive(true);
                        player.vfx.leftPunchTrail.Play();
                    }

                    break;
                case 1:
                    next = attackString2;
                    nextHitbox = "right";
                    nextDamage = attack2Damage;
                    
                    if (player.vfx.rightPunchTrail)
                    {
                        player.vfx.rightPunchTrail.gameObject.SetActive(true);
                        player.vfx.rightPunchTrail.Play();
                    }
                    
                    break;
                case 2:
                    next = attackString3;
                    nextHitbox = "aoe";
                    
                    if (player.vfx.leftPunchTrail)
                    {
                        player.vfx.leftPunchTrail.gameObject.SetActive(true);
                        player.vfx.leftPunchTrail.Play();
                    }
                    if (player.vfx.rightPunchTrail)
                    {
                        player.vfx.rightPunchTrail.gameObject.SetActive(true);
                        player.vfx.rightPunchTrail.Play();
                    }
                    break;
            }

            if (next == null)
                return;

            applyForward = false;
            attackCount++;
            if (singleAttack)
                attackCount = 2000;
            singleAttack = false;
            player.combat.SetHitbox("", 0F);
            Animancer.CrossFadeFromStart(next, 0.1F).OnEnd = OnAnimEnd;
        }

        public override void OnAnimationEvent(AnimationEvent e)
        {
            base.OnAnimationEvent(e);

            switch (e.stringParameter)
            {
                case "AttackCancel" when queueDash && player.forces.IsGrounded:
                    attackString1.Stop();
                    attackString2.Stop();
                    attackString3.Stop();
                    
                    player.PopBehavior();
                    player.PushBehavior(player.dash);
                    
                    break;
                case "AttackCancel" when attackPresses <= 0:
                    queueDash = false;
                    break;
                case "AttackCancel":
                    queueDash = false;
                    attackPresses--;
                    NextAttack();
                    break;
                case "AttackEnd":
                    OnAnimEnd();
                    break;
                case "AttackForward":
                    applyForward = true;
                    break;
                case "AttackHit":
                    ActivateHitbox();        
                    break;
            }
        }

        private void ActivateHitbox()
        {
            if (nextHitbox == "aoe")
            {
                player.combat.DealAoe(attack3Range, attack3Damage.x, attack3Damage.y);
//                player.vfx.SpawnGroundSlam();
            }
            else
                player.combat.SetHitbox(nextHitbox, nextDamage);
        }

        public override float GetSpeed() => base.GetSpeed() * .2F;
        public override bool CanRun() => false;

        private void OnAnimEnd()
        {
            Animancer.GetLayer(LAYER).StartFade(0F);
            player.PopBehavior();
        }
    }
}