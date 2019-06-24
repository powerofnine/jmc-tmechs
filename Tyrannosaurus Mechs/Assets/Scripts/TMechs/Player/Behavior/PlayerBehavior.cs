namespace TMechs.Player.Behavior
{
    public abstract class PlayerBehavior
    {
        protected Player player;

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }
        
        public virtual void OnUpdate()
        {
        }

        public virtual float GetSpeedMultiplier() => CanMove() ? 1F : 0F;
        public virtual bool CanMove() => true;

        public void SetProperties(Player player)
        {
            this.player = player;
        }
    }
}