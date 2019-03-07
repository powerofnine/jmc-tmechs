using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        public Rewired.Player Input => PlayerMovement.Input;

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

            animator.SetBool(Anim.SHOULDER_CHARGE, Input.GetButtonDown(DASH));
        }

        private struct Anim
        {
            public static readonly int SHOULDER_CHARGE = Animator.StringToHash("Shoulder Charge");
        }
    }
}