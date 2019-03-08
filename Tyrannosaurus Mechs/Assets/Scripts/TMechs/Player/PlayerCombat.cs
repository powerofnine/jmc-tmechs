using TMechs.Environment.Targets;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        private static Rewired.Player Input => PlayerMovement.Input;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
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
            animator.SetBool(Anim.ATTACK, Input.GetButtonDown(ATTACK));
            animator.SetBool(Anim.GRAPPLE, Input.GetButtonDown(GRAPPLE));
            animator.SetBool(Anim.GRAPPLE_DOWN, Input.GetButton(GRAPPLE));
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
    }
}