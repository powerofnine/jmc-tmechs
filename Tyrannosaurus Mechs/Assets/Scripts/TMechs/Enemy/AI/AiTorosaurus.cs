using System;
using JetBrains.Annotations;
using TMechs.Types;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMechs.Enemy.AI
{
    public class AiTorosaurus : MonoBehaviour, AnimatorEventListener.IAnimatorEvent, EnemyTrigger.ITriggerListener
    {
        public static readonly int IS_MOVING = Anim.Hash("Is Moving");
        public static readonly int IS_CHARGING = Anim.Hash("Is Charging");
        public static readonly int CHARGE_HIT = Anim.Hash("Charge Hit");

        public AiStateMachine stateMachine;

        public TorosaurusProperties properties = new TorosaurusProperties();

        private float yVelocity;

        private void Start()
        {
            CreateStateMachine(new TorosaurusShared
            {
                    animator = GetComponentInChildren<Animator>(),
                    controller = GetComponent<CharacterController>()
            });
        }

        private void CreateStateMachine(TorosaurusShared shared)
        {
            stateMachine = new AiStateMachine(transform) {target = Player.Player.Instance.transform, shared = shared};

            stateMachine.ImportProperties(properties);

            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(new Chasing(), "Chasing");
            stateMachine.RegisterState(new Charging(), "Charging");
            stateMachine.RegisterState(new Attacking(), "Attacking");

            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle",
                    machine => machine.DistanceToTarget > machine.Get<Radius>("rangeStopFollow"));
            stateMachine.RegisterTransition("Idle", "Chasing",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("rangeStartFollow"));

            stateMachine.RegisterTransition("Chasing", "Charging",
                    machine =>
                    {
                        if (machine.DistanceToTarget <= machine.Get<Radius>("chargeRange") && machine.GetAddSet<float>("chargeTimer", -Time.deltaTime) <= 0F)
                        {
                            machine.Set("chargeTimer", machine.Get<float>("chargeCooldown"));

                            return Random.Range(0, 100) <= machine.Get<float>("chargeChance") * 100F;
                        }

                        return false;
                    });
            stateMachine.RegisterTransition("Charging", "Chasing",
                    machine => machine.GetTrigger("chargeFinished"));

            stateMachine.RegisterTransition("Chasing", "Attacking",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("attackRange") && machine.GetAddSet<float>("attackTimer", -Time.deltaTime) <= 0F);
            stateMachine.RegisterTransition("Attacking", "Chasing",
                    machine => machine.GetTrigger("attackFinished"));

            stateMachine.SetDefaultState("Idle");
            stateMachine.RegisterVisualizer($"Torosaurus:{name}");
        }

        private void Update()
        {
            stateMachine.Tick();

            yVelocity += Utility.GRAVITY * Time.deltaTime;

            CharacterController controller = (stateMachine.shared as TorosaurusShared)?.controller;

            if (controller)
            {
                controller.Move(Vector3.down * yVelocity);
                if (controller.isGrounded)
                    yVelocity = 0F;
            }
        }

        public void OnAnimationEvent(string id)
            => stateMachine.OnEvent(AiStateMachine.EventType.Animation, id);

        public void OnTrigger(string id)
            => stateMachine.OnEvent(AiStateMachine.EventType.Trigger, id);

        #region States

        private abstract class TorosaurusState : AiStateMachine.State
        {
            protected TorosaurusShared shared;

            public override void OnEnter()
            {
                base.OnEnter();

                shared = Machine.shared as TorosaurusShared;
            }
        }

        private class Chasing : TorosaurusState
        {
            public override void OnEnter()
            {
                base.OnEnter();

                shared.animator.SetBool(IS_MOVING, true);
            }

            public override void OnExit()
            {
                base.OnExit();

                shared.animator.SetBool(IS_MOVING, false);
            }

            public override void OnTick()
            {
                Vector3 direction = HorizontalDirectionToTarget;
                transform.forward = direction;

                if (DistanceToTarget >= Machine.Get<Radius>("attackRange"))
                    shared.controller.Move(Machine.Get<float>("moveSpeed") * Time.deltaTime * direction);
            }
        }

        private class Charging : TorosaurusState
        {
            private bool chargeStart;
            private float distanceRemaining;
            private float timeRemaining;

            public override void OnEnter()
            {
                base.OnEnter();

                transform.forward = HorizontalDirectionToTarget;

                chargeStart = false;
                shared.animator.SetBool(IS_CHARGING, true);

                Machine.Set("chargeTimer", Machine.Get<float>("chargeCooldown"));

                distanceRemaining = Mathf.Min(Machine.Get<Radius>("chargeMaxDistance"), HorizontalDistanceToTarget + 5F);
                timeRemaining = Machine.Get<float>("chargeFallbackMaxTime");
            }

            public override void OnExit()
            {
                base.OnExit();

                shared.animator.SetBool(IS_CHARGING, false);
            }

            public override void OnTick()
            {
                if (!chargeStart)
                    return;

                Vector3 pos = transform.position;
                shared.controller.Move(Machine.Get<float>("chargeSpeed") * Time.deltaTime * transform.forward);
                
                //TODO figure out why controller.velocity returns the completely wrong value, in the meantime, calculate it ourselves
                distanceRemaining -= Vector3.Distance(pos, transform.position);
                timeRemaining -= Time.deltaTime;

                if (distanceRemaining <= 0F || timeRemaining <= 0F || shared.controller.velocity.magnitude < 1F)
                    Machine.SetTrigger("chargeFinished");
            }

            private void OnAnimation(string id)
            {
                switch (id)
                {
                    case "charge":
                        chargeStart = true;
                        break;
                    case "chargeDone":
                        Machine.SetTrigger("chargeFinished");
                        break;
                    case "chargeHit":
                        if(DistanceToTarget <= Machine.Get<Radius>("attackRange") && AngleToTarget <= 35F)
                            Player.Player.Instance.Damage(Machine.Get<int>("chargeHitDamage"));
                        break;
                }
            }

            private void OnTrigger(string id)
            {
                if ("chargeHit".Equals(id))
                {
                    chargeStart = false;
                    shared.animator.SetTrigger(CHARGE_HIT);
                }
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                switch (type)
                {
                    case AiStateMachine.EventType.Animation:
                        OnAnimation(id);
                        break;
                    case AiStateMachine.EventType.Trigger:
                        OnTrigger(id);
                        break;
                }
            }
        }

        private class Attacking : TorosaurusState
        {
            public override void OnEnter()
            {
                base.OnEnter();

                Machine.Set("attackTimer", Machine.Get<float>("attackCooldown"));
                shared.animator.SetTrigger(Anim.ATTACK);
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                switch (id)
                {
                    case "attackDone":
                        Machine.SetTrigger("attackFinished");
                        break;
                    case "attack":
                        if(DistanceToTarget <= Machine.Get<Radius>("attackRange") && AngleToTarget <= 35F)
                            Player.Player.Instance.Damage(Machine.Get<int>("attackDamage"));
                        break;
                }
            }
        }

        #endregion

        [Serializable]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public class TorosaurusProperties
        {
            [Header("Range")]
            public Radius rangeStartFollow = new Radius(25F);
            public Radius rangeStopFollow = new Radius(35F);
            public Radius chargeRange = new Radius(15F);
            public Radius attackRange = new Radius(1F);

            [Header("Chase")]
            public float moveSpeed = 10F;

            [Header("Charge")]
            public float chargeSpeed = 20F;
            [Range(0F, 1F)]
            public float chargeChance = .5F;
            public float chargeCooldown = 5F;
            public Radius chargeMaxDistance = new Radius(20F, true);
            public float chargeFallbackMaxTime = 5F;
            public int chargeHitDamage = 30;
            
            [Header("Attack")]
            public float attackCooldown = 2F;
            public int attackDamage = 20;
        }

        private class TorosaurusShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}