using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class RocketFist : StateMachineBehaviour
    {
        public GameObject template;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            Instantiate(template);
            
            animator.transform.Find("T_MechsProto_RocketFist").gameObject.SetActive(false);
        }
        
        private void RocketFistBegin()
        {
            
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            base.OnStateExit(animator, animatorStateInfo, layerIndex);
            
            animator.transform.Find("T_MechsProto_RocketFist").gameObject.SetActive(true);
        }
    }
}
