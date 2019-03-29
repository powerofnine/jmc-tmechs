using System;
using System.Collections.Generic;
using TMechs.Entity;
using TMechs.Environment.Targets;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        public float grappleRadius = 10F;

        [Header("Rocket Fist")]
        public float rocketFistDamageBase;
        public float rocketFistDamageMax;
        public float rocketFistChargeMax;
        public float rocketFistRechargeSpeedMultiplier = 2F;
        [NonSerialized]
        public float rocketFistCharge;

        public float RocketFistDamage => Mathf.Lerp(rocketFistDamageBase, rocketFistDamageMax, rocketFistCharge / rocketFistChargeMax);
        
        private CombatState combat;
        
        private static Rewired.Player Input => PlayerMovement.Input;

        private Animator animator;
        private readonly Dictionary<string, PlayerHitBox> hitboxes = new Dictionary<string,PlayerHitBox>();
        
        private void Awake()
        {
            animator = GetComponent<Animator>();

            foreach (PlayerHitBox hitbox in GetComponentsInChildren<PlayerHitBox>())
            {
                if (hitboxes.ContainsKey(hitbox.id))
                {
                    Debug.LogErrorFormat("HitBox id {0} already exists when trying to add {1}", hitbox.id, hitbox);
                    continue;
                }
                
                hitboxes.Add(hitbox.id, hitbox);
                hitbox.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            BaseTarget target = TargetController.Instance.GetTarget();
            
            if (Input.GetButtonDown(LOCK_ON))
            {
                if (TargetController.Instance.GetLock())
                    TargetController.Instance.Unlock();
                else
                    TargetController.Instance.HardLock();
            }

            animator.SetBool(Anim.HAS_ENEMY, target is EnemyTarget);
            animator.SetBool(Anim.HAS_GRAPPLE, target is GrappleTarget);
            
            if(animator.GetBool(Anim.ANGERY) != Input.GetButton(ANGERY))
                animator.ResetTrigger(Anim.ATTACK);
            
            animator.SetBool(Anim.ANGERY, Input.GetButton(ANGERY));
            animator.SetBool(Anim.DASH, Input.GetButtonDown(DASH));
            animator.SetBool(Anim.GRAPPLE, Input.GetButtonDown(GRAPPLE));
            animator.SetBool(Anim.GRAPPLE_DOWN, Input.GetButton(GRAPPLE));
            animator.SetBool(Anim.ATTACK_HELD, Input.GetButton(ATTACK));
            animator.SetBool(Anim.ROCKET_READY, rocketFistCharge <= 0F);
            
            if(Input.GetButtonDown(ATTACK))
                animator.SetTrigger(Anim.ATTACK);

            if (target is EnemyTarget && Vector3.Distance(transform.position, target.transform.position) < grappleRadius)
            {
                EnemyTarget enemy = (EnemyTarget) target;
                
                animator.SetInteger(Anim.PICKUP_TARGET_TYPE, (int)enemy.pickup);
            }
            else
                animator.SetInteger(Anim.PICKUP_TARGET_TYPE, 0);

            int stateHash = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Arms")).shortNameHash;
            if (!Anim.RAINBOW.ContainsKey(stateHash) || !Anim.RAINBOW[stateHash].StartsWith("Rocket Fist"))
                rocketFistCharge = Mathf.Clamp(rocketFistCharge - Time.deltaTime * rocketFistRechargeSpeedMultiplier, 0F, rocketFistChargeMax);
        }

        public void OnHitboxTrigger(PlayerHitBox hitbox, EntityHealth entity)
        {
            if (hitbox != combat.activeHitbox)
                return;
            
            entity.Damage(combat.damage);
        }

        public void SetHitbox(string hitbox, float damage)
        {
            if(combat.activeHitbox)
                combat.activeHitbox.gameObject.SetActive(false);
            combat.damage = 0F;
            
            if (string.IsNullOrWhiteSpace(hitbox))
                return;

            if (!hitboxes.ContainsKey(hitbox))
            {
                Debug.LogErrorFormat("HitBox ID {0} does not exist", hitbox);
                return;
            }

            combat.activeHitbox = hitboxes[hitbox];
            combat.activeHitbox.gameObject.SetActive(true);
            combat.damage = damage;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, grappleRadius);
        }

        private struct CombatState
        {
            public PlayerHitBox activeHitbox;
            public float damage;
        }
    }
}