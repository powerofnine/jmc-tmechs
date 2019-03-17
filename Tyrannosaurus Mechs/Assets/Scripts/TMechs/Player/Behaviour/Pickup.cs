using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Pickup : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            EnemyTarget target = TargetController.Instance.GetTarget<EnemyTarget>();
            target.HandlePickup();

            target.transform.SetParent(Player.Instance.pickupAnchor, true);
            Player.Instance.pickedUp = target;
        }
    }
}
