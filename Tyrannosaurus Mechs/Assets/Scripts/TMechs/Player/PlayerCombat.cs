using System.Collections.Generic;
using TMechs.Enemy;
using TMechs.Environment.Targets;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
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
            if (Input.GetButtonDown(LOCK_ON))
            {
                if (TargetController.Instance.GetLock())
                    TargetController.Instance.Unlock();
                else
                    TargetController.Instance.HardLock();
            }

            animator.SetBool(Anim.HAS_ENEMY, TargetController.Instance.GetTarget() is EnemyTarget);
            animator.SetBool(Anim.HAS_GRAPPLE, TargetController.Instance.GetTarget() is GrappleTarget);
            
            animator.SetBool(Anim.ANGERY, Input.GetButton(ANGERY));
            animator.SetBool(Anim.DASH, Input.GetButtonDown(DASH));
            animator.SetBool(Anim.GRAPPLE, Input.GetButtonDown(GRAPPLE));
            animator.SetBool(Anim.GRAPPLE_DOWN, Input.GetButton(GRAPPLE));
            
            if(Input.GetButtonDown(ATTACK))
                animator.SetTrigger(Anim.ATTACK);
        }

        public void OnHitboxTrigger(PlayerHitBox hitbox, EnemyHealth enemy)
        {
            if (hitbox != combat.activeHitbox)
                return;
            
            enemy.Damage(combat.damage);
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

        public struct Anim
        {
            public static readonly int HAS_ENEMY = Animator.StringToHash("Has Enemy");
            public static readonly int HAS_GRAPPLE = Animator.StringToHash("Has Grapple");
            public static readonly int ANGERY = Animator.StringToHash("ANGERY");
            public static readonly int DASH = Animator.StringToHash("Dash");
            public static readonly int ATTACK = Animator.StringToHash("Attack");
            public static readonly int GRAPPLE = Animator.StringToHash("Grapple");
            public static readonly int GRAPPLE_DOWN = Animator.StringToHash("Grapple Down");
        }

        private struct CombatState
        {
            public PlayerHitBox activeHitbox;
            public float damage;
        }
    }
}