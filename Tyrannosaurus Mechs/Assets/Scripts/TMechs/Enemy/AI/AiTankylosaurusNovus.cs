using System;
using Animancer;
using TMechs.Animation;
using TMechs.Attributes;
using TMechs.Entity;
using TMechs.Player;
using TMechs.Types;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Enemy.AI
{
    public class AiTankylosaurusNovus : MonoBehaviour, EntityHealth.IDeath
    {
        public AiStateMachine stateMachine;
        public TankyloProperties properties = new TankyloProperties();

        [Header("Animations")]
        [AnimationCollection.ValidateAttribute(typeof(TankyloAnimation))]
        public AnimationCollection animations;
        public AvatarMask leftMask;
        public AvatarMask rightMask;

        [Header("Anchors")]
        public Transform shellBone;

        [Header("VFX")]
        public VisualEffect moveVfx;
        public VisualEffect[] shotgunVfx = { };
        public VisualEffect[] shotgunBlast;

        private CharacterController controller;
        private Vector3 motion;
        private float yVelocity;

        private float moveAlpha;
        private float moveAlphaVelocity;
        
        private void Start()
        {
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
            stateMachine.RegisterState(null, "Tail Whip");

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

//            stateMachine.RegisterTransition("Standby", "Tail Whip", machine => machine.GetAddSet("TailWhipTimer", -Time.deltaTime, 2F) <= 0F, machine => machine.Set("TailWhipTimer", 2F));
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

            public AnimationClip GetClip(TankyloAnimation clip)
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

                // TODO: fanfare
                Machine.SetTrigger("RageDone");
            }
        }

        private class Chase : TankyloState
        {
            private float rotateVelocity;

            private LinearMixerState leftTread;
            private LinearMixerState rightTread;

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

                    if (timer > 2F)
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

                        break;
                    case "RockThrow":
                        if (!rock)
                            return;

                        rock.transform.SetParent(null, true);

                        // Very naive trajectory projection, but it works surprisingly well
                        rock.Throw(target.position + Player.Player.Instance.forces.ControllerVelocity * .75F, Machine.Get<float>(nameof(TankyloProperties.rockInAngle)), Machine.Get<float>(nameof(TankyloProperties.rockOutAngle)), Machine.Get<float>(nameof(TankyloProperties.rockSpeed)));

                        break;
                }
            }
        }

        private class Shotgun : TankyloState
        {
            private AnimancerState shotgun;

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

            Destroy(((TankyloShared) stateMachine.shared).animancer);
            Destroy(((TankyloShared) stateMachine.shared).health);
            Destroy(this);
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