using System;
using Animancer;
using JetBrains.Annotations;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    [PublicAPI]
    public abstract class PlayerBehavior
    {
        protected static Rewired.Player Input => Player.Input;
        
        [NonSerialized]
        public Player player;
        
        // ReSharper disable once InconsistentNaming - keep consistency with Unity naming
        public Transform transform => player.transform;
        // ReSharper disable once InconsistentNaming - keep consistency with Unity naming
        public GameObject gameObject => player.gameObject;

        public AnimancerComponent Animancer => player.Animancer;

        public virtual void OnInit()
        {
        }
        
        /**
         * Called when this behavior is pushed
         */
        public virtual void OnPush()
        {
        }

        /**
         * Called when another behavior is pushed in front of this one
         */
        public virtual void OnShadowed()
        {
        }
        
        /**
         * Called when the behavior in front of this one is popped
         */
        public virtual void OnSurfaced()
        {
        }
        
        /**
         * Called when this behavior is popped
         */
        public virtual void OnPop()
        {
        }
        
        /**
         * Called on player tick
         */
        public virtual void OnUpdate()
        {
        }

        /**
         * Called when an animation event is triggered
         */
        public virtual void OnAnimationEvent(AnimationEvent e)
        {
        }

        public virtual float GetSpeed() => player.movement.movementSpeed;
        public virtual bool CanMove() => true;

        public T GetComponent<T>()
            => player.GetComponent<T>();

        public T GetComponentInChildren<T>(bool includeInactive = false)
            => player.GetComponentInChildren<T>(includeInactive);

        public T GetComponentInParent<T>()
            => player.GetComponentInParent<T>();
    }
}