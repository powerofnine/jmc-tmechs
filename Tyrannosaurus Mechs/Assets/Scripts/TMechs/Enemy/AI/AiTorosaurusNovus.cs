using System;
using Animancer;
using TMechs.Animation;
using TMechs.Entity;
using TMechs.Types;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    public class AiTorosaurusNovus : MonoBehaviour, EntityHealth.IDamage
    {
        public AiStateMachine stateMachine;
        public ToroProperties properties;
        
        private void Start()
        {
            ToroShared shared = new ToroShared()
            {
                parent = this,
                animancer = GetComponentInChildren<EventfulAnimancerComponent>(),
                controller = GetComponent<CharacterController>()
            };
            
            shared.animancer.onEvent = new AnimationEventReceiver(null, OnAnimatorEvent);
            
            CreateStateMachine(shared);
        }

        private void CreateStateMachine(ToroShared shared)
        {
            stateMachine = new AiStateMachine(transform)
            {
                    target = Player.Player.Instance.transform,
                    shared = shared
            };
            
            stateMachine.ImportProperties(properties);
            
            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(null, "Notice");
            stateMachine.RegisterState(null, "Chasing");
            stateMachine.RegisterState(null, "Charge");
            stateMachine.RegisterState(null, "Standby");
            stateMachine.RegisterState(null, "Thrash");
            
            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle", machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>(nameof(ToroProperties.rangeStopFollow)));
            stateMachine.RegisterTransition("Idle", "Notice", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(ToroProperties.rangeStartFollow)));
            
            stateMachine.RegisterTransition("Chasing", "Charge", machine => machine.GetAddSet<float>("ChargeTimer", -Time.deltaTime) <= 0F, machine => machine.Set("ChargeTimer", machine.Get<float>(nameof(ToroProperties.chargeCooldown))));
            
            stateMachine.RegisterTransition("Chasing", "Standby", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(ToroProperties.stoppingRange)));
            stateMachine.RegisterTransition("Standby", "Chasing", machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>(nameof(ToroProperties.attackRange)));

            stateMachine.RegisterTransition("Standby", "Thrash", machine => machine.GetAddSet<float>("AttackTimer", -Time.deltaTime, machine.Get<float>(nameof(ToroProperties.attackCooldown))) < 0F, machine => machine.Set("AttackTimer", machine.Get<float>(nameof(ToroProperties.attackCooldown))));
            
            /*
             * Explicit transitions:
             * Notice -> Chasing
             * Charge -> Chasing
             * Thrash -> Standby
             */
            
            stateMachine.SetDefaultState("Idle");
            stateMachine.RegisterVisualizer($"HorndriverNovus:{name}");
        }
        
        #region States

        private class ToroState : AiStateMachine.State
        {
            private ToroShared shared;
            
            public override void OnInit()
            {
                base.OnInit();
                
                shared = Machine.shared as ToroShared;
            }
        }
        
        #endregion

        private void OnAnimatorEvent(AnimationEvent e)
        {
            stateMachine.OnEvent(AiStateMachine.EventType.Animation, e.stringParameter);
        }
        
        public void OnDamaged(EntityHealth health, ref bool cancel)
        {
            if("Idle".Equals(stateMachine.CurrentStateName))
                stateMachine.EnterState("Notice");
        }

        private class ToroShared
        {
            public AiTorosaurusNovus parent;
            public EventfulAnimancerComponent animancer;
            public CharacterController controller;
        }

        [Serializable]
        public class ToroProperties
        {
            [Header("Ranges")]
            public Radius rangeStartFollow;
            public Radius rangeStopFollow;
            public Radius attackRange;
            public Radius stoppingRange;

            [Header("Charge")]
            public float chargeCooldown = 4F;

            [Header("Attack")]
            public float attackCooldown = 1F;
        }

        [AnimationCollection.EnumAttribute("Horndriver Animations")]
        public enum ToroAnimations
        {
            Idle1,
            Idle2,
            Wander,
            Run,
            Primer,
            TakeDamage,
            Death,
            
            [Header("Attack")]
            ChargeIntro,
            Charge,
            Thrash
            
        }
    }
}
