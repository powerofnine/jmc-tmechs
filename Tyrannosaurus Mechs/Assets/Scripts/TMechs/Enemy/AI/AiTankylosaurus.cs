using System;
using JetBrains.Annotations;
using TMechs.Types;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    public class AiTankylosaurus : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        public AiStateMachine stateMachine;

        public TankylosaurusProperties properties = new TankylosaurusProperties();

        private void Awake()
        {
            CreateStateMachine(new TankyloShared() {animator = GetComponentInChildren<Animator>(), controller = GetComponent<CharacterController>()});
        }

        private void Update()
        {
            stateMachine.Tick();
        }
        
        public void OnAnimationEvent(string id)
        {
            stateMachine.OnEvent(AiStateMachine.EventType.Animation, id);
        }

        private void CreateStateMachine(TankyloShared shared)
        {
            stateMachine = new AiStateMachine(transform) {target = Player.Player.Instance.transform, shared = shared};
            
            stateMachine.ImportProperties(properties);
            
            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(null, "Chasing");
            stateMachine.RegisterState(null, "Attack");
            stateMachine.RegisterState(null, "RockThrow");
            
            
            
            stateMachine.SetDefaultState("Idle");
            stateMachine.RegisterVisualizer($"Tankylosaurus:{name}");
        }

        private abstract class TankyloState : AiStateMachine.State
        {
            protected TankyloShared shared;

            public override void OnEnter()
            {
                base.OnEnter();
                
                shared = Machine.shared as TankyloShared;
            }
        }
        
        [Serializable]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class TankylosaurusProperties
        {
            [Header("Range")]
            public Radius rangeStartFollow = new Radius(25F);
            public Radius rangeStopFollow = new Radius(35F);
            public Radius rockThrowRange = new Radius(15F);
            public Radius attackRange = new Radius(1F);
        }

        private class TankyloShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}