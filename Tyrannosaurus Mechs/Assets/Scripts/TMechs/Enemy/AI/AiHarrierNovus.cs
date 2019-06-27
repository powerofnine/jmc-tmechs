using System;
using JetBrains.Annotations;
using TMechs.Entity;
using TMechs.Types;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMechs.Enemy.AI
{
    public class AiHarrierNovus : MonoBehaviour, AnimatorEventListener.IAnimatorEvent, EntityHealth.IDamage, EntityHealth.IDeath
    {
        private bool isDead = false;
        private float deadVelocity;
        
        public static readonly int DART = Anim.Hash("Dart");
        public static readonly int DART_LEFT = Anim.Hash("Dart Left");
        public static readonly int DART_RIGHT = Anim.Hash("Dart Right");
        public static readonly int FIRE = Anim.Hash("Fire");
        public static readonly int TAKE_DAMAGE = Anim.Hash("Take Damage");
        public static readonly int DEATH_HIGH = Anim.Hash("Death High");
        public static readonly int DEATH_LOW = Anim.Hash("Death Low");
        
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
                    target = Player.Player.Instance.centerOfMass,
                    shared = shared
            };

            stateMachine.ImportProperties(properties);
            
            stateMachine.RegisterState(null, "Idle");
            stateMachine.RegisterState(new Moving(), "Moving");
            stateMachine.RegisterState(new Shooting(), "Shooting");
            
            stateMachine.RegisterTransition(AiStateMachine.ANY_STATE, "Idle", machine => machine.DistanceToTarget > machine.Get<Radius>(nameof(HarrierProperties.rangeStopFollow)));
            stateMachine.RegisterTransition("Idle", "Moving", machine => machine.DistanceToTarget <= machine.Get<Radius>(nameof(HarrierProperties.rangeStartFollow)));
            stateMachine.RegisterTransition("Moving", "Shooting", machine => machine.GetTrigger("Shoot"));
            stateMachine.RegisterTransition("Shooting", "Moving", machine => machine.GetTrigger("ShootDone"));
            
            stateMachine.SetDefaultState("Idle");
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

                Vector3 fw = DirectionToTarget;

                fw.y = Mathf.Clamp(fw.y, -.25F, .25F);
                fw.Normalize();
                
                transform.forward = fw;

                if (time >= 0F)
                {
                    time -= Time.deltaTime;

                    props.controller.Move(Machine.Get<float>(nameof(HarrierProperties.dashSpeed)) * Time.deltaTime * direction);
                    
                    return;
                }

                if (dashesDone >= Machine.Get<int>(nameof(HarrierProperties.moveCount)))
                {
                    Machine.SetTrigger("Shoot");
                    return;
                }
                
                if (direction.sqrMagnitude > Mathf.Epsilon)
                {
                    direction = Vector3.zero;
                    dashesDone++;
                    time = Machine.Get<float>(nameof(HarrierProperties.dashDelay));
                }
                else
                {
                    direction = new Vector3(Random.Range(-1F, 1F), Random.Range(-.35F, 1F), 0F);
                    int dir = Mathf.Abs(direction.x) > .25F ? (int)Mathf.Sign(direction.x) : 0;
                    
                    direction = transform.TransformDirection(direction);
                    
                    Vector3 heading = target.position - transform.position;

                    float distDifference = HorizontalDistanceToTarget - Machine.Get<Radius>(nameof(HarrierProperties.maintainDistance));

                    if (Math.Abs(distDifference) < 5F)
                    {
                        direction = DirectionToTarget * Mathf.Sign(distDifference);
                        dir = 0;
                    }

                    if (Mathf.Abs(heading.y) > 10F)
                        direction.y = Mathf.Sign(heading.y);
                    
                    direction.Normalize();

                    time = Machine.Get<Radius>(nameof(HarrierProperties.dashDistance)) / Machine.Get<float>(nameof(HarrierProperties.dashSpeed));
                    props.animator.SetTrigger(dir == 0 ? DART : dir < 0 ? DART_LEFT : DART_RIGHT);
                }
            }
        }

        private class Shooting : HarrierState
        {
            private bool leftSide;
            private int shots;
            private float nextShot;
            
            public override void OnEnter()
            {
                base.OnEnter();

                shots = 0;
                nextShot = 0F;
                leftSide = false;
                
                props.animator.SetTrigger(FIRE);
            }

            public override void OnTick()
            {
                base.OnTick();

                transform.forward = DirectionToTarget;
                
                if (shots <= 0)
                    return;

                if (nextShot > 0F)
                {
                    nextShot -= Time.deltaTime;
                    return;
                }

                shots--;
                
                Transform anchor = leftSide ? Machine.Get<Transform>(nameof(HarrierProperties.leftGunAnchor)) : Machine.Get<Transform>(nameof(HarrierProperties.rightGunAnchor));

                GameObject go = Instantiate(Machine.Get<GameObject>(nameof(HarrierProperties.bulletTemplate)), anchor.position, anchor.rotation);
                EnemyBullet bullet = go.GetComponent<EnemyBullet>();
                bullet.damage = Machine.Get<float>(nameof(HarrierProperties.shotDamage));
                bullet.direction = transform.forward;
                bullet.owner = transform;
 
                if(shots > 0)
                    nextShot = Machine.Get<float>(nameof(HarrierProperties.burstDelay));
                else
                    leftSide = !leftSide;
            }

            public override void OnEvent(AiStateMachine.EventType type, string id)
            {
                base.OnEvent(type, id);

                if (type != AiStateMachine.EventType.Animation)
                    return;

                switch (id)
                {
                    case "Shoot":
                        shots = Machine.Get<int>(nameof(HarrierProperties.burstCount));
                        break;
                    case "ShootDone":
                        Machine.SetTrigger("ShootDone");
                        break;
                }
            }
        }

        #endregion
        
        private void Update()
        {
            if (isDead)
            {
                deadVelocity += Utility.GRAVITY * Time.deltaTime;
                
                ((HarrierShared)stateMachine.shared).controller.Move(deadVelocity * Time.deltaTime * Vector3.down);
                return;
            }
            stateMachine.Tick();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (isDead)
            {
                gameObject.AddComponent<Rigidbody>();

                ((HarrierShared)stateMachine.shared).animator.enabled = false;
                
                CapsuleCollider cap = gameObject.AddComponent<CapsuleCollider>();
                cap.center = ((HarrierShared)stateMachine.shared).controller.center;
                cap.radius = ((HarrierShared)stateMachine.shared).controller.radius;
                cap.height = ((HarrierShared)stateMachine.shared).controller.height;
                
                Destroy(((HarrierShared)stateMachine.shared).controller);
                Destroy(this);
                Destroy(gameObject, 2F);
            }
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
            public int burstCount = 3;
            public float burstDelay = .1F;
            public float shotDamage = 1;
            
            [Header("Chasing")]
            public float chaseSpeed = 20F;
            public int shootsBeforeChase = 3;

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

        public void OnDamaged(EntityHealth health, ref bool cancel)
        {
            ((HarrierShared)stateMachine.shared).animator.SetTrigger(TAKE_DAMAGE);
        }
        
        public void OnDying(ref bool customDestroy)
        {
            isDead = true;
            customDestroy = true;
            
            Collider col = GetComponent<Collider>();

            if (!col)
            {
                ((HarrierShared)stateMachine.shared).animator.SetTrigger(DEATH_LOW);
                return;
            }
            
            if(Physics.Raycast(col.bounds.center, Vector3.down, 5F))
                ((HarrierShared)stateMachine.shared).animator.SetTrigger(DEATH_LOW);
            else
                ((HarrierShared)stateMachine.shared).animator.SetTrigger(DEATH_HIGH);
        }
    }
}