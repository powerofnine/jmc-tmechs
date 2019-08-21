using Animancer;
using TMechs.UI;
using TMechs.UI.Controllers;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    public class BehaviorDead : PlayerBehavior
    {
        private DeathScreenController dsc;
        
        public override void OnPush()
        {
            base.OnPush();

            Animancer.GetLayer(Player.LAYER_TOP).Stop();
            AnimancerState state = Animancer.CrossFadeFromStart(player.GetClip(Player.PlayerAnim.Death), 0.025F, Player.LAYER_TOP);
            
            if (player.deathScreenTemplate)
                dsc = Object.Instantiate(player.deathScreenTemplate).GetComponent<DeathScreenController>();
            
            state.Speed = .1F;
            state.OnEnd = OnAnimEnd;

            MenuActions.SetPause(true, false);
            Animancer.UpdateMode = AnimatorUpdateMode.UnscaledTime;
            
            if (!player.vfx.death)
                return;
            
            player.vfx.death.gameObject.SetActive(true);
            player.vfx.death.Play();
        }

        public override bool CanMove() => false;
        public override float GetSpeed() => 0F;
        public override bool CanAnimateTakeDamage() => false;

        private void OnAnimEnd()
        {
            // Disable animancer here to prevent this function from being called multiple times
            Animancer.enabled = false;
            
            if (!dsc)
            {
                SceneTransition.LoadScene(1);
                return;
            }

            dsc.StartCoroutine(dsc.FadeCanvas());
        }
    }
}