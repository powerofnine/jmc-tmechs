using System;
using Animancer;
using TMechs.Animation;
using TMechs.Entity;
using TMechs.Types;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TMechs.Enemy.AI
{
    public class AiTorosaurusNovus : MonoBehaviour, EntityHealth.IDamage
    {
        public AiStateMachine stateMachine;
        public ToroProperties properties;
        [AnimationCollection.ValidateAttribute(typeof(ToroAnimation))]
        public AnimationCollection animations;

        private Vector3 unitPosition;
        
        private void Start()
        {
            ToroShared shared = new ToroShared()
            {
                parent = this,
                animancer = GetComponentInChildren<EventfulAnimancerComponent>(),
                agent = GetComponent<NavMeshAgent>()
            };
            shared.agent.isStopped = true;
            
            shared.animancer.onEvent = new AnimationEventReceiver(null, OnAnimatorEvent);
            
            CreateStateMachine(shared);

            unitPosition = transform.position;
        }

        private void CreateStateMachine(ToroShared shared)
        {
            stateMachine = new AiStateMachine(transform)
            {
                    target = Player.Player.Instance.transform,
                    shared = shared
            };
            
            stateMachine.ImportProperties(properties);
            
            stateMachine.RegisterState(new Idle(), "Idle");
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
            protected ToroShared shared;
            protected AnimancerComponent Animancer => shared.animancer;

            private float yFaceVelocity;
            
            public override void OnInit()
            {
                base.OnInit();
                
                shared = Machine.shared as ToroShared;
            }

            protected AnimationClip GetClip(ToroAnimation clip)
            {
                return shared.parent.animations.GetClip(clip);
            }

            protected void Face(Vector3 targetDirection, float smoothTime = .15F)
            {
                targetDirection.y = 0F;

                float target = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target, ref yFaceVelocity, smoothTime);

                transform.eulerAngles = transform.eulerAngles.Set(angle, Utility.Axis.Y);
            }
        }

        private class Idle : ToroState
        {
            private AnimancerState wander;
            private AnimancerState idle1;
            private AnimancerState idle2;

            private bool isWandering;

            public override void OnInit()
            {
                base.OnInit();

                wander = Animancer.GetOrCreateState(GetClip(ToroAnimation.Wander));
                idle1 = Animancer.GetOrCreateState(GetClip(ToroAnimation.Idle1), 1);
                idle2 = Animancer.GetOrCreateState(GetClip(ToroAnimation.Idle2), 1);
            }
            
            public override void OnEnter()
            {
                base.OnEnter();

                shared.agent.isStopped = true;
                shared.agent.speed = Machine.Get<float>(nameof(ToroProperties.wanderSpeed));
                
                SetWander();
            }

            public override void OnExit()
            {
                base.OnExit();

                shared.agent.isStopped = true;
            }

            public override void OnTick()
            {
                base.OnTick();

                if (isWandering)
                {
                    if(!wander.IsPlaying)
                        wander.Play();
                    Animancer.GetLayer(0).SetWeight(Mathf.Clamp01(shared.agent.velocity.magnitude / Machine.Get<float>(nameof(ToroProperties.wanderSpeed))));

                    if (shared.agent.pathPending)
                        return;
                    
                    if (shared.agent.remainingDistance <= Mathf.Epsilon || shared.agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        isWandering = false;
                        
                        if(Random.Range(0, 100) < 15)
                            SetWander();
                        else
                        {
                            AnimancerState state = idle1;
                            if (Random.Range(0, 100) > 50)
                                state = idle2;

                            Animancer.GetLayer(0).StartFade(0F, .15F);
                            Animancer.CrossFadeFromStart(state, .15F).OnEnd = () =>
                            {
                                idle1.OnEnd = null;
                                idle2.OnEnd = null;
                                
                                SetWander();
                                Animancer.GetLayer(1).StartFade(0F, .15F);
                            };
                        }
                    }
                }
            }

            private void SetWander()
            {
                isWandering = true;

                shared.agent.isStopped = false;
                shared.agent.SetDestination(shared.parent.unitPosition + Random.insideUnitCircle.RemapXZ() * Machine.Get<Radius>(nameof(ToroProperties.wanderRange)));
            }
        }

        private class Chasing : ToroState
        {
            private AnimancerState run;
            
            public override void OnInit()
            {
                base.OnInit();

                run = Animancer.GetOrCreateState(GetClip(ToroAnimation.Run));
            }

            public override void OnEnter()
            {
                base.OnEnter();

                shared.agent.speed = Machine.Get<float>(nameof(ToroProperties.chaseSpeed));
                shared.agent.isStopped = false;
                
                Animancer.CrossFadeFromStart(run, .15F);
            }

            public override void OnExit()
            {
                base.OnExit();
                
                shared.agent.isStopped = true;
            }

            public override void OnTick()
            {
                base.OnTick();

                run.Weight = Mathf.Clamp01(shared.agent.velocity.magnitude / Machine.Get<float>(nameof(ToroProperties.chaseSpeed)));
                shared.agent.SetDestination(target.position);
            }
        }
        
        #endregion
        
        private void Update()
        {
            stateMachine.Tick();
        }

        private void LateUpdate()
        {
            stateMachine.LateTick();
        }

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
            public NavMeshAgent agent;
        }

        [Serializable]
        public class ToroProperties
        {
            [Header("Ranges")]
            public Radius rangeStartFollow;
            public Radius rangeStopFollow;
            public Radius attackRange;
            public Radius stoppingRange;

            [Header("Idle")]
            public Radius wanderRange;
            public float wanderSpeed = 8F;

            [Header("Chase")]
            public float chaseSpeed = 25F;
            
            [Header("Charge")]
            public float chargeCooldown = 4F;

            [Header("Attack")]
            public float attackCooldown = 1F;
        }

        [AnimationCollection.EnumAttribute("Horndriver Animations")]
        public enum ToroAnimation
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
