using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class RocketFistCharging : StateMachineBehaviour
    {
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            PlayerCombat combat = Player.Instance.Combat;
            combat.rocketFistCharge += Time.deltaTime;
            if (combat.rocketFistCharge >= combat.rocketFistChargeMax)
                animator.SetTrigger(Anim.ROCKET_OVERCHARGE);
        }
    }
}