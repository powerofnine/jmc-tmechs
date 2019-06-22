using System;
using System.Collections.Generic;
using System.Linq;
using TMechs.Attributes;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.UI.GamePad;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        public float grappleRadius = 10F;
        
        [Header("Jump AOE")]
        public float jumpAoeRadius = 20F;
        [MinMax]
        public Vector2 jumpAoeDamage = new Vector2(10F, 20F);
        
        
        [Header("Rocket Fist")]
        public float rocketFistDamageBase;
        public float rocketFistDamageMax;
        public float rocketFistChargeMax;
        public float rocketFistRechargeSpeedMultiplier = 2F;
        
        [NonSerialized]
        public bool rocketFistCharging;
        [NonSerialized]
        public float rocketFistCharge;

        public float RocketFistDamage => Mathf.Lerp(rocketFistDamageBase, rocketFistDamageMax, rocketFistCharge / rocketFistChargeMax);

        private CombatState combat;

        private static Rewired.Player Input => Player.Input;

        private Animator animator;
        private readonly Dictionary<string, HashSet<PlayerHitBox>> hitboxes = new Dictionary<string, HashSet<PlayerHitBox>>();

        private void Awake()
        {
            animator = Player.Instance.Animator;

            foreach (PlayerHitBox hitbox in GetComponentsInChildren<PlayerHitBox>())
            {
                if (!hitboxes.ContainsKey(hitbox.id))
                    hitboxes.Add(hitbox.id, new HashSet<PlayerHitBox>());

                hitboxes[hitbox.id].Add(hitbox);
                hitbox.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            BaseTarget target = TargetController.Instance.GetTarget();

            if (target)
                GamepadLabels.AddLabel(IconMap.Icon.L1, TargetController.Instance.GetLock() ? "Unlock" : "Lock-on");
            
            if (Input.GetButtonDown(LOCK_ON))
            {
                if (TargetController.Instance.GetLock())
                    TargetController.Instance.Unlock();
                else
                    TargetController.Instance.HardLock();
            }

            animator.SetBool(Anim.HAS_ENEMY, target is EnemyTarget);
            animator.SetBool(Anim.HAS_GRAPPLE, target is GrappleTarget);
            animator.SetBool(Anim.DASH, Input.GetButtonDown(DASH));
            animator.SetBool(Anim.LEFT_ARM_HELD, Input.GetButton(LEFT_ARM));
            animator.SetBool(Anim.RIGHT_ARM, Input.GetButtonDown(RIGHT_ARM));
            animator.SetBool(Anim.RIGHT_ARM_HELD, Input.GetButton(RIGHT_ARM));
            animator.SetBool(Anim.ATTACK_HELD, Input.GetButton(ATTACK));
            animator.SetBool(Anim.ROCKET_READY, rocketFistCharge <= 0F);
            
            if(target is EnemyTarget && rocketFistCharge <= 0F || rocketFistCharging)
                GamepadLabels.AddLabel(IconMap.Icon.L2, "Rocket Fist");
                
            if (Input.GetButtonDown(ATTACK))
                animator.SetTrigger(Anim.ATTACK);

            if (target is EnemyTarget && Vector3.Distance(transform.position, target.transform.position) < grappleRadius)
            {
                EnemyTarget enemy = (EnemyTarget) target;

                animator.SetInteger(Anim.PICKUP_TARGET_TYPE, (int) enemy.pickup);
                GamepadLabels.AddLabel(IconMap.Icon.R2, "Grab");
            }
            else
                animator.SetInteger(Anim.PICKUP_TARGET_TYPE, 0);

            if (!rocketFistCharging)
                rocketFistCharge = Mathf.Clamp(rocketFistCharge - Time.deltaTime * rocketFistRechargeSpeedMultiplier, 0F, rocketFistChargeMax);
        }

        public void OnHitboxTrigger(PlayerHitBox hitbox, EntityHealth entity)
        {
            if (hitbox.id != combat.activeHitbox)
                return;

            entity.Damage(combat.damage);
        }

        public void SetHitbox(string hitbox, float damage)
        {
            if (!string.IsNullOrWhiteSpace(combat.activeHitbox) && hitboxes.ContainsKey(combat.activeHitbox))
                foreach (PlayerHitBox box in hitboxes[combat.activeHitbox])
                    box.gameObject.SetActive(false);
            combat.damage = 0F;

            if (string.IsNullOrWhiteSpace(hitbox))
                return;

            if (!hitboxes.ContainsKey(hitbox))
            {
                Debug.LogErrorFormat("HitBox ID {0} does not exist", hitbox);
                return;
            }

            combat.activeHitbox = hitbox;
            foreach (PlayerHitBox box in hitboxes[combat.activeHitbox])
                box.gameObject.SetActive(true);

            combat.damage = damage;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, grappleRadius);

            Handles.color = Color.green;
            Handles.DrawWireDisc(transform.position, Vector3.up, jumpAoeRadius);
        }
#endif

        public void PerformAoe()
        {
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider[] colliders = Physics.OverlapBox(transform.position, jumpAoeRadius * 2F * Vector3.one.Remove(Utility.Axis.Y) + Vector3.up, transform.rotation);

            IEnumerable<EntityHealth> targets = 
                    from c in colliders
                    let h = c.GetComponent<EntityHealth>()
                    where h
                    let d = Vector3.Distance(transform.position, c.transform.position)
                    where d <= jumpAoeRadius
                    select h;
            
            foreach(EntityHealth health in targets)
                health.Damage(Mathf.Lerp(jumpAoeDamage.y, jumpAoeDamage.x, Vector3.Distance(transform.position, health.transform.position) / jumpAoeRadius));

        }
        
        private struct CombatState
        {
            public string activeHitbox;
            public float damage;
        }
    }
}