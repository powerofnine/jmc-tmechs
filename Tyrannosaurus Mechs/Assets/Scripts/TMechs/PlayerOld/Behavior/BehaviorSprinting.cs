namespace TMechs.PlayerOld.Behavior
{
    public class BehaviorSprinting : PlayerBehavior
    {
        public override float GetSpeed() => player.Movement.runSpeed;
    }
}
