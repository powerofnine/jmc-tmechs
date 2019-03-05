using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        public Rewired.Player Input => PlayerMovement.Input;
        
        private Animator animator;

        //TODO state engine
        private float shoulderCharge;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            animator.SetBool(Anim.SHOULDER_CHARGE, Input.GetButtonDown(SHOULDER_CHARGE));
        }

        private struct Anim
        {
            public static readonly int SHOULDER_CHARGE = Animator.StringToHash("Shoulder Charge");
        }
    }
}