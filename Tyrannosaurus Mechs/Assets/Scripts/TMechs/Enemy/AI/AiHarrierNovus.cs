using System;
using JetBrains.Annotations;
using TMechs.Types;
using UnityEngine;
using Random = UnityEngine.Random;

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
            stateMachine.RegisterTransition("Shooting", "Chasing", machine => machine.GetTrigger("ShootDone") && machine.GetAddSet<int>("ShootCount", 1) >= machine.Get<int>("shootsBeforeChase"), machine => machine.Set<int>("ShootCount", 0));
            stateMachine.RegisterTransition("Shooting", "Moving", machine => machine.GetTrigger("ShootDone"));
            stateMachine.RegisterTransition("Chasing", "Attack", machine => machine.DistanceToTarget <= machine.Get<Radius>("attackRange"));
            stateMachine.RegisterTransition("Attack", "Moving", machine => machine.GetTrigger("AttackDone"));
            
            stateMachine.RegisterVisualizer($"HarrierNovus:{name}");
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

        private class Moving : HarrierState
        {
            private int dashesDone;
            private float time;
            private Vector3 direction;
            
            public override void OnEnter()
            {
                base.OnEnter();

                dashesDone = 0;
                time = 0F;
            }

            public override void OnTick()
            {
                base.OnTick();

                transform.forward = DirectionToTarget;

                if (time >= 0F)
                {
                    time -= Time.deltaTime;

                    props.controller.Move(Machine.Get<float>("dashSpeed") * Time.deltaTime * direction);
                    
                    return;
                }

                if (dashesDone > Machine.Get<int>("moveCount"))
                {
                    Machine.SetTrigger("Shoot");
                    return;
                }
                
                if (direction.sqrMagnitude > Mathf.Epsilon)
                {
                    direction = Vector3.zero;
                    dashesDone++;
                    time = Machine.Get<float>("dashDelay");
                }
                else
                {
                    direction = new Vector3(Random.Range(-1F, 1F), Random.Range(-1F, 1F), 0F);

                    Vector3 heading = target.position - transform.position;

                    if (Math.Abs(HorizontalDistanceToTarget - Machine.Get<Radius>("maintainDistance")) > 4F)
                        direction = DirectionToTarget;
                    
                    if (Mathf.Abs(heading.y) > 10F)
                        direction.y = Mathf.Sign(heading.y);
                    
                    direction.Normalize();

                    time = Machine.Get<Radius>("dashDistance") / Machine.Get<float>("dashSpeed");
                }
            }
        }

        private class Shooting : HarrierState
        {
            private float nextShot;
            private bool leftSide;

            private int shots;

            public override void OnEnter()
            {
                base.OnEnter();

                nextShot = 0F;
                shots = 0;
            }

            public override void OnTick()
            {
                base.OnTick();

                transform.forward = DirectionToTarget;

                if (nextShot > 0F)
                {
                    nextShot -= Time.deltaTime;
                    return;
                }

                shots++;
                
                Transform anchor = leftSide ? Machine.Get<Transform>("leftGunAnchor") : Machine.Get<Transform>("rightGunAnchor");
                leftSide = !leftSide;
                    
                GameObject go = Instantiate(Machine.Get<GameObject>("bulletTemplate"), anchor.position, anchor.rotation);
                EnemyBullet bullet = go.GetComponent<EnemyBullet>();
                bullet.damage = Machine.Get<int>("shotDamage");
                bullet.direction = transform.forward;
                bullet.owner = transform;

                if (shots > Machine.Get<int>("shotCount"))
                {
                    Machine.SetTrigger("ShootDone");
                    return;
                }

                nextShot = Machine.Get<float>("shotDelay");
            }
        }
        
        #endregion
        
        private void Update()
        {
            stateMachine.Tick();
        }

        public void OnAnimationEvent(string id)
        {
            stateMachine.OnEvent(AiStateMachine.EventType.Animation, id);
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
            public Radius dashDistance = new Radius(5F, true);
            public float dashDelay = 1F;
            public float dashSpeed = 10F;
            public Radius maintainDistance = new Radius(20F, true);

            [Header("Shooting")]
            public int shotCount = 5;
            public float shotDelay = .25F;
            public int shotDamage = 1;
            
            [Header("Chasing")]
            public Radius chaseDashDistance = new Radius(15F, true);
            public float chaseDashDelay = .1F;
            public float chaseDashSpeed = 20F;
            public int shootsBeforeChase = 3;

            [Header("Attacking")]
            [Range(0F, 1F)]
            public float secondaryAttackChance = .5F;
            public float attackDamage = 5F;
            public Radius attackRange;

            [Header("Anchors")]
            public Transform leftGunAnchor;
            public Transform rightGunAnchor;
            
            [Header("Templates")]
            public GameObject bulletTemplate;
        }

        private class HarrierShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}