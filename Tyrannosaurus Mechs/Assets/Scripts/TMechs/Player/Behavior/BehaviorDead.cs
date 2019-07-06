using TMechs.UI;

namespace TMechs.Player.Behavior
{
    public class BehaviorDead : PlayerBehavior
    {
        public override void OnPush()
        {
            base.OnPush();

            Animancer.CrossFadeFromStart(player.GetClip(Player.PlayerAnim.Death), 0.25F, 3).OnEnd = OnAnimEnd;
        }

        public override bool CanMove() => false;
        public override float GetSpeed() => 0F;

        private void OnAnimEnd()
        {
            // Disable animancer here to prevent this function from being called multiple times
            Animancer.enabled = false;
            SceneTransition.LoadScene(0);
        }
    }
}