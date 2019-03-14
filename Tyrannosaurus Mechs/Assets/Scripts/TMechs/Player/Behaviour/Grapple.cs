using System;
using System.Linq;
using TMechs.Environment.Targets;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.Player.Behaviour
{
    public class Grapple : StateMachineBehaviour
    {
        private static readonly int GRAPPLE_DOWN = Animator.StringToHash("Grapple Down");
        private static readonly int GRAPPLE_END = Animator.StringToHash("Grapple End");

        private GrappleTarget target;
        private Types grappleType;
        private Vector3 velocity;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            velocity = Vector3.zero;
            
            target = TargetController.Instance.GetTarget<GrappleTarget>();
            grappleType = target.isSwing ? Types.SWING : Types.PULL;

            switch (grappleType)
            {
                case Types.PULL:
                    animator.SetTrigger(GRAPPLE_END);
                    break;
                case Types.SWING:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            switch (grappleType)
            {
                case Types.PULL:
                    animator.SetTrigger(GRAPPLE_END);
                    break;
                case Types.SWING:
                    if (!animator.GetBool(GRAPPLE_DOWN))
                    {
                        animator.SetTrigger(GRAPPLE_END);
                        return;
                    }

                    SwingPhysics(animator.transform, target.transform);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            animator.GetComponent<PlayerMovement>().velocity = velocity;
        }

        private void SwingPhysics(Transform ball, Transform anchor)
        {
            velocity.y -= 9.9F * Time.deltaTime;

            Vector3 tensionDir = (anchor.position - ball.position).normalized;
            Vector3 sideDir = (Quaternion.Euler(0F, 90F, 0F) * tensionDir).Remove(Utility.Axis.Y);
            sideDir.Normalize();

            float incline = Vector3.Angle(ball.position - anchor.position, Vector3.down);

            float tensionForce = 9.81F * Mathf.Cos(incline * Mathf.Deg2Rad);
            float centripetalForce = Mathf.Pow(velocity.magnitude, 2) / target.radius;
            tensionForce += centripetalForce;

            velocity += tensionDir * tensionForce * Time.deltaTime;

            ball.position = ClampPosition(anchor.position, ball.position + velocity * Time.deltaTime);
        }
        
        private Vector3 ClampPosition(Vector3 anchorPos, Vector3 newPos)
        {
            return anchorPos + target.radius * Vector3.Normalize(newPos - anchorPos);
        }

        private enum Types
        {
            PULL,
            SWING
        }
    }
}