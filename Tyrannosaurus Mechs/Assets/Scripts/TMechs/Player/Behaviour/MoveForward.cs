using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class MoveForward : StateMachineBehaviour
    {
        public float speedMultiplier = 1F;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Player.Instance.Controller.Move(animator.GetFloat(Anim.PLAYER_SPEED) * speedMultiplier * Time.deltaTime * animator.transform.forward);
        }
    }
}