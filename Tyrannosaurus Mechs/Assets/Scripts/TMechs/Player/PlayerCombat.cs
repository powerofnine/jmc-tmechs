using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        public Rewired.Player Input => PlayerMovement.Input;

        //TODO state engine
        private float shoulderCharge;
        
        private void Update()
        {
            if (shoulderCharge <= Mathf.Epsilon && Input.GetButtonDown(SHOULDER_CHARGE))
                shoulderCharge = .25F;

            if (shoulderCharge > 0F)
            {
                shoulderCharge -= Time.deltaTime;
                transform.Translate(Vector3.forward * 30F * Time.deltaTime);
            }
        }
    }
}