using TMechs.Environment.Targets;
using TMechs.Player;
using UnityEngine;

namespace TMechs.PlayerOld.BehaviourOld
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
            
            GameObject go = new GameObject($"ThrowableContainer:{target.name}");
            ThrowableContainer container = go.AddComponent<ThrowableContainer>();
            
            container.Initialize(target.gameObject);
            
            go.transform.SetParent(Player.Instance.pickupAnchor, false);
            go.transform.localPosition = Vector3.zero;
            Player.Instance.pickedUp = container;
        }
    }
}