using System;
using Animancer;
using JetBrains.Annotations;
using UnityEngine;

namespace TMechs.Player.Modules
{
    [Serializable]
    [PublicAPI]
    public abstract class PlayerModule
    {
        protected static Rewired.Player Input => Player.Input;
        
        public AnimancerComponent Animancer => player.Animancer;
        
        [NonSerialized]
        public Player player;

        // ReSharper disable once InconsistentNaming - keep consistency with Unity naming
        public Transform transform => player.transform;
        // ReSharper disable once InconsistentNaming - keep consistency with Unity naming
        public GameObject gameObject => player.gameObject;
        
        public bool enabled = true;

        public virtual void OnRegistered()
        {}
        
        public virtual void OnStart()
        {}
        
        public virtual void OnUpdate()
        {}

        public virtual void OnLateUpdate()
        {}

        public virtual void OnFixedUpdate()
        {}

        public T GetComponent<T>()
            => player.GetComponent<T>();

        public T GetComponentInChildren<T>(bool includeInactive = false)
            => player.GetComponentInChildren<T>(includeInactive);

        public T GetComponentInParent<T>()
            => player.GetComponentInParent<T>();
    }
}
