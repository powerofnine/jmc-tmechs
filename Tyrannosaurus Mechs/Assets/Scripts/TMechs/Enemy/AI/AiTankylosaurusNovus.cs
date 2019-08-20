using System;
using Animancer;
using TMechs.Animation;
using TMechs.Attributes;
using TMechs.Entity;
using TMechs.Environment;
using TMechs.Environment.Targets;
using TMechs.FX;
using TMechs.Player;
using TMechs.Player.Modules;
using TMechs.Types;
using TMechs.UI;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Enemy.AI
{
    public class AiTankylosaurusNovus : MonoBehaviour, EntityHealth.IDeath
    {
        public AiStateMachine stateMachine;
        public TankyloProperties properties = new TankyloProperties();

        public EntityHealth.DamageSource damageSource;
        
        [Header("Animations")]
        [AnimationCollection.ValidateAttribute(typeof(TankyloAnimation))]
        public AnimationCollection animations;
        public AvatarMask leftMask;
        public AvatarMask rightMask;

        [Header("Anchors")]
        public Transform shellBone;
        public Transform shotgunOrigin;

        [Header("VFX")]
        public VisualEffect moveVfx;
        public VisualEffect[] shotgunVfx = { };
        public VisualEffect[] shotgunBlast;
        public SetShaderProperty deathLights;
        public VisualEffectAsset digEffect;
        public Transform digAnchor;
        public float digDuration;
        public VisualEffect attackTrail;

        [Header("Enrage")]
        public GameObject enrageAnimationPreset;
        public SetShaderProperty enrageLights;
        
        private CharacterController controller;
        private Vector3 motion;
        private float yVelocity;

        private float moveAlpha;
        private float moveAlphaVelocity;

        private Vector3 startPosition;
        private Quaternion startOrientation;

        private void Start()
        {
            startPosition = transform.position;
            startOrientation = transform.rotation;
            
            controller = GetComponent<CharacterController>();

            TankyloShared shared = new TankyloShared()
            {
                    parent = this,
                    animancer = GetComponentInChildren<EventfulAnimancerComponent>(),
                    health = GetComponent<EntityHealth>()
            };

            shared.animancer.onEvent = new AnimationEventReceiver(null, OnAnimationEvent);

            CreateStateMachine(shared);
        }

        private void CreateStateMachine(TankyloShared shared)
        {
            stateMachine = new AiStateMachine(transform)
            {
                    target = Player.Player.Instance.centerOfMass,
                    shared = shared
            };

            stateMachine.ImportProperties(properties);

            // States
            stateMachine.RegisterState(new Primer(), "Primer");

            stateMachine.RegisterState(new Chase(), "Chase");
            stateMachine.RegisterState(new RockThrow(), "Rock Throw");

            stateMachine.RegisterState(new Standby(), "Standby");
            stateMachine.RegisterState(new Shotgun(), "Shotgun");
            stateMachine.RegisterState(new TailWhip(), "Tail Whip");

            stateMachine.RegisterState(new EnterRage(), "Enter Rage");

            // Transitions
            stateMachine.RegisterTransition("Primer", "Chase", machine => machine.GetTrigger("PrimerDone"));

            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Enter Rage", machine => !machine.Get("rage", false) && shared.health.Health <= .25F, machine => machine.Set("rage", true));
            stateMachine.RegisterTransition("Enter Rage", "Chase", machine => machine.GetTrigger("RageDone"));

            stateMachine.RegisterTransition("Chase", "Standby", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(TankyloProperties.midStopRange)));
            stateMachine.RegisterTransition("Standby", "Chase", machine => machine.HorizontalDistanceToTarget > machine.Get<Radius>(nameof(TankyloProperties.midRange)));

            stateMachine.RegisterTransition("Chase", "Rock Throw", machine => machine.GetAddSet("RockThrowTimer", -Time.deltaTime, machine.Get<Vector2>(nameof(TankyloProperties.rockThrowCooldown)).Random()) <= 0F, machine => machine.Set("RockThrowTimer", machine.Get<Vector2>(nameof(TankyloProperties.rockThrowCooldown)).Random()));
            stateMachine.RegisterTransition("Rock Throw", "Chase", machine => machine.GetTrigger("RockThrowDone"));

            stateMachine.RegisterTransition("Standby", "Shotgun", machine => machine.HorizontalDistanceToTarget <= machine.Get<Radius>(nameof(TankyloProperties.shortRange)) && machine.GetTrigger("Oriented"));
            stateMachine.RegisterTransition("Shotgun", "Standby", machine => machine.GetTrigger("ShotgunDone"));

            stateMachine.RegisterTransition("Standby", "Tail Whip", machine => machine.HorizontalDistanceToTarget >= machine.Get<Radius>(nameof(TankyloProperties.shortRange)) && machine.GetAddSet("TailWhipTimer", -Time.deltaTime, 1F) <= 0F, machine => machine.Set("TailWhipTimer", 1F));
            stateMachine.RegisterTransition("Tail Whip", "Standby", machine => machine.GetTrigger("TailWhipDone"));

            // State Machine
            stateMachine.SetDefaultState("Primer");
            stateMachine.RegisterVisualizer($"Tankylosaurus:{name}");
        }

        private class TankyloState : AiStateMachine.State
        {
            protected TankyloShared shared;
            protected AnimancerComponent Animancer => shared.animancer;

            public override void OnInit()
            {
                base.OnInit();

                shared = (TankyloShared) Machine.shared;
            }

            protected AnimationClip GetClip(TankyloAnimation clip)
            {
                if (!shared.parent.animations)
                    return null;

                return shared.parent.animations.GetClip(clip);
            }
        }

        private class Primer : TankyloState
        {
            public override void OnEnter()
            {
                base.OnEnter();


                AnimancerState state = Animancer.Play(GetClip(TankyloAnimation.Primer));

                state.OnEnd = () =>
                {
                    state.Stop();
                    Machine.SetTrigger("PrimerDone");
                };
            }
        }

        private class EnterRage : TankyloState
        {
            public override void OnEnter()
            {
                base.OnEnter();

                if(!shared.parent.enrageAnimationPreset)
                {
                    Machine.SetTrigger("RageDone");
                    return;
                }

                GameObject go = Instantiate(shared.parent.enrageAnimationPreset);
                CharacterIntroUtils utils = go.GetComponent<CharacterIntroUtils>();
                if (utils)
                    utils.onIntroDone = () => Machine.SetTrigger("RageDone");

                SetShaderProperty ssp = go.GetComponent<SetShaderProperty>();
                if(ssp)
                    ssp.Signal();
                
                if(shared.parent.enrageLights)
                    shared.parent.enrageLights.Signal();
                
                MenuActions.SetPause(true, false);
                MenuActions.pauseLocked = true;

                shared.parent.controller.enabled = false;
                transform.position = shared.parent.startPosition;
                transform.rotation = shared.parent.startOrientation;
                shared.parent.controller.enabled = true;

                if (shared.parent.attackTrail)
                {
                    shared.parent.attackTrail.gameObject.SetActive(true);
                    shared.parent.attackTrail.Play();
                }

                //TODO janky hack mate
                FindObjectOfType<CharacterIntroTrigger>().TeleportPlayer();
            }

            public override void OnExit()
            {
                base.OnExit();
                
                if (shared.parent.attackTrail)
                    shared.parent.attackTrail.Stop();
            }
        }

        private class Chase : TankyloState
        {
            private float rotateVelocity;

            private LinearMixerState leftTread;
            private LinearMixerState rightTread;

            private Vector3 shellVelocity;
            
            public override void OnInit()
            {
                base.OnInit();

                AnimancerLayer leftLayer = Animancer.GetLayer(2);
                AnimancerLayer rightLayer = Animancer.GetLayer(3);

                leftLayer.SetWeight(1F);
                rightLayer.SetWeight(1F);

                leftLayer.SetName("Left Tread");
                rightLayer.SetName("Right Tread");

                leftLayer.SetMask(shared.parent.leftMask);
                rightLayer.SetMask(shared.parent.rightMask);

                leftTread = new LinearMixerState(leftLayer);
                rightTread = new LinearMixerState(rightLayer);

                leftTread.Initialise(GetClip(TankyloAnimation.Backward), GetClip(TankyloAnimation.Still), GetClip(TankyloAnimation.Forward));
                rightTread.Initialise(GetClip(TankyloAnimation.Backward), GetClip(TankyloAnimation.Still), GetClip(TankyloAnimation.Forward));

                leftTread.Play();
                rightTread.Play();
            }

            public override void OnExit()
            {
                base.OnExit();

                leftTread.Parameter = 0F;
                rightTread.Parameter = 0F;
            }

            public override void OnTick()
            {
                base.OnTick();

                float neededY = Quaternion.LookRotation(HorizontalDirectionToTarget, Vector3.up).eulerAngles.y;

                transform.eulerAngles = transform.eulerAngles.Set(Mathf.SmoothDampAngle(transform.eulerAngles.y, neededY, ref rotateVelocity, .5F, Machine.Get<float>(nameof(TankyloProperties.rotateSpeed))), Utility.Axis.Y);

                float angle = neededY - transform.eulerAngles.y;
                float direction = Mathf.Clamp(angle, -1F, 1F);

                leftTread.Parameter = Mathf.Abs(direction) < Mathf.Epsilon ? 1F : -Mathf.Sign(direction);
                rightTread.Parameter = Mathf.Abs(direction) < Mathf.Epsilon ? 1F : Mathf.Sign(direction);

                if (Mathf.Abs(angle) < 10F)
                {
                    if (HorizontalDistanceToTarget <= Machine.Get<Radius>(nameof(TankyloProperties.midStopRange)))
                    {
                        leftTread.Parameter = 0F;
                        rightTread.Parameter = 0F;
                        return;
                    }

                    shared.parent.motion = transform.forward * Machine.Get<float>(nameof(TankyloProperties.chaseSpeed));
                }
            }

            public override void LateTick()
            {
                base.LateTick();

                shared.parent.shellBone.right = -HorizontalDirectionToTarget;
            }
        }

        private class Standby : TankyloState
        {
            private float timer;
            private bool oriented;

            public override void OnInit()
            {
                base.OnInit();

                backgroundState = Machine.FindState("Chase");
            }

            public override void OnEnter()
            {
                base.OnEnter();

                Machine.SetTrigger("Oriented", false);
                timer = 0F;
                oriented = false;
            }

            public override void OnTick()
            {
                base.OnTick();

                if (oriented)
                    return;

                if (HorizontalDistanceToTarget <= Machine.Get<Radius>(nameof(TankyloProperties.shortRange)))
                {
                    timer += Time.deltaTime;

                    if (timer > 1F)
                    {
                        Machine.SetTrigger("Oriented");
                        oriented = true;
                    }
                }
                else
                {
                    timer = 0F;
                }
            }
        }

        private class RockThrow : TankyloState
        {
            private AnimancerState rockThrow;
            private Transform anchor;

            private ThrowableContainer rock;

            public override void OnInit()
            {
                base.OnInit();

                backgroundState = Machine.FindState("Chase");
                rockThrow = Animancer.GetOrCreateState(GetClip(TankyloAnimation.RockThrow), 1);

                anchor = Machine.Get<Transform>(nameof(TankyloProperties.rockAnchor));
            }

            public override void OnEnter()
            {
                base.OnEnter();

                Animancer.Play(rockThrow);

                rockThrow.OnEnd = () =>
                {
                    rockThrow.Stop();
                    Machine.SetTrigger("RockThrowDone");
                };
            }

            public override void OnExit()
            {
                base.OnExit();
                
                if (shared.parent.attackTrail)
                    shared.parent.attackTrail.Stop();
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                switch (id)
                {
                    case "RockReady":
                        GameObject go = Instantiate(Machine.Get<GameObject>(nameof(TankyloProperties.rockTemplate)), anchor, false);

                        GameObject container = new GameObject("Tankylo Rock");
                        rock = container.AddComponent<ThrowableContainer>();

                        rock.enemyRock = true;
                        rock.recepientDamage = Machine.Get<float>(nameof(TankyloProperties.rockDamage));
                        rock.Initialize(go);

                        rock.transform.SetParent(anchor, true);
                        rock.transform.position = anchor.position;

                        VfxModule.SpawnEffect(shared.parent.digEffect, shared.parent.digAnchor ? shared.parent.digAnchor.position : transform.position, Quaternion.identity, shared.parent.digDuration);

                        if (shared.parent.attackTrail)
                        {
                            shared.parent.attackTrail.gameObject.SetActive(true);
                            shared.parent.attackTrail.Play();
                        }
                        
                        break;
                    case "RockThrow":
                        if (!rock)
                            return;

                        rock.transform.SetParent(null, true);

                        if (shared.parent.attackTrail)
                            shared.parent.attackTrail.Stop();
                        
                        // Very naive trajectory projection, but it works surprisingly well
                        rock.Throw(target.position + Player.Player.Instance.forces.ControllerVelocity * .75F, Machine.Get<float>(nameof(TankyloProperties.rockInAngle)), Machine.Get<float>(nameof(TankyloProperties.rockOutAngle)), Machine.Get<float>(nameof(TankyloProperties.rockSpeed)));

                        break;
                }
            }
        }

        private class Shotgun : TankyloState
        {
            private AnimancerState shotgun;

            private Vector3 shellDirection;

            public override void OnInit()
            {
                base.OnInit();

                shotgun = Animancer.GetOrCreateState(GetClip(TankyloAnimation.Shotgun), 1);
            }

            public override void OnEnter()
            {
                base.OnEnter();

                Animancer.CrossFadeFromStart(shotgun, .1F).OnEnd = () =>
                {
                    shotgun.Stop();
                    Machine.SetTrigger("ShotgunDone");
                };

                foreach (VisualEffect vfx in shared.parent.shotgunVfx)
                {
                    vfx.gameObject.SetActive(true);
                    vfx.Play();
                }
                
                shellDirection = -HorizontalDirectionToTarget;
            }

            public override void LateTick()
            {
                base.LateTick();

                shared.parent.shellBone.right = shellDirection;
            }

            public override void OnExit()
            {
                base.OnExit();

                foreach (VisualEffect vfx in shared.parent.shotgunVfx)
                    vfx.Stop();
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                if ("ShotgunBlast".Equals(id))
                {
                    foreach (VisualEffect vfx in shared.parent.shotgunVfx)
                        vfx.Stop();
                    foreach (VisualEffect vfx in shared.parent.shotgunBlast)
                    {
                        vfx.gameObject.SetActive(true);
                        vfx.Play();
                    }

                    Transform origin = shared.parent.shotgunOrigin ? shared.parent.shotgunOrigin : transform;
                    float distance = Vector3.Distance(origin.position, target.position);
                    float angle = Vector3.Angle((target.position - origin.position).Remove(Utility.Axis.Y).normalized, transform.forward.Remove(Utility.Axis.Y).normalized);
                    
                    if (distance <= Machine.Get<Radius>(nameof(TankyloProperties.midRange)) && angle <= 50F)
                    {
                        Player.Player.Instance.Health.Damage(Machine.Get<float>(nameof(TankyloProperties.shotgunDamage)), shared.parent.damageSource.GetWithSource(transform));
                        Player.Player.Instance.forces.frictionedVelocity = HorizontalDirectionToTarget * Machine.Get<float>(nameof(TankyloProperties.shotgunKnockback));
                    }
                }
            }
        }

        private class TailWhip : TankyloState
        {
            private AnimancerState tailWhip;

            private EnemyHitBox[] colliders;
            
            public override void OnInit()
            {
                base.OnInit();

                tailWhip = Animancer.GetOrCreateState(GetClip(TankyloAnimation.TailWhip), 1);
                colliders = Machine.Get<EnemyHitBox[]>(nameof(TankyloProperties.tailWhipColliders));
                foreach (EnemyHitBox box in colliders)
                {
                    box.gameObject.SetActive(false);
                    box.damage = 0F;

                    box.callback = () => SetBoxes(false);
                }
            }

            public override void OnEnter()
            {
                base.OnEnter();

                Animancer.CrossFadeFromStart(tailWhip, .1F).OnEnd = () =>
                {
                    tailWhip.Stop();
                    Machine.SetTrigger("TailWhipDone");
                };
                
                if (shared.parent.attackTrail)
                {
                    shared.parent.attackTrail.gameObject.SetActive(true);
                    shared.parent.attackTrail.Play();
                }
            }

            public override void OnTick()
            {
                base.OnTick();

                if (HorizontalDistanceToTarget * 1.2F >= Machine.Get<Radius>(nameof(TankyloProperties.midStopRange)))
                    shared.parent.motion = Machine.Get<float>(nameof(TankyloProperties.chaseSpeed)) * .5F * HorizontalDirectionToTarget;
            }

            public override void OnExit()
            {
                base.OnExit();

                foreach (EnemyHitBox box in colliders)
                {
                    box.gameObject.SetActive(false);
                    box.damage = 0F;
                }
                
                if (shared.parent.attackTrail)
                    shared.parent.attackTrail.Stop();
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                switch (id)
                {
                    case "WhipActive":
                        SetBoxes(true);

                        break;
                    case "WhipInactive":
                        SetBoxes(false);

                        break;
                }
            }

            private void SetBoxes(bool active)
            {
                float damage = Machine.Get<float>(nameof(TankyloProperties.tailWhipDamage));
                
                foreach (EnemyHitBox box in colliders)
                {
                    box.gameObject.SetActive(active);
                    box.damage = active ? damage : 0F; 
                }
            }
        }

    private void Update()
        {
            stateMachine.Tick();
            
            yVelocity += Utility.GRAVITY * Time.deltaTime;
            controller.Move((motion + Vector3.down * yVelocity) * Time.deltaTime);
            motion = Vector3.zero;

            float desiredAlpha = controller.velocity.sqrMagnitude > 3F ? 1F : 0F;
            moveAlpha = Mathf.SmoothDamp(moveAlpha, desiredAlpha, ref moveAlphaVelocity, .25F);
            moveVfx.SetFloat("Alpha", moveAlpha);

            if (controller.isGrounded)
                yVelocity = 0F;
        }

        private void LateUpdate()
        {
            stateMachine.LateTick();
        }

        public void OnDying(ref bool customDestroy)
        {
            customDestroy = true;
            
            stateMachine.Exit();

            Destroy(((TankyloShared) stateMachine.shared).health);
            Destroy(this);
            Destroy(GetComponentInChildren<EnemyTarget>());

            ((TankyloShared) stateMachine.shared).animancer.Stop();
            ((TankyloShared) stateMachine.shared).animancer.Play(animations.GetClip(TankyloAnimation.Death)).Time = 0;

            if (deathLights)
                deathLights.Signal();
        }

        private void OnAnimationEvent(AnimationEvent e)
        {
            stateMachine.OnEvent(AiStateMachine.EventType.Animation, e.stringParameter);
        }

        private class TankyloShared
        {
            public AiTankylosaurusNovus parent;
            public EventfulAnimancerComponent animancer;
            public EntityHealth health;
        }

        [Serializable]
        public class TankyloProperties
        {
            [Header("Chase")]
            public Radius shortRange = new Radius(10F);
            public Radius midRange = new Radius(20F);
            public Radius midStopRange = new Radius(15F);
            public float rotateSpeed = 35F;
            public float chaseSpeed = 30F;

            [Header("Rock Throw")]
            [MinMax]
            public Vector2 rockThrowCooldown = new Vector2(2F, 5F);
            public GameObject rockTemplate;
            public Transform rockAnchor;
            public float rockSpeed = 100F;
            public float rockInAngle = 0F;
            public float rockOutAngle = 20F;
            public float rockDamage = 20F;

            [Header("Shotgun")]
            public float shotgunDamage = 25F;
            public float shotgunKnockback = 10F;

            [Header("Tail Whip")]
            public EnemyHitBox[] tailWhipColliders = {};
            public float tailWhipDamage = 10F;
        }

        [AnimationCollection.EnumAttribute("Tankylosaurus Animations")]
        public enum TankyloAnimation
        {
            Primer,
            Death,

            [Header("Movement")]
            Forward,
            Backward,
            Still,

            [Header("Attacks")]
            RockThrow,
            TailWhip,
            Shotgun
        }
    }
}