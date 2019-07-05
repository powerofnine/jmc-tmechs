namespace TMechs.PlayerOld.Behavior
{
    public class PlayerBehavior
    {
        public static readonly PlayerBehavior SPRINTING = new BehaviorSprinting();
        public static readonly PlayerBehavior ROCKET_FIST = new BehaviorRocketFist();
        
        protected Player player;

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

        public virtual float GetSpeed() => player.Movement.movementSpeed;
        public virtual bool CanMove() => true;

        public void SetProperties(Player player)
        {
            this.player = player;
        }
    }
}