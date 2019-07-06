using System;
using UnityEngine;

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
        
        public override void OnInit()
        {
            base.OnInit();
        }
        
//        public override void OnPush()
//        {
//            base.OnPush();
//
//            player.Combat.rocketFistCharging = true;
//        }
//
//        public override void OnPop()
//        {
//            base.OnPop();
//
//            player.Combat.rocketFistCharging = false;
//        }
//
//        public override void OnUpdate()
//        {
//            base.OnUpdate();
//
//            player.Combat.rocketFistCharge += Time.deltaTime;
//        }

        public override bool CanMove() => false;
    }
}
