using UnityEngine;
using UnityEngine.Serialization;

namespace TMechs.Enemy.AI
{
    public class AiHarrier : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        public static readonly int ATTACK = Animator.StringToHash("Attack");
        public static readonly int MOVE = Animator.StringToHash("Move");
        public static readonly int FIRING = Animator.StringToHash("IsFiring");

        public AiStateMachine stateMachine;

        public HarrierProperties properties = new HarrierProperties();

        private void Start()
        {
            stateMachine = new AiStateMachine(transform)
                    {target = Player.Player.Instance.transform};

            properties.controller = GetComponent<CharacterController>();
            properties.animator = GetComponentInChildren<Animator>();
            stateMachine.SetProperties(properties);

            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(new Chasing(), "Chasing");
            stateMachine.RegisterState(new Shooting(), "Shooting");
            stateMachine.RegisterState(new Attacking(), "Attacking");

            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle",
                    (transform, target, machine) => machine.DistanceToTarget > properties.rangeStopFollow);

            stateMachine.RegisterTransition("Idle", "Chasing",
                    (transform, target, machine) => machine.DistanceToTarget <= properties.rangeStartFollow);

            stateMachine.RegisterTransition("Chasing", "Attacking",
                    (transform, target, machine) => machine.DistanceToTarget <= properties.attackRange && (properties.attackTimer -= Time.deltaTime) <= 0F);
            stateMachine.RegisterTransition("Chasing", "Shooting",
                    (transform, target, machine) =>
                    {
                        properties.nextShoot -= Time.deltaTime;
                        if (properties.nextShoot <= 0F)
                        {
                            properties.nextShoot = Random.Range(properties.shootFrequency.x, properties.shootFrequency.y);

                            return machine.HorizontalDistanceToTarget <= properties.shootRange;
                        }

                        return false;
                    });

            stateMachine.RegisterTransition("Attacking", "Chasing",
                    (transform, target, machine) => properties.attackReleased);

            stateMachine.RegisterTransition("Shooting", "Chasing",
                    (transform, target, machine) => machine.HorizontalDistanceToTarget > properties.shootRange || properties.shootTimer <= 0F);

            stateMachine.SetDefaultState("Idle");
            stateMachine.RegisterVisualizer($"Harrier:{name}");
        }

        public void OnAnimationEvent(string id)
        {
            stateMachine.OnAnimationEvent(id);
        }

        private class Chasing : AiStateMachine.State
        {
            public override void OnTick()
            {
                throw new System.NotImplementedException();
            }
        }

        private class Shooting : AiStateMachine.State
        {
            private HarrierProperties props;

            private float nextShot;

            public override void OnEnter()
            {
                base.OnEnter();

                props = properties as HarrierProperties ?? new HarrierProperties();

                props.shootTimer = Random.Range(props.shootTime.x, props.shootTime.y);
                props.animator.SetBool(FIRING, true);

                nextShot = 1F / props.fireRate;
            }

            public override void OnExit()
            {
                base.OnExit();

                props.animator.SetBool(FIRING, false);
            }

            public override void OnTick()
            {
                nextShot -= Time.deltaTime;
                if (nextShot <= 0F)
                {
                    nextShot = 1F / props.fireRate;
                    //TODO fire
                }
            }
        }

        private class Attacking : AiStateMachine.State
        {
            private HarrierProperties props;

            private int substate = 0;

            private bool attackTriggered = true;

            public override void OnEnter()
            {
                base.OnEnter();

                props = properties as HarrierProperties ?? new HarrierProperties();
                props.attackReleased = false;

                props.attackTimer = props.attackCooldown;

                substate = 0;
            }

            public override void OnTick()
            {
                if (!attackTriggered)
                    return;

                if (DistanceToTarget > props.attackRange)
                {
                    props.attackReleased = true;
                    return;
                }

                if (substate < 2)
                {
                    props.animator.SetTrigger(ATTACK);
                    attackTriggered = false;

                    substate++;
                    if (Random.Range(0, 100) > props.secondaryAttackChance)
                        substate = 4096;
                    return;
                }

                props.attackReleased = true;
            }

            public override void OnAnimationEvent(string id)
            {
                base.OnAnimationEvent(id);

                if ("attack".Equals(id))
                    attackTriggered = true;
            }
        }

        [System.Serializable]
        public class HarrierProperties : AiStateMachine.AiProperties
        {
            [Header("Ranges")]
            public float rangeStartFollow = 15F;
            public float rangeStopFollow = 25F;
            public float attackRange = 1F;
            public float shootRange = 10F;

            [Header("Chasing")]
            public float dashSpeed = 10F;

            [Header("Shooting")]
            public Vector2 shootFrequency = new Vector2(5F, 10F);
            public Vector2 shootTime = new Vector2(2F, 5F);
            public float fireRate = .5F;
            public int shotDamage = 1;

            [System.NonSerialized]
            public float nextShoot = 0F;
            [System.NonSerialized]
            public float shootTimer;

            [Header("Attacking")]
            public float attackCooldown = 2F;
            [Range(0F, 1F)]
            public float secondaryAttackChance = .5F;

            [System.NonSerialized]
            public bool attackReleased;
            [System.NonSerialized]
            public float attackTimer;

            [System.NonSerialized]
            public Animator animator;
            [System.NonSerialized]
            public CharacterController controller;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, properties.rangeStartFollow * transform.lossyScale.x);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, properties.rangeStopFollow * transform.lossyScale.x);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, properties.attackRange * transform.lossyScale.x);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, properties.shootRange * transform.lossyScale.x);
        }
    }
}