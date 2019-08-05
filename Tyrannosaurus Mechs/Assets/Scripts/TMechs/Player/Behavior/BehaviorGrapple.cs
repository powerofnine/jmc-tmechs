using System;
using Animancer;
using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorGrapple : PlayerBehavior
    {
        public float pullSpeedMin = 50F;
        public float pullSpeedMax = 100F;
        public float pullExitDistance = 2F;
        
        [Space]
        public float ikTime = .25F;
        
        private GrappleTarget target;
        private Types grappleType;
        private Vector3 velocity;

        private bool transitionComplete;
        private float radius;

        private float pullSpeed;

        private AnimancerState grapple;

        public override void OnInit()
        {
            base.OnInit();

            grapple = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Grapple), 1);
        }

        public override void OnPush()
        {
            base.OnPush();
            
            velocity = Vector3.zero;
            transitionComplete = false;

            target = TargetController.Instance.GetTarget<GrappleTarget>();

            if (!target)
            {
                player.PopBehavior();
                return;
            }

            radius = target.radius;
            pullSpeed = pullSpeedMin;
            
            grappleType = target.isSwing ? Types.Swing : Types.Pull;
            player.forces.enabled = false;
            player.forces.ResetGround();
            
            target.OnGrapple();
            transform.LookAt(target.transform.position.Set(Player.Instance.transform.position.y, Utility.Axis.Y));
            player.movement.ResetIntendedY();

            Animancer.CrossFadeFromStart(grapple);
            if (player.rightArmIk)
                player.rightArmIk.Transition(ikTime, 1F);
        }

        public override void OnPop()
        {
            base.OnPop();

            player.forces.velocity = velocity;
            player.forces.enabled = true;
            target.OnGrapple();
            
            Animancer.GetLayer(1).StartFade(0F);
            
            if(player.rightArmIk)
                player.rightArmIk.Transition(ikTime, 0F);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!target)
            {
                player.PopBehavior();
                return;
            }

            if (player.rightArmIk)
                player.rightArmIk.targetPosition = target.transform.position;
            
            switch (grappleType)
            {
                case Types.Pull:
                    if (PullPhysics(transform, target.transform))
                        player.PopBehavior();
                    break;
                case Types.Swing:
                    if (!Input.GetButton(Controls.Action.RIGHT_ARM))
                    {
                        player.PopBehavior();
                        return;
                    }

                    SwingPhysics(transform, target.transform);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
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

        public override bool CanMove() => false;

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