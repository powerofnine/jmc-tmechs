using System;
using JetBrains.Annotations;
using TMechs.Types;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMechs.Enemy.AI
{
    public class AiTankylosaurus : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        private static readonly int ROCK_THROW = Anim.Hash("Rock Throw");

        private static readonly int[] ATTACKS =
        {
                Anim.Hash("Tail Whip (CW)"),
                Anim.Hash("Tail Whip (CCW)"),
                Anim.Hash("Scorpion Punch")
        };

        public AiStateMachine stateMachine;

        public TankylosaurusProperties properties = new TankylosaurusProperties();

        private float yVelocity;

        private void Start()
        {
            CreateStateMachine(new TankyloShared {animator = GetComponentInChildren<Animator>(), controller = GetComponent<CharacterController>()});
        }

        private void Update()
        {
            stateMachine.Tick();

            CharacterController controller = (stateMachine.shared as TankyloShared)?.controller;

            yVelocity += Utility.GRAVITY * Time.deltaTime;

            if (controller)
            {
                controller.Move(Vector3.down * yVelocity);

                if (controller.isGrounded)
                    yVelocity = 0F;
            }
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
            stateMachine.RegisterState(new Chasing(), "Chasing");
            stateMachine.RegisterState(new Attack(), "Attack");
            stateMachine.RegisterState(new Throw(), "Throw");

            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle",
                    machine => machine.DistanceToTarget > machine.Get<Radius>("rangeStopFollow"));
            stateMachine.RegisterTransition("Idle", "Chasing",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("rangeStartFollow"));

            stateMachine.RegisterTransition("Chasing", "Attack",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("attackRange") && machine.GetAddSet<float>("attackTimer", -Time.deltaTime) <= 0F,
                    machine => machine.Set("attackTimer", machine.Get<float>("attackCooldown")));
            stateMachine.RegisterTransition("Attack", "Chasing",
                    machine => machine.GetTrigger("attackDone"));

            stateMachine.RegisterTransition("Chasing", "Throw",
                    machine => machine.DistanceToTarget <= machine.Get<Radius>("rockThrowRange") && machine.GetAddSet<float>("throwTimer", -Time.deltaTime) <= 0F,
                    machine => machine.Set("throwTimer", machine.Get<float>("rockThrowCooldown")));
            stateMachine.RegisterTransition("Throw", "Chasing",
                    machine => machine.GetTrigger("rockThrowDone"));

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

        private class Chasing : TankyloState
        {
            public override void OnTick()
            {
                Vector3 direction = HorizontalDirectionToTarget;

                transform.forward = direction;

                if (DistanceToTarget >= Machine.Get<Radius>("attackRange"))
                    shared.controller.Move(Machine.Get<float>("moveSpeed") * Time.deltaTime * direction);
            }
        }

        private class Attack : TankyloState
        {
            public override void OnEnter()
            {
                base.OnEnter();

                shared.animator.SetTrigger(ATTACKS[Random.Range(0, ATTACKS.Length)]);
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type == AiStateMachine.EventType.Animation && "attack".Equals(id))
                    Machine.SetTrigger("attackDone");
            }
        }

        private class Throw : TankyloState
        {
            private GameObject rock;

            public override void OnEnter()
            {
                base.OnEnter();

                shared.animator.SetTrigger(ROCK_THROW);
            }

            public override void OnExit()
            {
                base.OnExit();

                if (rock)
                    Destroy(rock);
            }

            private void SpawnRock()
            {
                if (!rock)
                    rock = Instantiate(Machine.Get<GameObject>("rockTemplate"), Machine.Get<Transform>("rockAnchor"));
            }

            private void ThrowRock()
            {
                if (!rock)
                {
                    Debug.LogWarning("No rock has been created");
                    return;
                }

                rock.transform.SetParent(null, true);
                rock.AddComponent<Rigidbody>().velocity = Utility.BallisticVelocity(rock.transform.position, target.position, 45F);

                rock = null;
                Machine.SetTrigger("rockThrowDone");
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                switch (id)
                {
                    case "rockReady":
                        SpawnRock();
                        break;
                    case "rockThrow":
                        ThrowRock();
                        break;
                }
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

            [Header("Chasing")]
            public float moveSpeed;

            [Header("Attacc")]
            public float attackCooldown;

            [Header("Rock Throw")]
            public float rockThrowCooldown;
            [Space]
            public Transform rockAnchor;
            public GameObject rockTemplate;
        }

        private class TankyloShared
        {
            public Animator animator;
            public CharacterController controller;
        }
    }
}