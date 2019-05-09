using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Pickup : StateMachineBehaviour
    {
        private EnemyTarget target;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            target = TargetController.Instance.GetTarget<EnemyTarget>();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (!target)
                return;
            
            target.HandlePickup();

            target.transform.SetParent(Player.Instance.pickupAnchor, false);
            target.transform.localPosition = Vector3.zero;
            Player.Instance.pickedUp = target;
        }
    }
}