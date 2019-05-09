using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class MoveForward : StateMachineBehaviour
    {
        public float speedMultiplier = 1F;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Player.Instance.Movement.motion = animator.GetFloat(Anim.PLAYER_SPEED) * speedMultiplier * animator.transform.forward;
        }
    }
}