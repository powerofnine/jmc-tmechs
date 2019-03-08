using System;
using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Grapple : StateMachineBehaviour
    {
        private Types grappleType;

        private SpringJoint joint;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            GrappleTarget target = TargetController.Instance.GetTarget<GrappleTarget>();
            grappleType = target.isSwing ? Types.SWING : Types.PULL;

            switch (grappleType)
            {
                case Types.PULL:
                    break;
                case Types.SWING:
                    joint = animator.gameObject.AddComponent<SpringJoint>();
//                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = animator.transform.position;
                    joint.anchor = animator.transform.InverseTransformPoint(target.transform.position);
                    joint.maxDistance = Vector3.Distance(target.transform.position, animator.transform.position);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

//            joint.connectedAnchor = animator.transform.position;
            
            switch (grappleType)
            {
                case Types.PULL:
                    break;
                case Types.SWING:
                    if(!animator.GetBool("Grapple Down"))
                        animator.SetTrigger("Grapple End");
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            if(joint)
                Destroy(joint);
        }

        private enum Types
        {
            PULL,
            SWING
        }
    }
}
