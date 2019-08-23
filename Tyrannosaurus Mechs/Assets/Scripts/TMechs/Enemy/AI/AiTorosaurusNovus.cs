using System;
using System.Collections;
using Animancer;
using TMechs.Animation;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.Player.Behavior;
using TMechs.Types;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.VFX;
using Random = UnityEngine.Random;

namespace TMechs.Enemy.AI
{
    public class AiTorosaurusNovus : MonoBehaviour, EntityHealth.IDamage, EntityHealth.IDeath
    {
        private static readonly int COLOR = Shader.PropertyToID("_Color");

        public AiStateMachine stateMachine;
        [AnimationCollection.ValidateAttribute(typeof(ToroAnimation))]
        public AnimationCollection animations;
        public ToroProperties properties;
        public new ToroAudio audio;

        public EntityHealth.DamageSource damageSource;

        [Header("VFX")]
        public VisualEffect[] vfxChargeIntro = { };
        public VisualEffect[] vfxCharging = { };
        public VisualEffect[] vfxChargeCooldown = { };
        public VisualEffect vfxChargeFoot;
        public VisualEffect vfxHornTaser;

        private Vector3 unitPosition;

        private AnimancerState hurt;
        private bool isDead;
        private bool lazyMode;

        private void Start()
        {
            ToroShared shared = new ToroShared
            {
                    parent = this,
                    animancer = GetComponentInChildren<EventfulAnimancerComponent>(),
                    agent = GetComponent<NavMeshAgent>(),
                    controller = GetComponent<CharacterController>(),
                    damageSource = damageSource
            };
            shared.controller.enabled = false;

            shared.animancer.onEvent = new AnimationEventReceiver(null, OnAnimatorEvent);
            hurt = shared.animancer.GetOrCreateState(animations.GetClip(ToroAnimation.TakeDamage), 3);

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
            stateMachine.RegisterState(new Notice(), "Notice");
            stateMachine.RegisterState(new Chasing(), "Chasing");
            stateMachine.RegisterState(new Charge(), "Charge");
            stateMachine.RegisterState(new Standby(), "Standby");
            stateMachine.RegisterState(new Thrash(), "Thrash");

            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle", machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>(nameof(ToroProperties.rangeStopFollow)), machine =>
            {
                StartCoroutine(FadeNightrider(false)); 
                if(vfxHornTaser)
                    vfxHornTaser.Stop();
                if(audio.taser)
                    audio.taser.Stop();
            });
            stateMachine.RegisterTransition("Idle", "Notice", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(ToroProperties.rangeStartFollow)));

            stateMachine.RegisterTransition("Chasing", "Charge", machine => machine.GetAddSet<float>("ChargeTimer", -Time.deltaTime) <= 0F, machine => machine.Set("ChargeTimer", machine.Get<float>(nameof(ToroProperties.chargeCooldown))));

            stateMachine.RegisterTransition("Chasing", "Standby", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(ToroProperties.stoppingRange)));
            stateMachine.RegisterTransition("Standby", "Chasing", machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>(nameof(ToroProperties.attackRange)));

            stateMachine.RegisterTransition("Standby", "Thrash", machine => machine.GetAddSet("AttackTimer", -Time.deltaTime, machine.Get<float>(nameof(ToroProperties.attackCooldown))) < 0F, machine => machine.Set("AttackTimer", machine.Get<float>(nameof(ToroProperties.attackCooldown))));

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
            protected ToroAudio Audio => shared.parent.audio;

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

                wander.StartFade(0F, .15F);
                Animancer.GetLayer(1).StartFade(0F, .15F);
            }

            public override void OnTick()
            {
                base.OnTick();

                if (isWandering)
                {
                    if (!wander.IsPlaying)
                        wander.Play();
                    Animancer.GetLayer(0).SetWeight(Mathf.Clamp01(shared.agent.velocity.magnitude / Machine.Get<float>(nameof(ToroProperties.wanderSpeed))));

                    if (shared.agent.pathPending)
                        return;

                    if (shared.agent.remainingDistance <= Mathf.Epsilon || shared.agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        isWandering = false;

                        if (Random.Range(0, 100) < 15)
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

        private class Notice : ToroState
        {
            private AnimancerState primer;

            public override void OnInit()
            {
                base.OnInit();

                primer = Animancer.GetOrCreateState(GetClip(ToroAnimation.Primer));
                primer.Speed *= 3F;
            }

            public override void OnEnter()
            {
                base.OnEnter();

                Animancer.CrossFadeFromStart(primer, .15F).OnEnd = () =>
                {
                    primer.OnEnd = null;
                    Machine.EnterState("Chasing");
                };

                shared.parent.StartCoroutine(shared.parent.FadeNightrider(true));
                if (shared.parent.vfxHornTaser)
                {
                    shared.parent.vfxHornTaser.gameObject.SetActive(true);
                    shared.parent.vfxHornTaser.Play();
                }
                if(Audio.taser)
                    Audio.taser.Play();
            }

            public override void OnExit()
            {
                base.OnExit();

                primer.Stop();
            }

            public override void OnTick()
            {
                base.OnTick();

                Face(HorizontalDirectionToTarget, .075F);
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

                run.StartFade(0F, .15F);
            }

            public override void OnTick()
            {
                base.OnTick();

                run.Weight = Mathf.Clamp01(shared.agent.velocity.magnitude / Machine.Get<float>(nameof(ToroProperties.chaseSpeed)));
                shared.agent.SetDestination(target.position);
            }
        }

        private class Charge : ToroState
        {
            private AnimancerState chargeIntro;
            private AnimancerState chargeLoop;

            private bool isCharging;

            private float gravity;

            private Vector3 targetPoint;

            private float lockoutTimer;

            public override void OnInit()
            {
                base.OnInit();

                chargeIntro = Animancer.GetOrCreateState(GetClip(ToroAnimation.ChargeIntro));
                chargeLoop = Animancer.GetOrCreateState(GetClip(ToroAnimation.Charge));
            }

            public override void OnEnter()
            {
                base.OnEnter();

                shared.agent.isStopped = true;
                shared.controller.enabled = true;

                isCharging = false;
                gravity = 0F;

                transform.forward = HorizontalDirectionToTarget;
                targetPoint = target.transform.position.Remove(Utility.Axis.Y);
                
                lockoutTimer = 0F;

                Animancer.CrossFadeFromStart(chargeIntro, .15F).OnEnd = () =>
                {
                    chargeIntro.Stop();
                    Animancer.Play(chargeLoop).Time = 0F;

                    isCharging = true;

                    foreach (VisualEffect vfx in shared.parent.vfxChargeIntro)
                        vfx.Stop();
                    foreach (VisualEffect vfx in shared.parent.vfxCharging)
                    {
                        vfx.gameObject.SetActive(true);
                        vfx.Play();
                    }
                };

                foreach (VisualEffect vfx in shared.parent.vfxChargeIntro)
                {
                    vfx.gameObject.SetActive(true);
                    vfx.Play();
                }
                if(Audio.chargeUp)
                    Audio.chargeUp.Play();
            }

            public override void OnExit()
            {
                base.OnExit();

                shared.controller.enabled = false;

                chargeIntro.Stop();
                chargeLoop.Stop();

                foreach (VisualEffect vfx in shared.parent.vfxCharging)
                    vfx.Stop();
                foreach (VisualEffect vfx in shared.parent.vfxChargeCooldown)
                {
                    vfx.gameObject.SetActive(true);
                    vfx.Play();
                }
                
                if(Audio.jet)
                    Audio.jet.Stop();
            }

            public override void OnTick()
            {
                base.OnTick();

                gravity += Utility.GRAVITY * Time.deltaTime;

                Vector3 movement = Vector3.zero;

                if (isCharging)
                    movement = transform.forward * Machine.Get<float>(nameof(ToroProperties.chargeSpeed));

                shared.controller.Move((movement + Vector3.down * gravity) * Time.deltaTime);

                if (shared.controller.isGrounded)
                    gravity = 0F;

                if (isCharging)
                {
                    lockoutTimer += Time.deltaTime;
                    if (Vector3.Distance(transform.position.Remove(Utility.Axis.Y), targetPoint) <= Machine.Get<Radius>(nameof(ToroProperties.stoppingRange)) ||
                        HorizontalDistanceToTarget <= Machine.Get<Radius>(nameof(ToroProperties.stoppingRange)) ||
                        lockoutTimer > 4F)
                    {
                        if (HorizontalDistanceToTarget <= Machine.Get<Radius>(nameof(ToroProperties.attackRange)) && AngleToTarget <= 35F)
                            Player.Player.Instance.Health.Damage(Machine.Get<float>(nameof(ToroProperties.chargeDamage)), shared.damageSource.GetWithSource(transform));

                        Machine.EnterState("Chasing");
                    }
                }
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                switch (id)
                {
                    case "charge":
                        isCharging = true;
                        if(Audio.jet)
                            Audio.jet.Play();
                        break;
                    case "footFx":
                        if (shared.parent.vfxChargeFoot)
                        {
                            shared.parent.vfxChargeFoot.gameObject.SetActive(true);
                            shared.parent.vfxChargeFoot.Play();
                        }

                        break;
                }
            }
        }

        private class Standby : ToroState
        {
            public override void OnTick()
            {
                base.OnTick();

                Face(DirectionToTarget);
            }
        }

        private class Thrash : ToroState
        {
            private AnimancerState thrash;

            public override void OnInit()
            {
                base.OnInit();

                thrash = Animancer.GetOrCreateState(GetClip(ToroAnimation.Thrash));
            }

            public override void OnEnter()
            {
                base.OnEnter();

                Animancer.CrossFadeFromStart(thrash, 0.1F);
            }

            public override void OnExit()
            {
                base.OnExit();

                thrash.StartFade(0F, .15F);
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                switch (id)
                {
                    case "attack":
                        if (HorizontalDistanceToTarget <= Machine.Get<Radius>(nameof(ToroProperties.attackRange)) && AngleToTarget <= 45F)
                            Player.Player.Instance.Health.Damage(Machine.Get<float>(nameof(ToroProperties.attackDamage)), shared.damageSource.GetWithSource(transform));

                        break;
                    case "attackDone":
                        Machine.EnterState("Standby");
                        break;
                }
            }
        }

        #endregion

        private void Update()
        {
            lazyMode = stateMachine.DistanceToTarget >= 200F;

            if (!lazyMode && !isDead)
            {
                stateMachine.Tick();

                if (stateMachine.HasValue("ChargeTimer") && stateMachine.Get<float>("ChargeTimer") <= 0F)
                    foreach (VisualEffect vfx in vfxChargeCooldown)
                        vfx.Stop();
            }
        }

        private void LateUpdate()
        {
            if (!lazyMode && !isDead)
                stateMachine.LateTick();
        }

        private void OnAnimatorEvent(AnimationEvent e)
        {
            if ("Footstep".Equals(e.stringParameter) && audio.walk)
            {
                audio.walk.RandyPitchford();
                audio.walk.Play();
            }
                
            
            if (!lazyMode && !isDead)
                stateMachine.OnEvent(AiStateMachine.EventType.Animation, e.stringParameter);
        }

        public void OnDamaged(EntityHealth health, ref bool cancel)
        {
            if (lazyMode || isDead)
                return;
            if ("Idle".Equals(stateMachine.CurrentStateName))
                stateMachine.EnterState("Notice");

            ((ToroShared) stateMachine.shared).animancer.CrossFadeFromStart(hurt, .1F).OnEnd = () =>
            {
                hurt.OnEnd = null;
                hurt.StartFade(0F, .1F);
            };
        }

        public void OnDying(EntityHealth.DamageSource source, ref bool customDestroy)
        {
            if (isDead)
                return;

            customDestroy = true;
            isDead = true;

            if(audio.idle)
                audio.idle.Stop();
            
            stateMachine.Exit();
            StopAllCoroutines();

            ToroShared shared = (ToroShared) stateMachine.shared;

            Destroy(shared.agent);
            Destroy(shared.controller);
            Destroy(GetComponent<EnemyTarget>());

            if(vfxHornTaser)
                vfxHornTaser.Stop();
            StartCoroutine(FadeNightrider(Color.black));
            AnimancerState state = shared.animancer.CrossFadeFromStart(animations.GetClip(ToroAnimation.Death), .1F, 3);
            state.OnEnd = () =>
            {
                state.OnEnd = null;
                Destroy(gameObject, .5F);
            };
        }

        private IEnumerator FadeNightrider(bool aggressive)
        {
            yield return FadeNightrider(aggressive ? properties.nightriderAggressive : properties.nightriderPassive);
        }

        private IEnumerator FadeNightrider(Color destColor)
        {
            if (!properties.nightriderDisplay)
                yield break;

            float time = 0F;
            Color sourceColor = properties.nightriderDisplay.material.GetColor(COLOR);

            while (time <= properties.nightriderFadeTime)
            {
                time += Time.deltaTime;
                properties.nightriderDisplay.material.SetColor(COLOR, Color.Lerp(sourceColor, destColor, time / properties.nightriderFadeTime));

                yield return null;
            }
        }

        private class ToroShared
        {
            public AiTorosaurusNovus parent;
            public EventfulAnimancerComponent animancer;
            public NavMeshAgent agent;
            public CharacterController controller;
            public EntityHealth.DamageSource damageSource;
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
            public float chargeSpeed = 45F;
            public float chargeDamage = 10F;

            [Header("Attack")]
            public float attackCooldown = 1F;
            public float attackDamage = 5F;

            [Header("Nightrider")]
            public Renderer nightriderDisplay;
            public Color nightriderPassive = Color.green;
            public Color nightriderAggressive = Color.red;
            public float nightriderFadeTime = 1F;
        }

        [Serializable]
        public struct ToroAudio
        {
            public AudioSource idle;
            public AudioSource walk;
            public AudioSource taser;
            public AudioSource chargeUp;
            public AudioSource jet;
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