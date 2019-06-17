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
            stateMachine.RegisterTransition("Chasing", "Attack", machine => machine.GetTrigger("Attack"));
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
            
            
        }

        private class HarrierShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}