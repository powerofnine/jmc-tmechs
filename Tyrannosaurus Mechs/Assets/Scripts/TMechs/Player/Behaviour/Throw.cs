using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Throw : StateMachineBehaviour
    {
        public float throwForce = 5F;
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            EnemyTarget target = Player.Instance.pickedUp;
            Player.Instance.pickedUp = null;
            
            target.HandleThrow();
            target.transform.SetParent(null);
            target.GetComponent<Rigidbody>().velocity = (animator.transform.forward + animator.transform.up) * throwForce;
        }
    }
}
