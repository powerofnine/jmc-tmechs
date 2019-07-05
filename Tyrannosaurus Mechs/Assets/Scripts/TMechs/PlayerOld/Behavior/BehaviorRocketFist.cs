using UnityEngine;

namespace TMechs.PlayerOld.Behavior
{
    public class BehaviorRocketFist : PlayerBehavior
    {
        public override void OnPush()
        {
            base.OnPush();

            player.Combat.rocketFistCharging = true;
        }

        public override void OnPop()
        {
            base.OnPop();

            player.Combat.rocketFistCharging = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            player.Combat.rocketFistCharge += Time.deltaTime;
        }

        public override bool CanMove() => false;
    }
}
