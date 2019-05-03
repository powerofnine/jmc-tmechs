using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Throw : StateMachineBehaviour
    {
        public float throwForce = 5F;
        public float launchAngle = 45F;
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            EnemyTarget grabbed = Player.Instance.pickedUp;
            Player.Instance.pickedUp = null;

            EnemyTarget target = TargetController.Instance.GetTarget<EnemyTarget>();
            
            grabbed.HandleThrow();
            grabbed.transform.SetParent(null);

            Vector3 ballisticVelocity;

            if (target)
                ballisticVelocity = Utility.BallisticVelocity(grabbed.transform.position, target.transform.position, launchAngle);
            else
                ballisticVelocity = Utility.BallisticVelocity(grabbed.transform.position, animator.transform.position + animator.transform.forward * throwForce, launchAngle);
            
            grabbed.GetComponent<Rigidbody>().velocity = ballisticVelocity;
        }
    }
}
