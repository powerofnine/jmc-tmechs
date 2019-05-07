using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class RocketFistCharging : StateMachineBehaviour
    {
        public bool charge = true;
        
        private PlayerCombat combat;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            combat = Player.Instance.Combat;
            combat.rocketFistCharging = true;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if(charge)
                combat.rocketFistCharge += Time.deltaTime;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            combat.rocketFistCharging = false;
        }
    }
}