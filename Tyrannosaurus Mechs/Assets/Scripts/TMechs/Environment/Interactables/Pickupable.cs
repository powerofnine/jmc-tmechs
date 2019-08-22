using TMechs.Player;
using TMechs.Player.Behavior;
using TMechs.Player.Modules;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Environment.Interactables
{
    public class Pickupable : Interactable, ThrowableContainer.IOnThrowableReleased
    {
        public VisualEffectAsset destroyVfx;
        public float destroyVfxTime = 2F;

        public bool destroy = true;

        public AudioSource onDie;
        
        public override PlayerBehavior GetPushBehavior()
        {
            Player.Player.Instance.carry.overrideTarget = gameObject;
            return Player.Player.Instance.carry;
        }

        public override int GetSortPriority() => -1;
        
        public void OnThrowableReleased()
        {
            VfxModule.SpawnEffect(destroyVfx, transform.position, Quaternion.identity, destroyVfxTime);

            if (destroy)
            {
                Destroy(gameObject);

                if (onDie)
                {
                    onDie.transform.SetParent(null, true);
                    
                    onDie.gameObject.SetActive(true);
                    onDie.enabled = true;
                    
                    onDie.Play();
                    Destroy(onDie, onDie.clip.length);
                }
            }
        }
    }
}
