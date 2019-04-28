using TMechs.Types;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    public class AiTorosaurus : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        public AiStateMachine stateMachine;

        public TorosaurusProperties properties = new TorosaurusProperties();
        
        private void Awake()
        {
            CreateStateMachine();
        }
     
        public void OnAnimationEvent(string id)
            => stateMachine.OnAnimationEvent(id);

        private void CreateStateMachine()
        {
            stateMachine = new AiStateMachine(transform);
            
            stateMachine.ImportProperties(properties);
            
            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(null, "Chasing");
            stateMachine.RegisterState(null, "Charging");
            stateMachine.RegisterState(null, "Attacking");
            
            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle", 
                    (machine) => machine.DistanceToTarget > properties.rangeStopFollow);
            stateMachine.RegisterTransition("Idle", "Chasing", 
                    (machine) => machine.DistanceToTarget <= properties.rangeStartFollow);
            
            
            
            
            stateMachine.SetDefaultState("Idle");
            stateMachine.RegisterVisualizer($"Torosaurus:{name}");
        }

        [System.Serializable]
        public class TorosaurusProperties
        {
            [Header("Range")]
            public float rangeStartFollow = 25F;
            public float rangeStopFollow = 35F;
            public float chargeRange = 15F;
            
            [Header("Charge")]
            [Range(0, 100)]
            public int chargeChance = 50;
            public float chargeCooldown = 5F;
            public float chargeMaxDistance = 20F;
        }
    }
}
