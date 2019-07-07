using System;
using Animancer;
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
        public float launchAngle = 45F;
        public float pummelDamage = 10F;
        
        private AnimancerState grab;
        private AnimancerState yeet; // Throw is a reserved keyword
        private AnimancerState pummel;

        private EnemyTarget target;
        private ThrowableContainer pickedUp;
        private bool hasPickedUp;
        private bool isThrowing;
        private bool isPummeling;
        
        public override void OnInit()
        {
            base.OnInit();

            grab = Animancer.CreateState(player.GetClip(Player.PlayerAnim.GrabObject), 1);
            yeet = Animancer.CreateState(player.GetClip(Player.PlayerAnim.ThrowObject), 1);
            pummel = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.Attack1), 2);
        }

        public override void OnPush()
        {
            base.OnPush();

            target = TargetController.Instance.GetTarget<EnemyTarget>();
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

        public override void OnUpdate()
        {
            base.OnUpdate();

            // If object got destroyed whilst in our hands
            if (hasPickedUp && !pickedUp)
            {
                grab.OnEnd = null;
                yeet.OnEnd = null;
                pummel.OnEnd = null;
                
                Animancer.GetLayer(1).StartFade(0F);
                Animancer.GetLayer(2).StartFade(0F);
                player.PopBehavior();
                return;
            }
            
            if (!hasPickedUp || isThrowing || isPummeling)
                return;
            
            GamepadLabels.AddLabel(IconMap.Icon.R2, "Throw");
            if (Input.GetButtonDown(Controls.Action.RIGHT_ARM))
            {
                isThrowing = true;
                Animancer.CrossFadeFromStart(yeet, .1F).OnEnd = Throw;
                return;
            }
            
            GamepadLabels.AddLabel(IconMap.Icon.ActionTopRow1, "Pummel");
            if (Input.GetButtonDown(Controls.Action.ATTACK))
            {
                isPummeling = true;

                Animancer.CrossFadeFromStart(pummel).OnEnd = () =>
                {
                    pummel.OnEnd = null;
                    isPummeling = false;
                    Animancer.GetLayer(2).StartFade(0F);
                    pickedUp.DamageContainedObject(pummelDamage);
                };
            }
        }

        private void Grab()
        {
            grab.OnEnd = null;
            
            if (!target)
                return;
            
            GameObject go = new GameObject($"ThrowableContainer:{target.name}");
            ThrowableContainer container = go.AddComponent<ThrowableContainer>();
            
            container.Initialize(target.gameObject);
            
            go.transform.SetParent(pickupAnchor, false);
            go.transform.localPosition = Vector3.zero;
            pickedUp = container;
            hasPickedUp = true;
        }

        private void Throw()
        {
            grab.OnEnd = null;
            yeet.OnEnd = null;
            pummel.OnEnd = null;

            if (pickedUp)
            {
                ThrowableContainer grabbed = pickedUp;
                pickedUp = null;

                EnemyTarget target = TargetController.Instance.GetTarget<EnemyTarget>();

                grabbed.transform.SetParent(null);

                Vector3 ballisticVelocity;

                if (target)
                    ballisticVelocity = Utility.BallisticVelocity(grabbed.transform.position, target.transform.position, launchAngle);
                else
                    ballisticVelocity = Utility.BallisticVelocity(grabbed.transform.position, transform.position + transform.forward * throwForce, launchAngle);

                grabbed.Throw(ballisticVelocity);
            }

            Animancer.GetLayer(1).StartFade(0F);
            Animancer.GetLayer(2).StartFade(0F);
            player.PopBehavior();
        }

        public override bool CanMove() => hasPickedUp && !isThrowing && !isPummeling;
        public override bool CanRun() => false;
        public override float GetSpeed() => base.GetSpeed() * .6F;
    }
}