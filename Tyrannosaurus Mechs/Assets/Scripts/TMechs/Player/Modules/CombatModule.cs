using System;
using System.Collections.Generic;
using System.Linq;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.UI.GamePad;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player.Modules
{
    [Serializable]
    public class CombatModule : PlayerModule
    {
        private readonly Dictionary<string, List<PlayerHitbox>> hitboxes = new Dictionary<string, List<PlayerHitbox>>();
        private bool hitboxUnique;
        private readonly HashSet<EntityHealth> hits = new HashSet<EntityHealth>();
        private string activeHitbox;
        private float hitboxDamage;

        public override void OnRegistered()
        {
            base.OnRegistered();

            foreach (PlayerHitbox hitbox in player.GetComponentsInChildren<PlayerHitbox>(true))
            {
                if(!hitboxes.ContainsKey(hitbox.id))
                    hitboxes.Add(hitbox.id, new List<PlayerHitbox>());
                
                hitboxes[hitbox.id].Add(hitbox);
                hitbox.gameObject.SetActive(false);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            EnemyTarget enemyTarget = TargetController.Instance.GetTarget<EnemyTarget>();

            if (enemyTarget)
                GamepadLabels.AddLabel(IconMap.Icon.L1, TargetController.Instance.GetLock() ? "Unlock" : "Lock-on");
            
                        
            if (Input.GetButtonDown(LOCK_ON))
            {
                if (TargetController.Instance.GetLock())
                    TargetController.Instance.Unlock();
                else
                    TargetController.Instance.HardLock();
            }
        }

        public void SetHitbox(string hitbox, float damage, bool unique = true)
        {
            if(!string.IsNullOrWhiteSpace(activeHitbox) && hitboxes.ContainsKey(activeHitbox))
                foreach(PlayerHitbox box in hitboxes[activeHitbox])
                    box.gameObject.SetActive(false);
                
            hitboxUnique = unique;
            hitboxDamage = damage;
            activeHitbox = hitbox;
            hits.Clear();
            
            if(!string.IsNullOrWhiteSpace(activeHitbox) && hitboxes.ContainsKey(activeHitbox))
                foreach(PlayerHitbox box in hitboxes[activeHitbox])
                    box.gameObject.SetActive(true);
        }

        public void OnHitbox(string id, Collider other)
        {
            if (other.CompareTag("Player"))
                return;
            
            if (other.CompareTag("Player") || id != activeHitbox)
                return;

            EntityHealth health = other.GetComponent<EntityHealth>();
            if (health)
            {
                if (hitboxUnique && hits.Contains(health))
                    return;
                
                health.Damage(hitboxDamage);
                hits.Add(health);

            }
        }
        
        public void DealAoe(float radius, float minDamage, float maxDamage)
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider[] colliders = Physics.OverlapBox(transform.position, radius * 2F * Vector3.one.Remove(Utility.Axis.Y) + Vector3.up * 2F, transform.rotation);

            IEnumerable<EntityHealth> targets = 
                    from c in colliders
                    where !c.CompareTag("Player")
                    let h = c.GetComponent<EntityHealth>()
                    where h
                    where Vector3.Distance(transform.position.Remove(Utility.Axis.Y), c.transform.position.Remove(Utility.Axis.Y)) <= radius
                    select h;
            
            foreach(EntityHealth health in targets)
                health.Damage(Mathf.Lerp(maxDamage, minDamage, Vector3.Distance(transform.position.Remove(Utility.Axis.Y), health.transform.position.Remove(Utility.Axis.Y)) / radius));

        }
    }
}