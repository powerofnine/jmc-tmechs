using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class MoveForward : StateMachineBehaviour
    {
        public float speedMultiplier = 1F;
        
        private static readonly int PLAYER_SPEED = Animator.StringToHash("Player Speed");

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.transform.Translate(Vector3.forward * animator.GetFloat(PLAYER_SPEED) * speedMultiplier * Time.deltaTime);
        }
    }
}
