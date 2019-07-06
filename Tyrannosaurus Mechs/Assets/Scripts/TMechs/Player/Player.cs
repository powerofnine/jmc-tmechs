using System;
using System.Collections.Generic;
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
        
        private readonly Stack<PlayerBehavior> behaviorStack = new Stack<PlayerBehavior>();
        
        [NotNull]
        public PlayerBehavior Behavior => behaviorStack.Peek();
        public bool CanMove => Behavior.CanMove();
        public float Speed => Behavior.GetSpeed();

        private void Awake()
        {
            Instance = this;
            
            Input = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
            Health = GetComponent<EntityHealth>();
            
            RegisterModule(forces);
            RegisterModule(movement);
            
            // Push the basic behavior state
            // _Do variant is used to avoid accessing the empty stack, this is done to avoid having an empty check for 
            // a stack that is only empty in this one occasion
            PushBehavior_Do(new PlayerBehavior());
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
            Behavior?.OnShadowed();
            
            PushBehavior_Do(behavior);
        }

        private void PushBehavior_Do([NotNull] PlayerBehavior behavior)
        {
            behaviorStack.Push(behavior);

            behavior.SetProperties(this);
            behavior.OnPush();
        }
        
        [NotNull]
        public PlayerBehavior PopBehavior()
        {
            Behavior.OnPop();

            if(behaviorStack.Count > 1)
                return behaviorStack.Pop();

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
