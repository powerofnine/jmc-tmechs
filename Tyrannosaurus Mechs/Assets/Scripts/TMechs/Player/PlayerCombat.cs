using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        public Rewired.Player Input => PlayerMovement.Input;

        [HideInInspector]
        public bool isLockedOn = false;
        
        private Animator animator;

        //TODO state engine
        private float shoulderCharge;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetButtonDown(LOCK_ON))
                isLockedOn = !isLockedOn;
            
            animator.SetBool(Anim.SHOULDER_CHARGE, Input.GetButtonDown(DASH));
        }

        private struct Anim
        {
            public static readonly int SHOULDER_CHARGE = Animator.StringToHash("Shoulder Charge");
        }
    }
}