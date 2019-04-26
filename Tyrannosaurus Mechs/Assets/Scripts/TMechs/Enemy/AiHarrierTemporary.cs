using System.Collections.Generic;
using System.Linq;
using TMechs.Enemy.AI;
using TMechs.InspectorAttributes;
using UnityEngine;

namespace TMechs.Enemy
{
    public class AiHarrierTemporary : MonoBehaviour, AnimatorEventListener.IAnimatorEvent
    {
        public static readonly int ATTACK = Animator.StringToHash("Attack");
        public static readonly int MOVE = Animator.StringToHash("Move");
        public static readonly int FIRING = Animator.StringToHash("IsFiring");

        [ReadOnly]
        public string currentState = "None";

        public AiStateMachine stateMachine;
        
        [Header("Chasing")]
        public float rangeStartFollow = 15F;
        public float rangeStopFollow = 25F;
        public float dashSpeed = 10F;
        
        [Header("Shooting")]
        public float shootRange = 10F;
        public float minTime = 2F;
        public float maxTime = 5F;
        public float minFrequency = 5F;
        public float maxFrequency = 10F;
        public float fireRate = .5F;
        public int bulletDamage = 1;

        [Header("Attacking")]
        public float attackRange = 1F;
        public int attackDamage = 10;
        public float attackCooldown = 2F;

        private readonly Stack<IState> state = new Stack<IState>();
        private CharacterController controller;
        private Animator animator;

        private Stack<IState> executionStack;

        private void Awake()
        {
            stateMachine = new AiStateMachine(transform);
            
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();

            shootRange *= transform.lossyScale.x;
            rangeStopFollow *= transform.lossyScale.x;
            rangeStopFollow *= transform.lossyScale.x;
            
            PushState(new Idle());
        }

        private void Update()
        {
            if (state.Count == 0)
            {
                currentState = "None";
                return;
            }

            if (!Player.Player.Instance)
                return;

            currentState = state.Peek().GetType().Name;

            executionStack = new Stack<IState>(state.Reverse());
            executionStack.Peek().StateUpdate(this);
        }

        private void PushState(IState state)
        {
            this.state.Push(state);
            state.OnEnter(this);
        }

        private void PopState()
        {
            if (state.Count > 0)
            {
                if (executionStack.Peek() == state.Peek())
                    executionStack.Pop();
                state.Pop().OnExit(this);
            }
        }

        private void Fallthrough()
        {
            executionStack.Pop();
            if (executionStack.Count > 0)
                executionStack.Peek().StateUpdate(this);
        }

        public void OnAnimationEvent(string id)
        {
            foreach (IAnimationCallback s in state.Where(x => x is IAnimationCallback).Cast<IAnimationCallback>())
                if (s.AnimationCallback(this, id))
                    break;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rangeStartFollow * transform.lossyScale.x);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, rangeStopFollow * transform.lossyScale.x);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange * transform.lossyScale.x);
        }

        private interface IState
        {
            void OnEnter(AiHarrierTemporary ai);
            void OnExit(AiHarrierTemporary ai);
            void StateUpdate(AiHarrierTemporary ai);
        }

        private interface IAnimationCallback
        {
            bool AnimationCallback(AiHarrierTemporary ai, string id);
        }

        private class Idle : IState
        {
            public void OnEnter(AiHarrierTemporary ai)
            {
            }

            public void OnExit(AiHarrierTemporary ai)
            {
            }

            public void StateUpdate(AiHarrierTemporary ai)
            {
                if (Vector3.Distance(Player.Player.Instance.transform.position, ai.transform.position) <= ai.rangeStartFollow)
                    ai.PushState(new Chasing());
            }
        }

        private class Chasing : IState
        {
            private float nextShooting;

            private float dashTimer = 0F;
            private Vector3 dashDirection;

            public void OnEnter(AiHarrierTemporary ai)
            {
                nextShooting = Random.Range(ai.minFrequency, ai.maxFrequency);
            }

            public void OnExit(AiHarrierTemporary ai)
            {
            }

            public void StateUpdate(AiHarrierTemporary ai)
            {
                Vector3 heading = Player.Player.Instance.transform.position - ai.transform.position;
                float yHeading = heading.y;
                heading = heading.Remove(Utility.Axis.Y);
                
                float distance = heading.magnitude;
                Vector3 direction = heading / distance;

                float fullDistance = Vector3.Distance(Player.Player.Instance.transform.position, ai.transform.position);
                
                if (distance >= ai.rangeStopFollow)
                {
                    ai.PopState();
                    return;
                }

                ai.transform.forward = direction;
                
                nextShooting -= Time.deltaTime;

                if (nextShooting <= 0F && ai.state.Peek() == this)
                {
                    if (distance <= ai.shootRange)
                        ai.PushState(new Shooting());

                    nextShooting = Random.Range(ai.minFrequency, ai.maxFrequency);
                    return;
                }

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

                        if (distance <= ai.attackRange)
                            dashDirection *= 0F;
                        
                        if (Mathf.Abs(yHeading) > 15F)
                            dashDirection.y = Mathf.Sign(yHeading);
                        else
                            dashDirection.y = Random.Range(-.75F, .75F);
                    }
                }
                
                ai.controller.Move(dashDirection * ai.dashSpeed * Time.deltaTime);
                
                if (fullDistance <= ai.attackRange && ai.state.Peek() == this)
                    ai.PushState(new Attacking());
            }
        }

        private class Shooting : IState
        {
            private float shootTime;
            private float nextShot;

            private float dashTime;
            
            public void OnEnter(AiHarrierTemporary ai)
            {
                shootTime = Random.Range(ai.minTime, ai.maxTime);
                nextShot = ai.fireRate;
                ai.animator.SetBool(FIRING, true);
            }

            public void OnExit(AiHarrierTemporary ai)
            {
                ai.animator.SetBool(FIRING, false);
            }

            public void StateUpdate(AiHarrierTemporary ai)
            {
                shootTime -= Time.deltaTime;
                if (Vector3.Distance(Player.Player.Instance.transform.position, ai.transform.position) > ai.shootRange || shootTime <= 0F)
                {
                    ai.PopState();
                    return;
                }

                nextShot -= Time.deltaTime;
                if (nextShot <= 0F)
                {
                    nextShot = ai.fireRate;
                    //TODO shoot
                }
            }
        }

        private class Attacking : IState, IAnimationCallback
        {
            private float cooldown;
            private int state = 0;

            private bool attackTriggered = true;

            public void OnEnter(AiHarrierTemporary ai)
            {
                cooldown = ai.attackCooldown;
            }

            public void OnExit(AiHarrierTemporary ai)
            {
            }

            public void StateUpdate(AiHarrierTemporary ai)
            {
                if (!attackTriggered)
                    return;

                switch (state)
                {
                    case 0:
                    case 1:
                        ai.animator.SetTrigger(ATTACK);
                        attackTriggered = false;
                        state++;
                        return;
                }

                cooldown -= Time.deltaTime;

                if (cooldown <= 0F)
                {
                    ai.PopState();
                    return;
                }

                ai.Fallthrough();
            }

            public bool AnimationCallback(AiHarrierTemporary ai, string id)
            {
                if (!"attack".Equals(id))
                    return false;

                attackTriggered = true;
                Player.Player.Instance.Damage(ai.attackDamage);

                return true;
            }
        }
    }
}