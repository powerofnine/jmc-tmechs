using System;
using Animancer;
using TMechs.Animation;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorCarry : PlayerBehavior
    {
        public Transform pickupAnchor;
        public float grabRange = 10F;
        public float throwForce = 5F;
        public float throwSpeed = 100F;
        public float launchAngle = 10F;
        public float pummelDamage = 10F;

        [NonSerialized]
        public GameObject overrideTarget;

        [Space]
        public float ikTime = .5F;

        public EntityHealth.DamageSource pummelDamageSource;

        private AnimancerState grab;
        private AnimancerState yeet; // Throw is a reserved keyword
        private AnimancerState pummel;

        private GameObject target;
        private ThrowableContainer pickedUp;
        private bool hasPickedUp;
        private bool isThrowing;
        private bool isPummeling;

        public override void OnInit()
        {
            base.OnInit();

            grab = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.GrabObject), 1);
            yeet = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.ThrowObject), 1);
            pummel = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Attack1), 2);
        }

        public override void OnPush()
        {
            base.OnPush();

            if (overrideTarget)
                target = overrideTarget.gameObject;
            else
                target = TargetController.Instance.GetTarget<EnemyTarget>().gameObject;
            overrideTarget = null;
            pickedUp = null;

            hasPickedUp = false;
            isThrowing = false;
            isPummeling = false;

            if (!target)
            {
                player.PopBehavior();
                return;
            }

            Animancer.CrossFadeFromStart(grab, .1F).OnEnd = Grab;
        }

        public override void OnPop()
        {
            base.OnPop();

            if (player.rightArmIk)
            {
                player.rightArmIk.Stop();
                player.rightArmIk.weight = 0F;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // If object got destroyed whilst in our hands
            if (hasPickedUp && !pickedUp || !target)
            {
                grab.OnEnd = null;
                yeet.OnEnd = null;
                pummel.OnEnd = null;

                if(player.rightArmIk)
                    player.rightArmIk.Transition(.1F, 0F);
                
                Animancer.GetLayer(1).StartFade(0F);
                Animancer.GetLayer(2).StartFade(0F);
                player.PopBehavior();
                return;
            }

            if (isThrowing)
            {
                EnemyTarget target = TargetController.Instance.GetTarget<EnemyTarget>();

                if (target)
                {
                    transform.LookAt(target.transform.position.Set(Player.Instance.transform.position.y, Utility.Axis.Y));
                    player.movement.ResetIntendedY();
                }
            }

            if (player.rightArmIk)
                player.rightArmIk.targetPosition = target.transform.position;

            if (!hasPickedUp)
            {
                    transform.LookAt(target.transform.position.Set(Player.Instance.transform.position.y, Utility.Axis.Y));
                    player.movement.ResetIntendedY();
            }

            if (!hasPickedUp || isThrowing || isPummeling)
                return;

            if (Input.GetButtonDown(Controls.Action.RIGHT_ARM))
            {
                isThrowing = true;
                Animancer.CrossFadeFromStart(yeet, .1F);

                return;
            }

            GamepadLabels.EnableLabel(GamepadLabels.ButtonLabel.Attack, "Pummel");
            if (Input.GetButtonDown(Controls.Action.ATTACK))
            {
                isPummeling = true;

                Animancer.CrossFadeFromStart(pummel).OnEnd = () =>
                {
                    pummel.OnEnd = null;
                    isPummeling = false;
                    Animancer.GetLayer(2).StartFade(0F);
                };
            }
        }

        private void Grab()
        {
            grab.OnEnd = null;

            if (!target)
                return;

            if (!player.rightArmIk)
            {
                Grab_PostIk();
                return;
            }

            player.rightArmIk.Transition(ikTime, 1F, Grab_PostIk);
        }

        private void Grab_PostIk()
        {
            GameObject go = new GameObject($"ThrowableContainer:{target.name}");
            ThrowableContainer container = go.AddComponent<ThrowableContainer>();

            container.Initialize(target.gameObject);

            go.transform.SetParent(pickupAnchor, false);
            go.transform.localPosition = Vector3.zero;
            pickedUp = container;

            if (!player.rightArmIk)
            {
                hasPickedUp = true;
                return;
            }

            player.rightArmIk.Transition(ikTime, 0F, () => hasPickedUp = true);
        }

        private void Throw()
        {
            grab.OnEnd = null;
            pummel.OnEnd = null;

            if (pickedUp)
            {
                ThrowableContainer grabbed = pickedUp;
                pickedUp = null;

                EnemyTarget target = TargetController.Instance.GetTarget<EnemyTarget>();

                Vector3 pos = grabbed.transform.position;
                grabbed.transform.SetParent(null, false);
                grabbed.transform.position = pos;

                Vector3 throwTarget = transform.position + transform.forward * throwForce;
                float angle = launchAngle;
                if (target)
                    throwTarget = target.transform.position;

                if (Vector3.Distance(transform.position, throwTarget) < 15F)
                    angle = Mathf.Lerp(0F, angle, Vector3.Distance(transform.position, throwTarget) / 15F);

                grabbed.Throw(throwTarget, angle, throwSpeed);
            }

            Animancer.GetLayer(1).StartFade(0F);
            Animancer.GetLayer(2).StartFade(0F);
            player.PopBehavior();
        }

        public override void OnAnimationEvent(AnimationEvent e)
        {
            base.OnAnimationEvent(e);

            switch (e.stringParameter)
            {
                case "Throw":
                    Throw();
                    break;
                case "AttackHit":
                    pickedUp.DamageContainedObject(pummelDamage, pummelDamageSource.GetWithSource(transform));
                    break;
            }
        }

        public override bool CanMove() => hasPickedUp && !isThrowing && !isPummeling;
        public override bool CanRun() => false;
        public override float GetSpeed() => base.GetSpeed() * .6F;
    }
}