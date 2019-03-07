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
        }
    }
}
