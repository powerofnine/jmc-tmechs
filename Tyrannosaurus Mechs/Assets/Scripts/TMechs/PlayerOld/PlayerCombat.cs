using System;
using System.Collections.Generic;
using System.Linq;
using TMechs.Attributes;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.Player;
using TMechs.UI.GamePad;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static TMechs.Controls.Action;

namespace TMechs.PlayerOld
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
        private readonly Dictionary<string, HashSet<PlayerHitbox>> hitboxes = new Dictionary<string, HashSet<PlayerHitbox>>();

        private void Awake()
        {
            animator = Player.Instance.Animator;

            foreach (PlayerHitbox hitbox in GetComponentsInChildren<PlayerHitbox>())
            {
                if (!hitboxes.ContainsKey(hitbox.id))
                    hitboxes.Add(hitbox.id, new HashSet<PlayerHitbox>());

                hitboxes[hitbox.id].Add(hitbox);
                hitbox.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            EnemyTarget enemyTarget = TargetController.Instance.GetTarget<EnemyTarget>();
            GrappleTarget grappleTarget = TargetController.Instance.GetTarget<GrappleTarget>();

            if (enemyTarget)
                GamepadLabels.AddLabel(IconMap.Icon.L1, TargetController.Instance.GetLock() ? "Unlock" : "Lock-on");
            
            if (Input.GetButtonDown(LOCK_ON))
            {
                if (TargetController.Instance.GetLock())
                    TargetController.Instance.Unlock();
                else
                    TargetController.Instance.HardLock();
            }

            animator.SetBool(Anim.HAS_ENEMY, enemyTarget);
            animator.SetBool(Anim.HAS_GRAPPLE, grappleTarget);
            animator.SetBool(Anim.DASH, Input.GetButtonDown(DASH));
            animator.SetBool(Anim.LEFT_ARM_HELD, Input.GetButton(LEFT_ARM));
            animator.SetBool(Anim.RIGHT_ARM, Input.GetButtonDown(RIGHT_ARM));
            animator.SetBool(Anim.RIGHT_ARM_HELD, Input.GetButton(RIGHT_ARM));
            animator.SetBool(Anim.ATTACK_HELD, Input.GetButton(ATTACK));
            animator.SetBool(Anim.ROCKET_READY, rocketFistCharge <= 0F);
            
            if(enemyTarget && rocketFistCharge <= 0F || rocketFistCharging)
                GamepadLabels.AddLabel(IconMap.Icon.L2, "Rocket Fist");
                
            if (Input.GetButtonDown(ATTACK))
                animator.SetTrigger(Anim.ATTACK);

            animator.SetInteger(Anim.PICKUP_TARGET_TYPE, 0);
            if (enemyTarget && Vector3.Distance(transform.position, enemyTarget.transform.position) < grappleRadius)
            {
                if (!grappleTarget || Player.Instance.Movement.isGrounded)
                {
                    animator.SetInteger(Anim.PICKUP_TARGET_TYPE, (int) enemyTarget.pickup);
                    GamepadLabels.AddLabel(IconMap.Icon.R2, "Grab");
                }
            }

            if (!rocketFistCharging)
                rocketFistCharge -= Time.deltaTime * rocketFistRechargeSpeedMultiplier;

            rocketFistCharge = Mathf.Clamp(rocketFistCharge, 0F, rocketFistChargeMax);
        }

        public void OnHitboxTrigger(PlayerHitbox hitbox, EntityHealth entity)
        {
            if (hitbox.id != combat.activeHitbox)
                return;

            entity.Damage(combat.damage);
        }

        public void SetHitbox(string hitbox, float damage)
        {
            if (!string.IsNullOrWhiteSpace(combat.activeHitbox) && hitboxes.ContainsKey(combat.activeHitbox))
                foreach (PlayerHitbox box in hitboxes[combat.activeHitbox])
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
            foreach (PlayerHitbox box in hitboxes[combat.activeHitbox])
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