﻿using System;
using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Grapple : StateMachineBehaviour
    {
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

            if (!target)
                return;
            
            radius = target.radius;
            pullSpeed = pullSpeedMin;

            grappleType = target.isSwing ? Types.Swing : Types.Pull;

            Player.Instance.Movement.disableControllerMovement = true;
            target.OnGrapple();
            
            Player.Instance.transform.LookAt(target.transform.position.Set(Player.Instance.transform.position.y, Utility.Axis.Y));
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (!target)
            {
                animator.SetTrigger(Anim.GRAPPLE_END);
                return;
            }
            
            switch (grappleType)
            {
                case Types.Pull:
                    if (PullPhysics(Transform, target.transform))
                        animator.SetTrigger(Anim.GRAPPLE_END);
                    break;
                case Types.Swing:
                    if (!animator.GetBool(Anim.RIGHT_ARM_HELD))
                    {
                        animator.SetTrigger(Anim.GRAPPLE_END);
                        return;
                    }

                    SwingPhysics(Transform, target.transform);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            target.OnGrapple();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            Player.Instance.Movement.velocity = velocity;
            Player.Instance.Movement.disableControllerMovement = false;
            target.OnGrapple();
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