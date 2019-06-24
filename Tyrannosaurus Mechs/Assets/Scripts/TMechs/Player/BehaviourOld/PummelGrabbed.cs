using UnityEngine;

namespace TMechs.Player.BehaviourOld
{
    public class PummelGrabbed : StateMachineBehaviour
    {
        public float pummelDamage = 5F;
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (Player.Instance.pickedUp)
                Player.Instance.pickedUp.DamageContainedObject(pummelDamage);
        }
    }
}
