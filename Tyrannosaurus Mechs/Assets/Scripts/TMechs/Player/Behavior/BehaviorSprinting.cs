namespace TMechs.Player.Behavior
{
    public class BehaviorSprinting : PlayerBehavior
    {
        public override float GetSpeed() => player.Movement.runSpeed;
    }
}
