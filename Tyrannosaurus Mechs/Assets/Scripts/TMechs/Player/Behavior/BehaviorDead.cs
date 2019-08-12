using System.Collections;
using TMechs.UI;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    public class BehaviorDead : PlayerBehavior
    {
        public override void OnPush()
        {
            base.OnPush();

            Animancer.GetLayer(Player.LAYER_TOP).Stop();
            Animancer.CrossFadeFromStart(player.GetClip(Player.PlayerAnim.Death), 0.025F, Player.LAYER_TOP).OnEnd = OnAnimEnd;

            if (!player.vfx.death)
                return;
            
            player.vfx.death.gameObject.SetActive(true);
            player.vfx.death.Play();
        }

        public override bool CanMove() => false;
        public override float GetSpeed() => 0F;

        private void OnAnimEnd()
        {
            // Disable animancer here to prevent this function from being called multiple times
            Animancer.enabled = false;
            player.StartCoroutine(Die());
        }

        private IEnumerator Die()
        {
            yield return new WaitForSeconds(2F);
            SceneTransition.LoadScene(0);
        }
    }
}