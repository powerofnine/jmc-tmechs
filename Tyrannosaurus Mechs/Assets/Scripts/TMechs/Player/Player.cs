using System;
using System.Collections.Generic;
using Animancer;
using JetBrains.Annotations;
using Rewired;
using TMechs.Animation;
using TMechs.Entity;
using TMechs.Player.Behavior;
using TMechs.Player.Modules;
using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        public static Rewired.Player Input { get; private set; }

        public EntityHealth Health { get; private set; }
        public EventfulAnimancerComponent Animancer { get; private set; }
        
        private readonly List<PlayerModule> newModules = new List<PlayerModule>();
        private readonly List<PlayerModule> modules = new List<PlayerModule>();

        [AnimationCollection.ValidateAttribute(typeof(PlayerAnim))]
        public AnimationCollection animations;

        [NonSerialized]
        public Vector3 contactPoint;
        
        [Header("Modules")]
        public ForcesModule forces = new ForcesModule();
        public MovementModule movement = new MovementModule();

        [Header("Behavior")]
        public BehaviorStandard standard = new BehaviorStandard();
        public PlayerBehavior sprint = new BehaviorSprinting();
        public BehaviorJump jump = new BehaviorJump();
        public BehaviorAttack attack = new BehaviorAttack();
        
        private readonly Stack<PlayerBehavior> behaviorStack = new Stack<PlayerBehavior>();
        private readonly HashSet<PlayerBehavior> initializedBehaviors = new HashSet<PlayerBehavior>();

        [NotNull]
        public PlayerBehavior Behavior => behaviorStack.Count > 0 ? behaviorStack.Peek() : standard;
        public bool CanMove => Behavior.CanMove();
        public float Speed => Behavior.GetSpeed();

        private void Awake()
        {
            Instance = this;
            
            Input = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
            Health = GetComponent<EntityHealth>();
            Animancer = GetComponentInChildren<EventfulAnimancerComponent>();
            Animancer.onEvent = new AnimationEventReceiver(null, OnAnimationEvent);
            
            RegisterModule(forces);
            RegisterModule(movement);
            
            PushBehavior(standard);
        }

        private void Update()
        {
            for (int i = 0; i < newModules.Count; i++)
            {
                PlayerModule module = newModules[i];
                
                if (!module.enabled)
                    continue;
                
                module.OnStart();
                newModules.RemoveAt(i);
                modules.Add(module);

                i--;
            }
            
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnUpdate();
            
            Behavior.OnUpdate();
        }

        private void LateUpdate()
        {
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnLateUpdate();
        }

        private void FixedUpdate()
        {
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnFixedUpdate();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
            => contactPoint = hit.point;

        private void OnAnimationEvent(AnimationEvent e)
            => Behavior.OnAnimationEvent(e);
        
        public void RegisterModule(PlayerModule module)
        {
            if (module == null)
                throw new ArgumentException("Module is null");

            module.player = this;
            newModules.Add(module);
            module.OnRegistered();
        }

        public AnimationClip GetClip(PlayerAnim animation)
            => animations ? animations.GetClip(animation) : null;
        
        public void PushBehavior([NotNull] PlayerBehavior behavior)
        { 
            Behavior.OnShadowed();
            
            behavior.player = this;

            if (!initializedBehaviors.Contains(behavior))
            {
                behavior.OnInit();
                initializedBehaviors.Add(behavior);
            }

            behaviorStack.Push(behavior);

            behavior.OnPush();
        }

        [NotNull]
        public PlayerBehavior PopBehavior()
        {
            Behavior.OnPop();

            if(behaviorStack.Count > 1)
                return behaviorStack.Pop();
            
            Behavior.OnSurfaced();

            throw new InvalidOperationException("Cannot pop the last player behavior");
        }

        [AnimationCollection.Enum("Player Animations")]
        public enum PlayerAnim
        {
            [Header("Basic")]
            Idle,
            Death,
        
            [Header("Movement")]
            Walk,
            Run,
            Dash,
            Jump,
            AirJump,
        
            [Header("Attack String")]
            Attack1,
            Attack2,
            Attack3,
            
            [Header("Grapple")]
            GrabObject,
            ThrowObject,
            Grapple,
        
            [Header("Rocket Fist")]
            RocketChargeIntro,
            RocketCharge,
            RocketHold,
            RocketRecover,
            RocketReturn
        }
    }
}
