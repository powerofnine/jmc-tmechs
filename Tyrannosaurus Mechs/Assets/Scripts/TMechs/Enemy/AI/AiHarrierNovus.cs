using System;
using JetBrains.Annotations;
using TMechs.Types;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    public class AiHarrierNovus : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        public AiStateMachine stateMachine;

        public HarrierProperties properties = new HarrierProperties();

        private void Start()
        {
            CreateStateMachine(new HarrierShared()
            {
                    animator = GetComponentInChildren<Animator>(),
                    controller = GetComponent<CharacterController>()
            });
        }

        private void CreateStateMachine(HarrierShared shared)
        {
            stateMachine = new AiStateMachine(transform)
            {
                    target = Player.Player.Instance.transform,
                    shared = shared
            };

            stateMachine.ImportProperties(properties);
            
            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(null, "Moving");
            stateMachine.RegisterState(null, "Shooting");
            stateMachine.RegisterState(null, "Chasing");
            stateMachine.RegisterState(null, "Attack");
            
            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle", machine => machine.DistanceToTarget > machine.Get<Radius>(nameof(HarrierProperties.rangeStopFollow)));
            stateMachine.RegisterTransition("Idle", "Moving", machine => machine.DistanceToTarget <= machine.Get<Radius>(nameof(HarrierProperties.rangeStartFollow)));
            
            stateMachine.RegisterTransition("Moving", "Shooting", machine => machine.GetTrigger("Shoot"));
            stateMachine.RegisterTransition("Shooting", "Chasing", machine => machine.GetTrigger("ShootDone"));
            stateMachine.RegisterTransition("Chasing", "Attack", machine => machine.DistanceToTarget <= machine.Get<Radius>("attackRange"));
            stateMachine.RegisterTransition("Attack", "Moving", machine => machine.GetTrigger("AttackDone"));
            
            stateMachine.RegisterVisualizer($"HarrierNovus:{name}");
        }

        public void OnAnimationEvent(string id)
        {
            throw new System.NotImplementedException();
        }

        [Serializable]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class HarrierProperties
        {
            [Header("Ranges")]
            public Radius rangeStartFollow;
            public Radius rangeStopFollow;

            [Header("Moving")]
            public int moveCount = 3;
            public Radius dashDistance = new Radius(10F, true);
            public float dashDelay = 1F;
            public float dashSpeed = 10F;

            [Header("Shooting")]
            public int shotCount = 5;
            public float shotDelay = .25F;
            
            [Header("Chasing")]
            public Radius chaseDashDistance = new Radius(15F, true);
            public float chaseDashDelay = .1F;
            public float chaseDashSpeed = 20F;

            [Header("Attacking")]
            [Range(0F, 1F)]
            public float secondaryAttackChance = .5F;
            public float attackDamage = 5F;
            public Radius attackRange;
        }

        private class HarrierShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}