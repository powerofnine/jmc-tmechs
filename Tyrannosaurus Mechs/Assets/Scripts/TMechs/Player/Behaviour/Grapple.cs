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
        private static readonly Type[] DISABLE_TYPES =
        {
                typeof(PlayerMovement),
                typeof(Collider)
        };

        private Types grappleType;

        private ConfigurableJoint joint;
        private Rigidbody rb;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            rb = animator.GetComponent<Rigidbody>();
            
            GrappleTarget target = TargetController.Instance.GetTarget<GrappleTarget>();
            grappleType = target.isSwing ? Types.SWING : Types.PULL;

            switch (grappleType)
            {
                case Types.PULL:
                    animator.SetTrigger(GRAPPLE_END);
                    break;
                case Types.SWING:
                    joint = animator.gameObject.AddComponent<ConfigurableJoint>();
                    ConfigureJoint(joint);
                    joint.anchor = animator.transform.InverseTransformPoint(target.transform.position);

                    rb.constraints = RigidbodyConstraints.None;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetDisableTypeState(animator, false);
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
                    if(!animator.GetBool(GRAPPLE_DOWN))
                        animator.SetTrigger(GRAPPLE_END);
                    
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

            rb.constraints = RigidbodyConstraints.FreezeRotation;
            
            SetDisableTypeState(animator, true);
        }

        private void SetDisableTypeState(Component animator, bool enabled)
        {
            foreach (Component c in DISABLE_TYPES.Select(animator.GetComponent))
            {
                switch (c)
                {
                    case UnityEngine.Behaviour behaviour:
                        behaviour.enabled = enabled;
                        break;
                    case Collider collider:
                        collider.enabled = enabled;
                        break;
                }
            }
        }

        // TODO: load joint from preset (will need custom preset loader cause presets are editor-only for some stupid reason
        private static void ConfigureJoint(ConfigurableJoint joint)
        {
            joint.axis = Vector3.one;
            
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
        }
        
        private enum Types
        {
            PULL,
            SWING
        }
    }
}
