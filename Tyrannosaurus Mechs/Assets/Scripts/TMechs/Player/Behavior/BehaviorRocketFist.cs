using System;
using Animancer;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.UI.GamePad;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorRocketFist : PlayerBehavior
    {
        private const int ATTACK_LAYER = 2;
        
        public float baseDamage = 10F;
        public float maxDamage = 100F;
        public float maxChargeTime = 5F;
        public float rechargeSpeed = 2F;
        public float rocketFistReturnTime = 1.5F;

        [Space]
        public GameObject rocketFistTemplate;
        public GameObject rocketFistGeo;
        public Transform rocketFistAnchor;
        
        private AnimancerState intro;
        private AnimancerState charge;
        private AnimancerState hold;
        private AnimancerState fire;
        private AnimancerState comeBack;

        private bool charging;
        private bool fired;
        private float returnTimer;
        [NonSerialized]
        public bool rocketReturned;
        
        public override void OnInit()
        {
            base.OnInit();

            intro = Animancer.CreateState(player.GetClip(Player.PlayerAnim.RocketChargeIntro), ATTACK_LAYER);
            charge = Animancer.CreateState(player.GetClip(Player.PlayerAnim.RocketCharge), ATTACK_LAYER);
            hold = Animancer.CreateState(player.GetClip(Player.PlayerAnim.RocketHold), ATTACK_LAYER);
            fire = Animancer.CreateState(player.GetClip(Player.PlayerAnim.RocketRecover), ATTACK_LAYER);
            comeBack = Animancer.CreateState(player.GetClip(Player.PlayerAnim.RocketReturn), ATTACK_LAYER);
        }
        
        public override void OnPush()
        {
            base.OnPush();

            charging = false;
            fired = false;
            rocketReturned = false;
            returnTimer = 0F;

            Animancer.CrossFadeFromStart(intro).OnEnd = () =>
            {
                intro.OnEnd = null;
                
                charging = true;
                Animancer.CrossFadeFromStart(charge, .1F).OnEnd = () =>
                {
                    Animancer.CrossFadeFromStart(hold, .1F);
                    hold.OnEnd = null;
                };
            };
        }
        
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (charging)
                player.standard.rocketFistCharge += Time.deltaTime;

            if (rocketReturned)
            {
                if (returnTimer <= rocketFistReturnTime)
                {
                    returnTimer += Time.deltaTime;
                    return;
                }
                
                Animancer.CrossFadeFromStart(comeBack, .1F).OnEnd = () =>
                {
                    Animancer.GetLayer(ATTACK_LAYER).StartFade(0F);
                    comeBack.OnEnd = null;
                };
                
                rocketFistGeo.SetActive(true);
                player.PopBehavior();
                return;
            }
            
            if (!fired)
            {
                GamepadLabels.AddLabel(IconMap.Icon.L2, "Rocket Fist");
                EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();
                
                transform.LookAt(enemy.transform.position.Set(transform.position.y, Utility.Axis.Y));
                player.movement.ResetIntendedY();
            }
            
            if (charging && !Input.GetButton(Controls.Action.LEFT_ARM))
            {
                intro.OnEnd = null;
                charge.OnEnd = null;

                charging = false;
                
                EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();
                if (!enemy)
                {
                    player.PopBehavior();
                    Animancer.GetLayer(ATTACK_LAYER).StartFade(0F);
                    return;
                }
                
                RocketFist rf = Object.Instantiate(rocketFistTemplate, rocketFistAnchor.position, rocketFistAnchor.rotation).GetComponent<RocketFist>();
                rf.damage = Mathf.Lerp(baseDamage, maxDamage, player.standard.rocketFistCharge / maxChargeTime);
                rf.target = enemy.transform;
                rocketFistGeo.SetActive(false);
                
                Animancer.CrossFadeFromStart(fire).OnEnd = () =>
                {
                    fire.OnEnd = null;
                    fired = true;
                    Animancer.GetLayer(ATTACK_LAYER).StartFade(0F);
                };
            }
        }

        public override float GetSpeed() => base.GetSpeed() * (fired ? .8F : .2F);
    }
}
