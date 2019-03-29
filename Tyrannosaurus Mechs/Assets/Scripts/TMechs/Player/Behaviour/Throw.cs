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
                ballisticVelocity = BallisticVelocity(grabbed.transform.position, target.transform.position, launchAngle);
            else
                ballisticVelocity = BallisticVelocity(grabbed.transform.position, animator.transform.position + animator.transform.forward * throwForce, launchAngle);
            
            grabbed.GetComponent<Rigidbody>().velocity = ballisticVelocity;
        }

        private Vector3 BallisticVelocity(Vector3 source, Vector3 target, float angle)
        {
            angle *= Mathf.Deg2Rad;
            
            Vector3 direction = target - source;
            float height = direction.y;
            direction.y = .0F;
            
            float distance = direction.magnitude;
            direction.y = distance * Mathf.Tan(angle);
            distance += height / Mathf.Tan(angle);
            
            float velocity = Mathf.Sqrt(distance * Utility.GRAVITY / Mathf.Sin(2 * angle));
            return velocity * direction.normalized;
        }
    }
}
