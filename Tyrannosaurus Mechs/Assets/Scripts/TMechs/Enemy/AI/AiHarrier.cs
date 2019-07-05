using System;
using JetBrains.Annotations;
using TMechs.Types;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMechs.Enemy.AI
{
    public sealed class AiHarrier : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        public static readonly int MOVE = Anim.Hash("Move");
        public static readonly int FIRING = Anim.Hash("IsFiring");

        public AiStateMachine stateMachine;

        public HarrierProperties properties = new HarrierProperties();

        private void Start()
        {
            CreateStateMachine(new HarrierShared {controller = GetComponent<CharacterController>(), animator = GetComponentInChildren<Animator>()});
        }

        private void CreateStateMachine(HarrierShared shared)
        {
            stateMachine = new AiStateMachine(transform)
            {
                    target = PlayerOld.Player.Instance.transform,
                    shared = shared
            };

            stateMachine.ImportProperties(properties);

            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(new Chasing(), "Chasing");
            stateMachine.RegisterState(new Shooting(), "Shooting");
            stateMachine.RegisterState(new Attacking(), "Attacking");

            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle",
                    machine => machine.DistanceToTarget > machine.Get<Radius>("rangeStopFollow"));
            stateMachine.RegisterTransition("Idle", "Chasing",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("rangeStartFollow"));

            stateMachine.RegisterTransition("Chasing", "Attacking",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("attackRange") && machine.GetAddSet<float>("attackTimer", -Time.deltaTime) <= 0F);
            stateMachine.RegisterTransition("Chasing", "Shooting",
                    machine =>
                    {
                        if (machine.GetAddSet<float>("nextShoot", -Time.deltaTime) <= 0F)
                        {
                            Vector2 frequency = machine.Get<Vector2>("shootFrequency");
                            machine.Set("nextShoot", Random.Range(frequency.x, frequency.y));

                            return machine.HorizontalDistanceToTarget <= machine.Get<Radius>("shootRange");
                        }

                        return false;
                    });

            stateMachine.RegisterTransition("Attacking", "Chasing",
                    machine => machine.GetTrigger("attackReleased"));

            stateMachine.RegisterTransition("Shooting", "Chasing",
                    machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>("shootRange") || machine.GetAddSet<float>("shootTimer", -Time.deltaTime) <= 0F);

            stateMachine.SetDefaultState("Idle");
            stateMachine.RegisterVisualizer($"Harrier:{name}");
        }

        private void Update()
        {
            stateMachine.Tick();
        }

        public void OnAnimationEvent(string id)
        {
            stateMachine.OnEvent(AiStateMachine.EventType.Animation, id);
        }

        #region States

        private abstract class HarrierState : AiStateMachine.State
        {
            protected HarrierShared props;

            public override void OnEnter()
            {
                base.OnEnter();

                props = Machine.shared as HarrierShared;
            }
        }

        private class Chasing : HarrierState
        {
            private float dashTimer;
            private Vector3 dashDirection;

            public override void OnEnter()
            {
                base.OnEnter();

                dashTimer = 0F;
                dashDirection = Vector3.zero;
            }

            public override void OnTick()
            {
                Vector3 heading = target.position - transform.position;
                Vector3 fullDirection = heading / heading.magnitude;

                float yHeading = heading.y;
                heading = heading.Remove(Utility.Axis.Y);

                float distance = heading.magnitude;

                Vector3 direction = heading / distance;

                transform.forward = fullDirection;

                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0F)
                {
                    if (dashDirection.magnitude > float.Epsilon)
                    {
                        dashTimer = .5F;
                        dashDirection = Vector3.zero;
                    }
                    else
                    {
                        dashTimer = Random.Range(2F, 4F);
                        dashDirection = direction;

                        if (Mathf.Abs(yHeading) > 15F)
                            dashDirection.y = Mathf.Sign(yHeading);
                        else
                            dashDirection.y = Random.Range(-.75F, .75F);
                    }
                }

                props?.controller.Move(Machine.Get<float>("dashSpeed") * Time.deltaTime * dashDirection);
            }
        }

        private class Shooting : HarrierState
        {
            private bool left;
            
            public override void OnEnter()
            {
                base.OnEnter();

                Vector2 shootTime = Machine.Get<Vector2>("shootTime");
                Machine.Set("shootTimer", Random.Range(shootTime.x, shootTime.y));
                props?.animator.SetBool(FIRING, true);
            }

            public override void OnExit()
            {
                base.OnExit();

                props.animator.SetBool(FIRING, false);
            }

            public override void OnTick()
            {
                transform.forward = DirectionToTarget;
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                if ("fire".Equals(id))
                {
                    Transform anchor = left ? Machine.Get<Transform>("leftGunAnchor") : Machine.Get<Transform>("rightGunAnchor");
                    left = !left;
                    
                    GameObject go = Instantiate(Machine.Get<GameObject>("bulletTemplate"), anchor.position, anchor.rotation);
                    EnemyBullet bullet = go.GetComponent<EnemyBullet>();
                    bullet.damage = Machine.Get<int>("shotDamage");
                    bullet.direction = DirectionToTarget;
                    bullet.owner = transform;
                }
            }
        }

        private class Attacking : HarrierState
        {
            private int substate;

            private bool attackTriggered = true;

            public override void OnEnter()
            {
                base.OnEnter();

                substate = 0;
                Machine.Set("attackTimer", Machine.Get<float>("attackCooldown"));
            }

            public override void OnTick()
            {
                if (!attackTriggered)
                    return;

                transform.forward = DirectionToTarget;
                
                if (DistanceToTarget > Machine.Get<Radius>("attackRange"))
                {
                    Machine.SetTrigger("attackReleased");
                    return;
                }

                if (substate < 2)
                {
                    props.animator.SetTrigger(Anim.ATTACK);
                    attackTriggered = false;

                    substate++;
                    if (Random.Range(0, 100) > Machine.Get<float>("secondaryAttackChance") * 100F)
                        substate = 4096;
                    return;
                }

                Machine.SetTrigger("attackReleased");
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type == AiStateMachine.EventType.Animation && "attack".Equals(id))
                {
                    attackTriggered = true;
                    
                    if(DistanceToTarget <= Machine.Get<Radius>("attackRange") && AngleToTarget <= 35F)
                        PlayerOld.Player.Instance.Damage(Machine.Get<int>("attackDamage"));
                }
            }
        }

        #endregion States

        [Serializable]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class HarrierProperties
        {
            [Header("Ranges")]
            public Radius rangeStartFollow = new Radius(15F);
            public Radius rangeStopFollow = new Radius(25F);
            public Radius attackRange = new Radius(1F);
            public Radius shootRange = new Radius(10F);

            [Header("Chasing")]
            public float dashSpeed = 10F;

            [Header("Shooting")]
            public Vector2 shootFrequency = new Vector2(5F, 10F);
            public Vector2 shootTime = new Vector2(2F, 5F);
            public int shotDamage = 1;
            public Transform leftGunAnchor;
            public Transform rightGunAnchor;
            public GameObject bulletTemplate;

            [Header("Attacking")]
            public float attackCooldown = 2F;
            [Range(0F, 1F)]
            public float secondaryAttackChance = .5F;
            public int attackDamage = 10;
        }

        private class HarrierShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}