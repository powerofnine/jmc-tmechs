using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Dash : StateMachineBehaviour
    {
        public float speedMultiplier = 1F;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            Vector3 movement = Player.Input.GetAxis2DRaw(Controls.Action.MOVE_HORIZONTAL, Controls.Action.MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, Player.Instance.Movement.aaCamera.eulerAngles.y, 0F) * movement;
            
            float movementMag = movement.sqrMagnitude;

            if (movementMag > float.Epsilon)
                Player.Instance.transform.eulerAngles = Player.Instance.transform.eulerAngles.Set(Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg, Utility.Axis.Y);

            Player.Instance.Movement.ResetIntendedY();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Player.Instance.Movement.motion = animator.GetFloat(Anim.PLAYER_SPEED) * speedMultiplier * Player.Instance.transform.forward;
        }
    }
}