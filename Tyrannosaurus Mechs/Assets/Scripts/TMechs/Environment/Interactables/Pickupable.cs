﻿using TMechs.Player;
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
        
        public override void OnInteract()
        {
        }

        public override PlayerBehavior GetPushBehavior()
        {
            Player.Player.Instance.carry.overrideTarget = gameObject;
            return Player.Player.Instance.carry;
        }

        public override int GetSortPriority() => -1;
        
        public void OnThrowableReleased()
        {
            VfxModule.SpawnEffect(destroyVfx, transform.position, Quaternion.identity, destroyVfxTime);
            Destroy(gameObject);
        }
    }
}
