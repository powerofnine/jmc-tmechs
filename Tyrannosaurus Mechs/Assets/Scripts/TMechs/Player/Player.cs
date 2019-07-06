using System;
using System.Collections.Generic;
using TMechs.Animation;
using TMechs.Player.Modules;
using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour
    {
        private readonly List<PlayerModule> newModules = new List<PlayerModule>();
        private readonly List<PlayerModule> modules = new List<PlayerModule>();

        [AnimationCollection.ValidateAttribute(typeof(PlayerAnim))]
        public AnimationCollection animations;
        
        [Header("Modules")]
        public MovementModule movement = new MovementModule();

        private void Awake()
        {
            RegisterModule(movement);
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
                    module.OnUpdate();
        }

        private void FixedUpdate()
        {
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnUpdate();
        }

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
