using System;
using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Grapple : StateMachineBehaviour
    {
        private static readonly int GRAPPLE_DOWN = Anim.Hash("Grapple Down");
        private static readonly int GRAPPLE_END = Anim.Hash("Grapple End");

        public float pullSpeedMin = 50F;
        public float pullSpeedMax = 100F;
        public float pullExitDistance = 2F;

        private GrappleTarget target;
        private Types grappleType;
        private Vector3 velocity;

        private bool transitionComplete;
        private float radius;

        private float pullSpeed;

        private Transform Transform => Player.Instance.transform;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            velocity = Vector3.zero;
            transitionComplete = false;

            target = TargetController.Instance.GetTarget<GrappleTarget>();
            radius = target.radius;
            pullSpeed = pullSpeedMin;

            grappleType = target.isSwing ? Types.Swing : Types.Pull;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            switch (grappleType)
            {
                case Types.Pull:
                    if (PullPhysics(Transform, target.transform))
                        animator.SetTrigger(GRAPPLE_END);
                    break;
                case Types.Swing:
                    if (!animator.GetBool(GRAPPLE_DOWN))
                    {
                        animator.SetTrigger(GRAPPLE_END);
                        return;
                    }

                    SwingPhysics(Transform, target.transform);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            Player.Instance.Movement.velocity = velocity;
        }

        public bool PullPhysics(Transform ball, Transform anchor, float exitDistance = float.NegativeInfinity)
        {
            if (float.IsNegativeInfinity(exitDistance))
                exitDistance = pullExitDistance;

            Vector3 heading = anchor.position - ball.position;
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;

            if (distance > exitDistance)
                ball.position += pullSpeed * Time.deltaTime * direction;
            else
                return true;

            pullSpeed = Mathf.Clamp(pullSpeed + Utility.GRAVITY, pullSpeedMin, pullSpeedMax);

            return false;
        }

        private void SwingPhysics(Transform ball, Transform anchor)
        {
            if (!transitionComplete)
            {
                if (PullPhysics(ball, anchor, target.radius))
                {
                    radius = Vector3.Distance(ball.position, anchor.position);
                    transitionComplete = true;
                }

                return;
            }

            velocity.y -= (Utility.GRAVITY + .1F) * Time.deltaTime;

            Vector3 tensionDir = (anchor.position - ball.position).normalized;
            Vector3 sideDir = (Quaternion.Euler(0F, 90F, 0F) * tensionDir).Remove(Utility.Axis.Y);
            sideDir.Normalize();

            float incline = Vector3.Angle(ball.position - anchor.position, Vector3.down);

            float tensionForce = Utility.GRAVITY * Mathf.Cos(incline * Mathf.Deg2Rad);
            float centripetalForce = Mathf.Pow(velocity.magnitude, 2) / radius;
            tensionForce += centripetalForce;

            velocity += tensionForce * Time.deltaTime * tensionDir;

            ball.position = ClampPosition(anchor.position, ball.position + velocity * Time.deltaTime);
        }

        private Vector3 ClampPosition(Vector3 anchorPos, Vector3 newPos)
        {
            return anchorPos + radius * Vector3.Normalize(newPos - anchorPos);
        }

        private enum Types
        {
            Pull,
            Swing
        }
    }
}