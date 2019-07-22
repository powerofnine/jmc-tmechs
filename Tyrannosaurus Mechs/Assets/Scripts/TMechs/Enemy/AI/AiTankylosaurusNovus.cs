using System;
using Animancer;
using TMechs.Animation;
using TMechs.Attributes;
using TMechs.Entity;
using TMechs.Types;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    public class AiTankylosaurusNovus : MonoBehaviour, EntityHealth.IDeath
    {
        public AiStateMachine stateMachine;
        public TankyloProperties properties = new TankyloProperties();
        [AnimationCollection.ValidateAttribute(typeof(TankyloAnimation))]
        public AnimationCollection animations;
        
        private void Start()
        {
            TankyloShared shared = new TankyloShared()
            {
                    parent = this,
                    animancer = GetComponentInChildren<EventfulAnimancerComponent>(),
                    health = GetComponent<EntityHealth>()
            };

            shared.animancer.onEvent = new AnimationEventReceiver(null, OnAnimationEvent);
            
            CreateStateMachine(shared);
        }

        private void CreateStateMachine(TankyloShared shared)
        {
            stateMachine = new AiStateMachine(transform)
            {
                    target = Player.Player.Instance.centerOfMass,
                    shared = shared
            };
            
            stateMachine.ImportProperties(properties);
            
            // States
            stateMachine.RegisterState(null, "Primer");
            
            stateMachine.RegisterState(null, "Chase");
            stateMachine.RegisterState(null, "Rock Throw");
            
            stateMachine.RegisterState(null, "Standby");
            stateMachine.RegisterState(null, "Shotgun");
            stateMachine.RegisterState(null, "Tail Whip");
            
            stateMachine.RegisterState(null, "Enter Rage");
            
            // Transitions
            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Enter Rage", machine => !machine.Get("rage", false) && shared.health.Health <= .25F, machine => machine.Set("rage", true));
            stateMachine.RegisterTransition("Enter Rage", "Chase", machine => machine.GetTrigger("RageDone"));
            
            stateMachine.RegisterTransition("Chase", "Standby", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(TankyloProperties.midStopRange)));
            stateMachine.RegisterTransition("Standby", "Chase", machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>(nameof(TankyloProperties.midRange)));
            
//            stateMachine.RegisterTransition("Chase", "Rock Throw", );
            
            // State Machine
            stateMachine.SetDefaultState("Primer");
            stateMachine.RegisterVisualizer($"Tankylosaurus:{name}");
        }
        
        private class TankyloState : AiStateMachine.State
        {
            public TankyloShared shared;

            public override void OnInit()
            {
                base.OnInit();
                
                shared = (TankyloShared)Machine.shared;
            }

            public AnimationClip GetClip(TankyloAnimation clip)
            {
                if (!shared.parent.animations)
                    return null;
                
                return shared.parent.animations.GetClip(clip);
            }
        }

        public void OnDying(ref bool customDestroy)
        {
            customDestroy = true;
        }

        private void OnAnimationEvent(AnimationEvent e)
        {
            
        }
        
        private class TankyloShared
        {
            public AiTankylosaurusNovus parent;
            public EventfulAnimancerComponent animancer;
            public EntityHealth health;
        }

        [Serializable]
        public class TankyloProperties
        {
            [Header("Chase")]
            public Radius shortRange = new Radius(10F);
            public Radius midRange = new Radius(20F);
            public Radius midStopRange = new Radius(15F);

            [Header("Rock Throw")]
            [MinMax]
            public Vector2 rockThrowCooldown = new Vector2(2F, 5F);
        }

        [AnimationCollection.EnumAttribute("Tankylosaurus Animations")]
        public enum TankyloAnimation
        {
            
        }
    }
}